"use client";

import { ApiError, apiFetch } from "@/lib/api";
import { getSupplierId } from "@/lib/supplier-session";
import { useCallback, useEffect, useState } from "react";

type ProductSubpanelTab = "variants" | "shipping";

type ProductVariant = {
  id: string;
  supplierSku: string;
  size?: string | null;
  color?: string | null;
  weight: number;
  countryOfOrigin: string;
};

type ProductShipping = {
  netWeight: number;
  grossWeight: number;
  packageLength: number;
  packageWidth: number;
  packageHeight: number;
  packageDimensionUnit: string;
  isFragile: boolean;
  isPerishable: boolean;
};

type ProductExtras = {
  id: string;
  variants: ProductVariant[];
  shipping?: ProductShipping | null;
};

type VariantFormState = {
  id?: string;
  supplierSku: string;
  size: string;
  color: string;
  weight: string;
  countryOfOrigin: string;
};

const EMPTY_VARIANT_FORM: VariantFormState = {
  supplierSku: "",
  size: "",
  color: "",
  weight: "0.5",
  countryOfOrigin: "AM",
};

const TABS: Array<{ id: ProductSubpanelTab; label: string }> = [
  { id: "variants", label: "Variants" },
  { id: "shipping", label: "Shipping profile" },
];

type ProductSubpanelsProps = {
  productId: string;
  defaultTab?: ProductSubpanelTab;
};

