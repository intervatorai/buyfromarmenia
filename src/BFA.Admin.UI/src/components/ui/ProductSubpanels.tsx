"use client";

import { useCallback, useEffect, useState } from "react";
import { ApiError, apiFetch } from "@/lib/api";
import { Modal } from "./Modal";

export type ProductSubpanelTab = "variants" | "shipping";

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
  /** When false, parent controls visibility (e.g. list expand row). */
  collapsible?: boolean;
  defaultOpen?: boolean;
  defaultTab?: ProductSubpanelTab;
};

export function ProductSubpanels({
  productId,
  collapsible = true,
  defaultOpen = false,
  defaultTab = "variants",
}: ProductSubpanelsProps) {
  const [open, setOpen] = useState(defaultOpen || !collapsible);
  const [tab, setTab] = useState<ProductSubpanelTab>(defaultTab);
  const [data, setData] = useState<ProductExtras | null>(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [variantFormOpen, setVariantFormOpen] = useState(false);
  const [variantForm, setVariantForm] = useState<VariantFormState>(EMPTY_VARIANT_FORM);
  const [variantSaving, setVariantSaving] = useState(false);
  const [variantError, setVariantError] = useState("");
  const [busyAction, setBusyAction] = useState("");

  const load = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      const product = await apiFetch<ProductExtras>(`/api/products/${productId}`);
      setData(product);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load product details.");
      setData(null);
    } finally {
      setIsLoading(false);
    }
  }, [productId]);

  useEffect(() => {
    if (open) {
      void load();
    }
  }, [open, load]);

  function openAddVariant() {
    setVariantForm(EMPTY_VARIANT_FORM);
    setVariantError("");
    setVariantFormOpen(true);
  }

  function openEditVariant(variant: ProductVariant) {
    setVariantForm({
      id: variant.id,
      supplierSku: variant.supplierSku,
      size: variant.size ?? "",
      color: variant.color ?? "",
      weight: String(variant.weight),
      countryOfOrigin: variant.countryOfOrigin || "AM",
    });
    setVariantError("");
    setVariantFormOpen(true);
  }

  async function saveVariant() {
    if (!variantForm.id && !variantForm.supplierSku.trim()) {
      // auto-SKU allowed on create
    } else if (!variantForm.supplierSku.trim()) {
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
    const payload = {
      supplierSku: variantForm.supplierSku.trim() || null,
      weight,
      size: variantForm.size.trim() || null,
      color: variantForm.color.trim() || null,
      countryOfOrigin: variantForm.countryOfOrigin.trim() || "AM",
    };

    try {
      if (variantForm.id) {
        await apiFetch(`/api/products/${productId}/variants/${variantForm.id}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
      } else {
        await apiFetch(`/api/products/${productId}/variants`, {
          method: "POST",
          body: JSON.stringify(payload),
        });
      }
      setVariantFormOpen(false);
      await load();
    } catch (err) {
      if (err instanceof ApiError) {
        setVariantError(err.message);
      } else if (err instanceof TypeError) {
        setVariantError("Cannot reach Admin API. Is it running on :5101?");
      } else {
        setVariantError("Failed to save variant.");
      }
    } finally {
      setVariantSaving(false);
    }
  }

  async function deleteVariant(variant: ProductVariant) {
    if (!data || data.variants.length <= 1) {
      setError("Cannot delete the last variant. Add another variant first.");
      return;
    }

    const confirmed = window.confirm(
      `Delete variant ${variant.supplierSku}? Related stock (if any) will also be removed.`,
    );
    if (!confirmed) {
      return;
    }

    setBusyAction(variant.id);
    setError("");
    try {
      await apiFetch(`/api/products/${productId}/variants/${variant.id}`, {
        method: "DELETE",
      });
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete variant.");
    } finally {
      setBusyAction("");
    }
  }

  async function clearShipping() {
    const confirmed = window.confirm("Remove the shipping profile from this product?");
    if (!confirmed) {
      return;
    }

    setBusyAction("shipping");
    setError("");
    try {
      await apiFetch(`/api/products/${productId}/shipping`, { method: "DELETE" });
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to clear shipping profile.");
    } finally {
      setBusyAction("");
    }
  }

  const panelBody = (
    <div className="product-subpanels-body">
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

      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p className="form-hint">Loading…</p> : null}

      {!isLoading && data && tab === "variants" ? (
        <div className="detail-tab-panel">
          <div className="product-subpanels-toolbar">
            <p className="form-hint" style={{ margin: 0 }}>
              {data.variants.length} variant{data.variants.length === 1 ? "" : "s"}
            </p>
            <button
              type="button"
              className="button-primary"
              disabled={variantSaving}
              onClick={openAddVariant}
            >
              Add variant
            </button>
          </div>
          <div className="admin-table-wrap">
            <table className="admin-table">
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
                    <td colSpan={6} style={{ color: "var(--admin-muted)" }}>
                      No variants yet. Add one or leave SKU blank on create to auto-generate.
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
                      <div style={{ display: "flex", gap: 8, justifyContent: "flex-end" }}>
                        <button
                          type="button"
                          className="button-ghost"
                          disabled={variantSaving || Boolean(busyAction)}
                          onClick={() => openEditVariant(variant)}
                        >
                          Edit
                        </button>
                        <button
                          type="button"
                          className="button-ghost"
                          disabled={
                            variantSaving ||
                            Boolean(busyAction) ||
                            data.variants.length <= 1
                          }
                          title={
                            data.variants.length <= 1
                              ? "Cannot delete the last variant"
                              : undefined
                          }
                          onClick={() => void deleteVariant(variant)}
                        >
                          {busyAction === variant.id ? "…" : "Delete"}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : null}

      {!isLoading && data && tab === "shipping" ? (
        <div className="detail-tab-panel">
          {data.shipping ? (
            <>
              <div className="product-subpanels-toolbar">
                <p className="form-hint" style={{ margin: 0 }}>
                  Package dimensions and handling flags
                </p>
                <button
                  type="button"
                  className="button-ghost"
                  disabled={Boolean(busyAction)}
                  onClick={() => void clearShipping()}
                >
                  {busyAction === "shipping" ? "…" : "Delete profile"}
                </button>
              </div>
              <div className="admin-table-wrap">
                <table className="admin-table">
                  <thead>
                    <tr>
                      <th>Net weight (kg)</th>
                      <th>Gross weight (kg)</th>
                      <th>Length ({data.shipping.packageDimensionUnit})</th>
                      <th>Width ({data.shipping.packageDimensionUnit})</th>
                      <th>Height ({data.shipping.packageDimensionUnit})</th>
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
            <p className="form-hint" style={{ margin: 0 }}>
              Shipping profile is not set for this product.
            </p>
          )}
        </div>
      ) : null}
    </div>
  );

  return (
    <>
      <div className={`product-subpanels${open ? " open" : ""}`}>
        {collapsible ? (
          <button
            type="button"
            className="product-subpanels-toggle"
            aria-expanded={open}
            onClick={() => setOpen((current) => !current)}
          >
            <span className="product-subpanels-chevron" aria-hidden>
              {open ? "▾" : "▸"}
            </span>
            <span>Variants &amp; shipping</span>
            {data && open ? (
              <span className="product-subpanels-meta">
                {data.variants.length} variant{data.variants.length === 1 ? "" : "s"}
                {data.shipping ? " · shipping set" : ""}
              </span>
            ) : null}
          </button>
        ) : null}
        {open ? panelBody : null}
      </div>

      <Modal
        open={variantFormOpen}
        title={variantForm.id ? "Edit variant" : "Add variant"}
        onClose={() => {
          if (!variantSaving) {
            setVariantFormOpen(false);
          }
        }}
        footer={
          <>
            <button
              type="button"
              className="button-ghost"
              disabled={variantSaving}
              onClick={() => setVariantFormOpen(false)}
            >
              Cancel
            </button>
            <button
              type="button"
              className="button-primary"
              disabled={variantSaving}
              onClick={() => void saveVariant()}
            >
              {variantSaving ? "Saving..." : "Save"}
            </button>
          </>
        }
      >
        {variantError ? <p className="form-error">{variantError}</p> : null}
        <div className="form-field">
          <label htmlFor={`variant-sku-${productId}`}>
            SKU{variantForm.id ? " *" : ""}
          </label>
          <input
            id={`variant-sku-${productId}`}
            value={variantForm.supplierSku}
            placeholder={variantForm.id ? undefined : "Leave empty to auto-generate"}
            onChange={(e) =>
              setVariantForm((current) => ({ ...current, supplierSku: e.target.value }))
            }
          />
          {!variantForm.id ? (
            <p className="form-hint">
              Blank SKU uses the product category prefix (e.g. JW-0001).
            </p>
          ) : null}
        </div>
        <div className="form-row-2">
          <div className="form-field">
            <label htmlFor={`variant-weight-${productId}`}>Weight (kg) *</label>
            <input
              id={`variant-weight-${productId}`}
              type="number"
              step="0.01"
              min="0"
              value={variantForm.weight}
              onChange={(e) =>
                setVariantForm((current) => ({ ...current, weight: e.target.value }))
              }
            />
          </div>
          <div className="form-field">
            <label htmlFor={`variant-origin-${productId}`}>Country of origin</label>
            <input
              id={`variant-origin-${productId}`}
              value={variantForm.countryOfOrigin}
              onChange={(e) =>
                setVariantForm((current) => ({
                  ...current,
                  countryOfOrigin: e.target.value,
                }))
              }
            />
          </div>
        </div>
        <div className="form-row-2">
          <div className="form-field">
            <label htmlFor={`variant-size-${productId}`}>Size</label>
            <input
              id={`variant-size-${productId}`}
              value={variantForm.size}
              onChange={(e) =>
                setVariantForm((current) => ({ ...current, size: e.target.value }))
              }
            />
          </div>
          <div className="form-field">
            <label htmlFor={`variant-color-${productId}`}>Color</label>
            <input
              id={`variant-color-${productId}`}
              value={variantForm.color}
              onChange={(e) =>
                setVariantForm((current) => ({ ...current, color: e.target.value }))
              }
            />
          </div>
        </div>
      </Modal>
    </>
  );
}
