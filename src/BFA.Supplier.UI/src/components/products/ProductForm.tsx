"use client";

import { ApiError, apiFetch, uploadMedia } from "@/lib/api";
import { getSupplierId } from "@/lib/supplier-session";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";

type Category = { id: string; name: string; slug: string };

type LocaleFields = {
  name: string;
  shortDescription: string;
  description: string;
};

type LocaleTab = "en" | "hy";

type ProductFormData = {
  ingredients: string;
  price: string;
  currency: string;
  categoryId: string;
  supplierSku: string;
  variantWeight: string;
  variantSize: string;
  variantColor: string;
  countryOfOrigin: string;
  imageStorageKey: string;
  netWeight: string;
  grossWeight: string;
  packageLength: string;
  packageWidth: string;
  packageHeight: string;
  isFragile: boolean;
};

export type ProductFormInitial = Partial<ProductFormData> & {
  en?: LocaleFields;
  hy?: LocaleFields;
};

const EMPTY_LOCALE: LocaleFields = {
  name: "",
  shortDescription: "",
  description: "",
};

const emptyForm: ProductFormData = {
  ingredients: "",
  price: "",
  currency: "USD",
  categoryId: "",
  supplierSku: "",
  variantWeight: "",
  variantSize: "",
  variantColor: "",
  countryOfOrigin: "AM",
  imageStorageKey: "",
  netWeight: "",
  grossWeight: "",
  packageLength: "",
  packageWidth: "",
  packageHeight: "",
  isFragile: false,
};

const inputStyle: React.CSSProperties = {
  border: "1px solid #e2e8f0",
  borderRadius: 8,
  padding: "10px 12px",
  width: "100%",
  minWidth: 0,
};