export function ProductSubpanels({
  productId,
  defaultTab = "variants",
}: ProductSubpanelsProps) {
  const [tab, setTab] = useState<ProductSubpanelTab>(defaultTab);
  const [data, setData] = useState<ProductExtras | null>(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [busyAction, setBusyAction] = useState("");
  const [variantForm, setVariantForm] = useState<VariantFormState | null>(null);
  const [variantSaving, setVariantSaving] = useState(false);
  const [variantError, setVariantError] = useState("");

  const load = useCallback(async () => {
    const supplierId = getSupplierId();
    if (!supplierId) {
      setError("Complete onboarding first.");
      return;
    }

    setIsLoading(true);
    setError("");
    try {
      const product = await apiFetch<ProductExtras>(
        `/api/products/${productId}?supplierId=${supplierId}`,
      );
      setData(product);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load product details.");
      setData(null);
    } finally {
      setIsLoading(false);
    }
  }, [productId]);

  useEffect(() => {
    void load();
  }, [load]);

  async function deleteVariant(variant: ProductVariant) {
    if (!data || data.variants.length <= 1) {
      setError("Cannot delete the last variant. Add another variant first.");
      return;
    }

    const supplierId = getSupplierId();
    if (!supplierId) return;

    const confirmed = window.confirm(
      `Delete variant ${variant.supplierSku}? Related stock (if any) will also be removed.`,
    );
    if (!confirmed) return;

    setBusyAction(variant.id);
    setError("");
    try {
      await apiFetch(
        `/api/products/${productId}/variants/${variant.id}?supplierId=${supplierId}`,
        { method: "DELETE" },
      );
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete variant.");
    } finally {
      setBusyAction("");
    }
  }

  async function clearShipping() {
    const supplierId = getSupplierId();
    if (!supplierId) return;

    const confirmed = window.confirm("Remove the shipping profile from this product?");
    if (!confirmed) return;

    setBusyAction("shipping");
    setError("");
    try {
      await apiFetch(`/api/products/${productId}/shipping?supplierId=${supplierId}`, {
        method: "DELETE",
      });
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to clear shipping profile.");
    } finally {
      setBusyAction("");
    }
  }

  async function saveVariant() {
    const supplierId = getSupplierId();
    if (!supplierId || !variantForm) return;

    if (variantForm.id && !variantForm.supplierSku.trim()) {
      setVariantError("SKU is required.");
      return;
    }

    const weight = Number(variantForm.weight);
    if (!Number.isFinite(weight) || weight <= 0) {
      setVariantError("Weight must be greater than 0.");
      return;
    }

    setVariantSaving(true);
    setVariantError("");
    try {
      if (variantForm.id) {
        setVariantError("Edit variant from the product Edit form for now.");
        setVariantSaving(false);
        return;
      }

      await apiFetch(`/api/products/${productId}/variants`, {
        method: "POST",
        body: JSON.stringify({
          supplierId,
          supplierSku: variantForm.supplierSku.trim() || null,
          weight,
          countryOfOrigin: variantForm.countryOfOrigin.trim() || "AM",
          size: variantForm.size.trim() || null,
          color: variantForm.color.trim() || null,
        }),
      });
      setVariantForm(null);
      await load();
    } catch (err) {
      if (err instanceof ApiError) {
        setVariantError(err.message);
      } else if (err instanceof TypeError) {
        setVariantError("Cannot reach Supplier API. Is it running on :5102?");
      } else {
        setVariantError("Failed to save variant.");
      }
    } finally {
      setVariantSaving(false);
    }
  }

  return (
    <div className="product-subpanels">
      <div className="detail-tabs" role="tablist" aria-label="Product sections">
        {TABS.map((item) => (
          <button
            key={item.id}
            type="button"
            role="tab"
            aria-selected={tab === item.id}
            className={`detail-tab${tab === item.id ? " active" : ""}`}
            onClick={() => setTab(item.id)}
          >
            {item.label}
          </button>
        ))}
      </div>

      {error ? <p style={{ color: "#b91c1c", fontSize: 13 }}>{error}</p> : null}
      {isLoading ? <p style={{ color: "var(--supplier-muted)", fontSize: 13 }}>Loading…</p> : null}

      {!isLoading && data && tab === "variants" ? (
        <div>
          <div className="product-subpanels-toolbar">
            <span style={{ color: "var(--supplier-muted)", fontSize: 13 }}>
              {data.variants.length} variant{data.variants.length === 1 ? "" : "s"}
            </span>
            <button
              type="button"
              className="button-primary"
              disabled={Boolean(busyAction)}
              onClick={() => {
                setVariantForm(EMPTY_VARIANT_FORM);
                setVariantError("");
              }}
            >
              Add variant
            </button>
          </div>

          {variantForm && !variantForm.id ? (
            <div
              style={{
                border: "1px solid var(--supplier-border)",
                borderRadius: 10,
                padding: 12,
                marginBottom: 12,
                background: "#fff",
              }}
            >
              {variantError ? (
                <p style={{ color: "#b91c1c", fontSize: 13 }}>{variantError}</p>
              ) : null}
              <div style={{ display: "grid", gap: 8, gridTemplateColumns: "1fr 1fr" }}>
                <label style={{ fontSize: 13 }}>
                  SKU
                  <input
                    value={variantForm.supplierSku}
                    placeholder="Leave empty to auto-generate"
                    onChange={(e) =>
                      setVariantForm((current) =>
                        current ? { ...current, supplierSku: e.target.value } : current,
                      )
                    }
                    style={{ display: "block", width: "100%", marginTop: 4 }}
                  />
                </label>
                <label style={{ fontSize: 13 }}>
                  Weight (kg)
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    value={variantForm.weight}
                    onChange={(e) =>
                      setVariantForm((current) =>
                        current ? { ...current, weight: e.target.value } : current,
                      )
                    }
                    style={{ display: "block", width: "100%", marginTop: 4 }}
                  />
                </label>
                <label style={{ fontSize: 13 }}>
                  Size
                  <input
                    value={variantForm.size}
                    onChange={(e) =>
                      setVariantForm((current) =>
                        current ? { ...current, size: e.target.value } : current,
                      )
                    }
                    style={{ display: "block", width: "100%", marginTop: 4 }}
                  />
                </label>
                <label style={{ fontSize: 13 }}>
                  Color
                  <input
                    value={variantForm.color}
                    onChange={(e) =>
                      setVariantForm((current) =>
                        current ? { ...current, color: e.target.value } : current,
                      )
                    }
                    style={{ display: "block", width: "100%", marginTop: 4 }}
                  />
                </label>
              </div>
              <div style={{ display: "flex", gap: 8, marginTop: 12 }}>
                <button
                  type="button"
                  className="button-primary"
                  disabled={variantSaving}
                  onClick={() => void saveVariant()}
                >
                  {variantSaving ? "Saving..." : "Save variant"}
                </button>
                <button
                  type="button"
                  className="button-ghost"
                  disabled={variantSaving}
                  onClick={() => setVariantForm(null)}
                >
                  Cancel
                </button>
              </div>
            </div>
          ) : null}

          <div className="supplier-table-wrap">
            <table className="supplier-table">
              <thead>
                <tr>
                  <th>SKU</th>
                  <th>Size</th>
                  <th>Color</th>
                  <th>Weight</th>
                  <th>Origin</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {data.variants.length === 0 ? (
                  <tr>
                    <td colSpan={6} style={{ color: "var(--supplier-muted)" }}>
                      No variants yet.
                    </td>
                  </tr>
                ) : null}
                {data.variants.map((variant) => (
                  <tr key={variant.id}>
                    <td>{variant.supplierSku}</td>
                    <td>{variant.size ?? "—"}</td>
                    <td>{variant.color ?? "—"}</td>
                    <td>{variant.weight}</td>
                    <td>{variant.countryOfOrigin}</td>
                    <td>
                      <button
                        type="button"
                        className="button-ghost"
                        disabled={Boolean(busyAction) || data.variants.length <= 1}
                        title={
                          data.variants.length <= 1
                            ? "Cannot delete the last variant"
                            : undefined
                        }
                        onClick={() => void deleteVariant(variant)}
                      >
                        {busyAction === variant.id ? "…" : "Delete"}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : null}

      {!isLoading && data && tab === "shipping" ? (
        <div>
          {data.shipping ? (
            <>
              <div className="product-subpanels-toolbar">
                <span style={{ color: "var(--supplier-muted)", fontSize: 13 }}>
                  Package dimensions and handling flags
                </span>
                <button
                  type="button"
                  className="button-ghost"
                  disabled={Boolean(busyAction)}
                  onClick={() => void clearShipping()}
                >
                  {busyAction === "shipping" ? "…" : "Delete profile"}
                </button>
              </div>
              <div className="supplier-table-wrap">
                <table className="supplier-table">
                  <thead>
                    <tr>
                      <th>Net (kg)</th>
                      <th>Gross (kg)</th>
                      <th>L</th>
                      <th>W</th>
                      <th>H</th>
                      <th>Fragile</th>
                      <th>Perishable</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>{data.shipping.netWeight}</td>
                      <td>{data.shipping.grossWeight}</td>
                      <td>{data.shipping.packageLength}</td>
                      <td>{data.shipping.packageWidth}</td>
                      <td>{data.shipping.packageHeight}</td>
                      <td>{data.shipping.isFragile ? "Yes" : "No"}</td>
                      <td>{data.shipping.isPerishable ? "Yes" : "No"}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </>
          ) : (
            <p style={{ color: "var(--supplier-muted)", fontSize: 13, margin: 0 }}>
              Shipping profile is not set. Add it when editing the product.
            </p>
          )}
        </div>
      ) : null}
    </div>
  );
}
