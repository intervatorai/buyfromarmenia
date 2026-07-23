"use client";

import { FormEvent, useEffect, useState } from "react";
import { ApiError, apiFetch, uploadMedia } from "@/lib/api";
import { Modal } from "./Modal";

type SupplierOption = {
  id: string;
  tradingName: string;
  status: string;
};

type CategoryOption = {
  id: string;
  name: string;
  skuPrefix?: string;
};

type ProductTranslationSeed = {
  languageCode: string;
  name: string;
  shortDescription: string;
  description: string;
};

type ProductVariantSeed = {
  id: string;
  supplierSku: string;
  size?: string | null;
  color?: string | null;
  weight: number;
  countryOfOrigin: string;
};

type ProductSeed = {
  id: string;
  name: string;
  shortDescription: string;
  description: string;
  price: number;
  currency: string;
  categoryId?: string | null;
  status?: string;
  tag?: string | null;
  translations?: ProductTranslationSeed[];
  variants?: ProductVariantSeed[];
};

type LocaleFields = {
  name: string;
  shortDescription: string;
  description: string;
};

type LocaleTab = "en" | "hy";

const PRODUCT_TAGS = [
  { value: "None", label: "No tag" },
  { value: "Popular", label: "Popular" },
  { value: "Bestseller", label: "Bestseller" },
  { value: "New", label: "New" },
] as const;

const EMPTY_LOCALE: LocaleFields = {
  name: "",
  shortDescription: "",
  description: "",
};

type ProductFormModalProps = {
  open: boolean;
  productId?: string | null;
  onClose: () => void;
  onSaved: (productId: string) => void;
};