export function ProductForm({
  productId,
  initial,
  initialImageUrl,
  onSuccess,
  onCancel,
}: {
  productId?: string;
  initial?: ProductFormInitial;
  initialImageUrl?: string;
  onSuccess?: (productId: string) => void;
  onCancel?: () => void;
}) {
  const router = useRouter();
  const isEmbedded = Boolean(onSuccess || onCancel);
  const [localeTab, setLocaleTab] = useState<LocaleTab>("en");
  const [en, setEn] = useState<LocaleFields>(initial?.en ?? EMPTY_LOCALE);
  const [hy, setHy] = useState<LocaleFields>(initial?.hy ?? EMPTY_LOCALE);
  const [form, setForm] = useState<ProductFormData>({
    ...emptyForm,
    ingredients: initial?.ingredients ?? "",
    price: initial?.price ?? "",
    currency: initial?.currency ?? "USD",
    categoryId: initial?.categoryId ?? "",
    supplierSku: initial?.supplierSku ?? "",
    variantWeight: initial?.variantWeight ?? "",
    variantSize: initial?.variantSize ?? "",
    variantColor: initial?.variantColor ?? "",
    countryOfOrigin: initial?.countryOfOrigin ?? "AM",
    imageStorageKey: initial?.imageStorageKey ?? "",
    netWeight: initial?.netWeight ?? "",
    grossWeight: initial?.grossWeight ?? "",
    packageLength: initial?.packageLength ?? "",
    packageWidth: initial?.packageWidth ?? "",
    packageHeight: initial?.packageHeight ?? "",
    isFragile: initial?.isFragile ?? false,
  });
  const [imagePreviewUrl, setImagePreviewUrl] = useState(initialImageUrl ?? "");
  const [isUploadingImage, setIsUploadingImage] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isAiEnabled, setIsAiEnabled] = useState(false);
  const [aiBusyField, setAiBusyField] = useState<"" | "shortDescription" | "description">("");

  useEffect(() => {
    void apiFetch<Category[]>("/api/categories")
      .then(setCategories)
      .catch(() => setCategories([]));
    void apiFetch<{ enabled: boolean }>("/api/products/ai/enabled")
      .then((result) => setIsAiEnabled(result.enabled))
      .catch(() => setIsAiEnabled(false));
  }, []);

  function updateField<K extends keyof ProductFormData>(key: K, value: ProductFormData[K]) {
    setForm((prev) => ({ ...prev, [key]: value }));
  }

  function updateLocale(locale: LocaleTab, patch: Partial<LocaleFields>) {
    if (locale === "en") {
      setEn((current) => ({ ...current, ...patch }));
    } else {
      setHy((current) => ({ ...current, ...patch }));
    }
  }

  async function generateProductCopy(field: "shortDescription" | "description") {
    const current = localeTab === "en" ? en : hy;
    if (!current.name.trim()) {
      setError(`Enter the ${localeTab === "en" ? "English" : "Armenian"} product name first.`);
      return;
    }

    setError("");
    setAiBusyField(field);
    try {
      const result = await apiFetch<{ text: string }>("/api/products/ai/generate", {
        method: "POST",
        body: JSON.stringify({
          languageCode: localeTab,
          field,
          productName: current.name,
          shortDescription: current.shortDescription || null,
          description: current.description || null,
        }),
      });
      updateLocale(localeTab, { [field]: result.text });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not generate product copy.");
    } finally {
      setAiBusyField("");
    }
  }

  function renderAiButton(field: "shortDescription" | "description") {
    if (!isAiEnabled) return null;
    const current = localeTab === "en" ? en : hy;
    const hasText = current[field].trim().length > 0;
    const isBusy = aiBusyField === field;
    return (
      <button
        type="button"
        disabled={aiBusyField !== ""}
        title={hasText ? "Polish the existing text with AI" : "Generate text from the product name"}
        onClick={() => void generateProductCopy(field)}
        style={{
          border: "1px solid #e2e8f0",
          borderRadius: 6,
          background: "#fff",
          color: "#475569",
          fontSize: 12,
          padding: "3px 10px",
          cursor: aiBusyField !== "" ? "default" : "pointer",
        }}
      >
        {isBusy ? "Writing…" : hasText ? "Polish with AI" : "Generate with AI"}
      </button>
    );
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    const supplierId = getSupplierId();
    if (!supplierId) {
      setError("Complete onboarding first to get a supplier account.");
      return;
    }

    if (!en.name.trim() || !en.description.trim()) {
      setError("English name and description are required.");
      setLocaleTab("en");
      return;
    }

    setError("");
    setIsSubmitting(true);

    const translations = [
      {
        languageCode: "en",
        name: en.name.trim(),
        shortDescription: en.shortDescription.trim(),
        description: en.description.trim(),
      },
    ];

    if (hy.name.trim()) {
      translations.push({
        languageCode: "hy",
        name: hy.name.trim(),
        shortDescription: hy.shortDescription.trim(),
        description: hy.description.trim(),
      });
    }

    const payload = {
      supplierId,
      price: Number(form.price),
      currency: form.currency,
      categoryId: form.categoryId || null,
      ingredients: form.ingredients,
      supplierSku: form.supplierSku || null,
      variantWeight: form.variantWeight ? Number(form.variantWeight) : null,
      variantSize: form.variantSize || null,
      variantColor: form.variantColor || null,
      countryOfOrigin: form.countryOfOrigin,
      imageStorageKey: form.imageStorageKey || null,
      netWeight: form.netWeight ? Number(form.netWeight) : null,
      grossWeight: form.grossWeight ? Number(form.grossWeight) : null,
      packageLength: form.packageLength ? Number(form.packageLength) : null,
      packageWidth: form.packageWidth ? Number(form.packageWidth) : null,
      packageHeight: form.packageHeight ? Number(form.packageHeight) : null,
      isFragile: form.isFragile,
      translations,
    };

    try {
      if (productId) {
        await apiFetch(`/api/products/${productId}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
        if (onSuccess) {
          onSuccess(productId);
        } else {
          router.push("/products");
        }
      } else {
        const result = await apiFetch<{ id: string }>("/api/products", {
          method: "POST",
          body: JSON.stringify(payload),
        });
        if (onSuccess) {
          onSuccess(result.id);
        } else {
          router.push(`/products/${result.id}`);
        }
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save product.");
    } finally {
      setIsSubmitting(false);
    }
  }

  const activeLocale = localeTab === "en" ? en : hy;

  return (
    <form
      onSubmit={handleSubmit}
      className={isEmbedded ? undefined : "supplier-card"}
      style={isEmbedded ? undefined : { maxWidth: 800 }}
    >
      {error ? <div style={{ color: "#b91c1c", marginBottom: 16, fontSize: 14 }}>{error}</div> : null}

      <h2 style={{ margin: "0 0 16px", fontSize: 16 }}>Basic information</h2>

      <div className="locale-tabs" role="tablist" aria-label="Product language">
        <button
          type="button"
          role="tab"
          aria-selected={localeTab === "en"}
          className={`locale-tab${localeTab === "en" ? " active" : ""}`}
          onClick={() => setLocaleTab("en")}
        >
          English
        </button>
        <button
          type="button"
          role="tab"
          aria-selected={localeTab === "hy"}
          className={`locale-tab${localeTab === "hy" ? " active" : ""}`}
          onClick={() => setLocaleTab("hy")}
        >
          Armenian
        </button>
      </div>

      <div className="locale-panel" role="tabpanel">
        <p className="locale-hint" style={{ margin: "0 0 12px" }}>
          {localeTab === "en"
            ? "English is required and used as the default storefront language."
            : "Armenian is optional. Leave empty to fall back to English on the public site."}
        </p>
        <div style={{ display: "grid", gap: 14 }}>
          <label style={{ display: "grid", gap: 6 }}>
            <span style={{ fontSize: 13, fontWeight: 500 }}>
              Product name {localeTab === "en" ? "*" : ""}
            </span>
            <input
              required={localeTab === "en"}
              value={activeLocale.name}
              onChange={(e) => updateLocale(localeTab, { name: e.target.value })}
              style={inputStyle}
            />
          </label>
          <label style={{ display: "grid", gap: 6 }}>
            <span
              style={{
                fontSize: 13,
                fontWeight: 500,
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                gap: 8,
              }}
            >
              Short description
              {renderAiButton("shortDescription")}
            </span>
            <input
              value={activeLocale.shortDescription}
              onChange={(e) =>
                updateLocale(localeTab, { shortDescription: e.target.value })
              }
              style={inputStyle}
            />
          </label>
          <label style={{ display: "grid", gap: 6 }}>
            <span
              style={{
                fontSize: 13,
                fontWeight: 500,
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                gap: 8,
              }}
            >
              <span>Description {localeTab === "en" ? "*" : ""}</span>
              {renderAiButton("description")}
            </span>
            <textarea
              required={localeTab === "en"}
              rows={4}
              value={activeLocale.description}
              onChange={(e) => updateLocale(localeTab, { description: e.target.value })}
              style={{ ...inputStyle, resize: "vertical" }}
            />
          </label>
        </div>
      </div>

      <div style={{ display: "grid", gap: 14, margin: "24px 0" }}>
        <label style={{ display: "grid", gap: 6 }}>
          <span style={{ fontSize: 13, fontWeight: 500 }}>Ingredients / composition</span>
          <textarea
            rows={2}
            value={form.ingredients}
            onChange={(e) => updateField("ingredients", e.target.value)}
            style={{ ...inputStyle, resize: "vertical" }}
          />
        </label>
        <div style={{ display: "grid", gridTemplateColumns: "minmax(0, 1fr) 120px", gap: 12 }}>
          <label style={{ display: "grid", gap: 6 }}>
            <span style={{ fontSize: 13, fontWeight: 500 }}>Price *</span>
            <input
              required
              type="number"
              min="0"
              step="0.01"
              value={form.price}
              onChange={(e) => updateField("price", e.target.value)}
              style={inputStyle}
            />
          </label>
          <label style={{ display: "grid", gap: 6 }}>
            <span style={{ fontSize: 13, fontWeight: 500 }}>Currency</span>
            <input
              value={form.currency}
              onChange={(e) => updateField("currency", e.target.value)}
              style={inputStyle}
            />
          </label>
        </div>
        <label style={{ display: "grid", gap: 6 }}>
          <span style={{ fontSize: 13, fontWeight: 500 }}>Category</span>
          <select
            value={form.categoryId}
            onChange={(e) => updateField("categoryId", e.target.value)}
            style={inputStyle}
          >
            <option value="">Select category</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </label>
        <label style={{ display: "grid", gap: 6 }}>
          <span style={{ fontSize: 13, fontWeight: 500 }}>Primary image</span>
          <input
            type="file"
            accept="image/jpeg,image/png,image/webp,image/gif"
            disabled={isUploadingImage || isSubmitting}
            onChange={(e) => {
              const file = e.target.files?.[0];
              const supplierId = getSupplierId();
              if (!file || !supplierId) return;
              setIsUploadingImage(true);
              setError("");
              void uploadMedia(file, {
                supplierId,
                ...(productId ? { productId } : {}),
                isPrimary: "true",
              })
                .then((result) => {
                  updateField("imageStorageKey", result.storageKey);
                  setImagePreviewUrl(result.url);
                })
                .catch((err) => {
                  setError(
                    err instanceof ApiError
                      ? err.message
                      : "Unable to upload image.",
                  );
                })
                .finally(() => setIsUploadingImage(false));
            }}
            style={inputStyle}
          />
          {isUploadingImage ? (
            <span style={{ fontSize: 13, color: "#64748b" }}>Uploading…</span>
          ) : null}
          {imagePreviewUrl ? (
            <img
              src={imagePreviewUrl}
              alt=""
              style={{ marginTop: 8, maxWidth: 180, borderRadius: 8 }}
            />
          ) : null}
        </label>
      </div>

      <h2 style={{ margin: "0 0 16px", fontSize: 16 }}>Variant (SKU)</h2>
      <div
        style={{
          display: "grid",
          gap: 14,
          marginBottom: 24,
          gridTemplateColumns: "repeat(auto-fit, minmax(160px, 1fr))",
        }}
      >
        <label style={{ display: "grid", gap: 6 }}>
          <span style={{ fontSize: 13, fontWeight: 500 }}>SKU</span>
          <input
            value={form.supplierSku}
            onChange={(e) => updateField("supplierSku", e.target.value)}
            style={inputStyle}
          />
        </label>
        <label style={{ display: "grid", gap: 6 }}>
          <span style={{ fontSize: 13, fontWeight: 500 }}>Weight (kg)</span>
          <input
            type="number"
            step="0.001"
            value={form.variantWeight}
            onChange={(e) => updateField("variantWeight", e.target.value)}
            style={inputStyle}
          />
        </label>
        <label style={{ display: "grid", gap: 6 }}>
          <span style={{ fontSize: 13, fontWeight: 500 }}>Size</span>
          <input
            value={form.variantSize}
            onChange={(e) => updateField("variantSize", e.target.value)}
            style={inputStyle}
          />
        </label>
        <label style={{ display: "grid", gap: 6 }}>
          <span style={{ fontSize: 13, fontWeight: 500 }}>Color</span>
          <input
            value={form.variantColor}
            onChange={(e) => updateField("variantColor", e.target.value)}
            style={inputStyle}
          />
        </label>
      </div>

      <h2 style={{ margin: "0 0 16px", fontSize: 16 }}>Shipping profile</h2>
      <div
        style={{
          display: "grid",
          gap: 14,
          marginBottom: 24,
          gridTemplateColumns: "repeat(auto-fit, minmax(130px, 1fr))",
        }}
      >
        {(
          [
            ["netWeight", "Net weight (kg)"],
            ["grossWeight", "Gross weight (kg)"],
            ["packageLength", "Length (cm)"],
            ["packageWidth", "Width (cm)"],
            ["packageHeight", "Height (cm)"],
          ] as const
        ).map(([key, label]) => (
          <label key={key} style={{ display: "grid", gap: 6 }}>
            <span style={{ fontSize: 13, fontWeight: 500 }}>{label}</span>
            <input
              type="number"
              step="0.01"
              value={form[key]}
              onChange={(e) => updateField(key, e.target.value)}
              style={inputStyle}
            />
          </label>
        ))}
        <label style={{ display: "flex", alignItems: "center", gap: 8, marginTop: 24 }}>
          <input
            type="checkbox"
            checked={form.isFragile}
            onChange={(e) => updateField("isFragile", e.target.checked)}
          />
          <span style={{ fontSize: 13 }}>Fragile</span>
        </label>
      </div>

      <div style={{ display: "flex", gap: 12 }}>
        <button type="submit" className="button-primary" disabled={isSubmitting}>
          {isSubmitting ? "Saving..." : productId ? "Save changes" : "Save as draft"}
        </button>
        {onCancel ? (
          <button type="button" className="button-secondary" onClick={onCancel}>
            Cancel
          </button>
        ) : (
          <Link
            href="/products"
            className="button-secondary"
            style={{ display: "inline-flex", alignItems: "center" }}
          >
            Cancel
          </Link>
        )}
      </div>
    </form>
  );
}