export function ProductFormModal({ open, productId, onClose, onSaved }: ProductFormModalProps) {
  const isEdit = Boolean(productId);
  const [suppliers, setSuppliers] = useState<SupplierOption[]>([]);
  const [categories, setCategories] = useState<CategoryOption[]>([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [localeTab, setLocaleTab] = useState<LocaleTab>("en");

  const [supplierId, setSupplierId] = useState("");
  const [en, setEn] = useState<LocaleFields>(EMPTY_LOCALE);
  const [hy, setHy] = useState<LocaleFields>(EMPTY_LOCALE);
  const [price, setPrice] = useState("19.99");
  const [currency, setCurrency] = useState("USD");
  const [categoryId, setCategoryId] = useState("");
  const [supplierSku, setSupplierSku] = useState("");
  const [variantWeight, setVariantWeight] = useState("0.5");
  const [variantSize, setVariantSize] = useState("");
  const [variantColor, setVariantColor] = useState("");
  const [countryOfOrigin, setCountryOfOrigin] = useState("AM");
  const [imageStorageKey, setImageStorageKey] = useState("");
  const [imagePreviewUrl, setImagePreviewUrl] = useState("");
  const [isUploadingImage, setIsUploadingImage] = useState(false);
  const [publishImmediately, setPublishImmediately] = useState(true);
  const [status, setStatus] = useState("");
  const [tag, setTag] = useState("None");

  useEffect(() => {
    if (!open) {
      return;
    }

    async function load() {
      setIsLoading(true);
      setError("");
      setLocaleTab("en");
      try {
        const [supplierList, categoryList] = await Promise.all([
          apiFetch<SupplierOption[]>("/api/suppliers?status=Active"),
          apiFetch<CategoryOption[]>("/api/categories"),
        ]);
        setSuppliers(supplierList);
        setCategories(categoryList);

        if (productId) {
          const product = await apiFetch<ProductSeed>(`/api/products/${productId}`);
          const enTranslation = product.translations?.find((item) => item.languageCode === "en");
          const hyTranslation = product.translations?.find((item) => item.languageCode === "hy");

          setEn({
            name: enTranslation?.name ?? product.name ?? "",
            shortDescription:
              enTranslation?.shortDescription ?? product.shortDescription ?? "",
            description: enTranslation?.description ?? product.description ?? "",
          });
          setHy({
            name: hyTranslation?.name ?? "",
            shortDescription: hyTranslation?.shortDescription ?? "",
            description: hyTranslation?.description ?? "",
          });
          setPrice(String(product.price));
          setCurrency(product.currency);
          setCategoryId(product.categoryId ?? "");
          setStatus(product.status ?? "");
          setTag(product.tag && product.tag !== "None" ? product.tag : "None");
          const firstVariant = product.variants?.[0];
          setSupplierSku(firstVariant?.supplierSku ?? "");
          setVariantWeight(
            firstVariant?.weight != null ? String(firstVariant.weight) : "0.5",
          );
          setVariantSize(firstVariant?.size ?? "");
          setVariantColor(firstVariant?.color ?? "");
          setCountryOfOrigin(firstVariant?.countryOfOrigin ?? "AM");
        } else {
          setSupplierId(supplierList[0]?.id ?? "");
          setEn(EMPTY_LOCALE);
          setHy(EMPTY_LOCALE);
          setPrice("19.99");
          setCurrency("USD");
          setCategoryId("");
          setSupplierSku("");
          setVariantWeight("0.5");
          setVariantSize("");
          setVariantColor("");
          setCountryOfOrigin("AM");
          setImageStorageKey("");
          setImagePreviewUrl("");
          setIsUploadingImage(false);
          setPublishImmediately(true);
          setStatus("");
          setTag("None");
        }
      } catch (err) {
        setError(err instanceof ApiError ? err.message : "Failed to load form.");
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [open, productId]);

  function updateLocale(locale: LocaleTab, patch: Partial<LocaleFields>) {
    if (locale === "en") {
      setEn((current) => ({ ...current, ...patch }));
    } else {
      setHy((current) => ({ ...current, ...patch }));
    }
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setIsSaving(true);
    setError("");

    if (!en.name.trim() || !en.description.trim()) {
      setError("English name and description are required.");
      setLocaleTab("en");
      setIsSaving(false);
      return;
    }

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

    try {
      if (isEdit && productId) {
        await apiFetch(`/api/products/${productId}`, {
          method: "PUT",
          body: JSON.stringify({
            price: Number(price),
            currency,
            categoryId: categoryId || null,
            tag,
            translations,
          }),
        });
        onSaved(productId);
      } else {
        const result = await apiFetch<{ id: string }>("/api/products", {
          method: "POST",
          body: JSON.stringify({
            supplierId,
            price: Number(price),
            currency,
            categoryId: categoryId || null,
            supplierSku: supplierSku.trim() || null,
            variantWeight: Number(variantWeight) || 0.5,
            variantSize: variantSize.trim() || null,
            variantColor: variantColor.trim() || null,
            countryOfOrigin: countryOfOrigin.trim() || "AM",
            imageStorageKey: imageStorageKey || null,
            publishImmediately,
            tag,
            translations,
          }),
        });
        onSaved(result.id);
      }
      onClose();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save product.");
    } finally {
      setIsSaving(false);
    }
  }

  const activeLocale = localeTab === "en" ? en : hy;

  return (
    <Modal
      open={open}
      title={isEdit ? "Edit product" : "Add product"}
      onClose={onClose}
      wide
      footer={
        <>
          <button type="button" className="button-ghost" onClick={onClose} disabled={isSaving}>
            Cancel
          </button>
          <button
            type="submit"
            form="product-form-modal"
            className="button-primary"
            disabled={isSaving || isLoading || (!isEdit && !supplierId)}
          >
            {isSaving ? "Saving..." : isEdit ? "Save changes" : "Create product"}
          </button>
        </>
      }
    >
      {isLoading ? <p>Loading...</p> : null}
      {error ? <p className="form-error">{error}</p> : null}

      {!isLoading ? (
        <form id="product-form-modal" onSubmit={(event) => void handleSubmit(event)}>
          {isEdit && status ? (
            <p style={{ color: "var(--admin-muted)", marginTop: 0 }}>
              Status: <strong>{status}</strong>. Admin edits do not send the product back to review.
            </p>
          ) : null}

          {!isEdit ? (
            <div className="form-field">
              <label htmlFor="product-supplierId">Supplier (owner)</label>
              <select
                id="product-supplierId"
                className="form-control"
                required
                value={supplierId}
                onChange={(e) => setSupplierId(e.target.value)}
              >
                {suppliers.map((supplier) => (
                  <option key={supplier.id} value={supplier.id}>
                    {supplier.tradingName} ({supplier.status})
                  </option>
                ))}
              </select>
            </div>
          ) : null}

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
            <p className="locale-hint">
              {localeTab === "en"
                ? "English is required and used as the default storefront language."
                : "Armenian is optional. Leave empty to fall back to English on the public site."}
            </p>

            <div className="form-field">
              <label htmlFor={`product-name-${localeTab}`}>
                Name {localeTab === "en" ? "*" : ""}
              </label>
              <input
                id={`product-name-${localeTab}`}
                required={localeTab === "en"}
                value={activeLocale.name}
                onChange={(e) => updateLocale(localeTab, { name: e.target.value })}
              />
            </div>

            <div className="form-field">
              <label htmlFor={`product-shortDescription-${localeTab}`}>Short description</label>
              <input
                id={`product-shortDescription-${localeTab}`}
                value={activeLocale.shortDescription}
                onChange={(e) =>
                  updateLocale(localeTab, { shortDescription: e.target.value })
                }
              />
            </div>

            <div className="form-field">
              <label htmlFor={`product-description-${localeTab}`}>
                Description {localeTab === "en" ? "*" : ""}
              </label>
              <textarea
                id={`product-description-${localeTab}`}
                className="form-control"
                required={localeTab === "en"}
                rows={5}
                value={activeLocale.description}
                onChange={(e) => updateLocale(localeTab, { description: e.target.value })}
              />
            </div>
          </div>

          <div className="form-row-2">
            <div className="form-field">
              <label htmlFor="product-price">Price</label>
              <input
                id="product-price"
                type="number"
                step="0.01"
                min="0"
                required
                value={price}
                onChange={(e) => setPrice(e.target.value)}
              />
            </div>
            <div className="form-field">
              <label htmlFor="product-currency">Currency</label>
              <input
                id="product-currency"
                required
                value={currency}
                onChange={(e) => setCurrency(e.target.value)}
              />
            </div>
          </div>

          <div className="form-row-2">
            <div className="form-field">
              <label htmlFor="product-categoryId">Category</label>
              <select
                id="product-categoryId"
                className="form-control"
                value={categoryId}
                onChange={(e) => setCategoryId(e.target.value)}
              >
                <option value="">No category</option>
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-field">
              <label htmlFor="product-tag">Tag</label>
              <select
                id="product-tag"
                className="form-control"
                value={tag}
                onChange={(e) => setTag(e.target.value)}
              >
                {PRODUCT_TAGS.map((item) => (
                  <option key={item.value} value={item.value}>
                    {item.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {!isEdit ? (
            <>
              <div className="form-row-2">
                <div className="form-field">
                  <label htmlFor="product-sku">Default SKU</label>
                  <input
                    id="product-sku"
                    value={supplierSku}
                    placeholder={
                      categories.find((c) => c.id === categoryId)?.skuPrefix
                        ? `${categories.find((c) => c.id === categoryId)?.skuPrefix}-####`
                        : "Auto (e.g. JW-0001)"
                    }
                    onChange={(e) => setSupplierSku(e.target.value)}
                  />
                  <p className="form-hint">
                    Leave empty to auto-generate. Manage more variants after create via ▸
                    on the products list.
                  </p>
                </div>
                <div className="form-field">
                  <label htmlFor="product-weight">Variant weight (kg)</label>
                  <input
                    id="product-weight"
                    type="number"
                    step="0.01"
                    min="0"
                    value={variantWeight}
                    onChange={(e) => setVariantWeight(e.target.value)}
                  />
                </div>
              </div>

              <div className="form-field">
                <label htmlFor="product-image">Primary image</label>
                <input
                  id="product-image"
                  type="file"
                  accept="image/jpeg,image/png,image/webp,image/gif"
                  disabled={isUploadingImage || isSaving}
                  onChange={(e) => {
                    const file = e.target.files?.[0];
                    if (!file) return;
                    setIsUploadingImage(true);
                    setError("");
                    void uploadMedia(file)
                      .then((result) => {
                        setImageStorageKey(result.storageKey);
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
                />
                {isUploadingImage ? <p className="form-hint">Uploading…</p> : null}
                {imagePreviewUrl ? (
                  <img
                    src={imagePreviewUrl}
                    alt=""
                    style={{ marginTop: 12, maxWidth: 180, borderRadius: 8 }}
                  />
                ) : null}
                {imageStorageKey ? (
                  <p className="form-hint" style={{ marginTop: 8 }}>
                    {imageStorageKey}
                  </p>
                ) : null}
              </div>
              <label className="form-checkbox">
                <input
                  type="checkbox"
                  checked={publishImmediately}
                  onChange={(e) => setPublishImmediately(e.target.checked)}
                />
                Publish immediately (skip supplier review queue)
              </label>
            </>
          ) : (
            <p className="form-hint">
              Variants and shipping are managed from the product row expand (▸) or the
              panel on this page.
            </p>
          )}
        </form>
      ) : null}
    </Modal>
  );
}
