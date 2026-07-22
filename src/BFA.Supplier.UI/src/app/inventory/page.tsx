"use client";

import { useCallback, useEffect, useState } from "react";
import { SupplierShell } from "@/components/layout/SupplierShell";
import { ApiError, apiFetch } from "@/lib/api";
import { getSupplierId } from "@/lib/supplier-session";

type Product = {
  id: string;
  name: string;
  variants: Array<{
    id: string;
    supplierSku: string;
  }>;
};

type Stock = {
  id: string;
  productId: string;
  productVariantId: string;
  productName: string;
  supplierSku: string;
  onHand: number;
  reserved: number;
  available: number;
  lowStockThreshold: number;
};

type EditableStock = {
  productId: string;
  productVariantId: string;
  productName: string;
  supplierSku: string;
  onHand: number;
  reserved: number;
  available: number;
  lowStockThreshold: number;
};

export default function InventoryPage() {
  const [rows, setRows] = useState<EditableStock[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [savingId, setSavingId] = useState<string | null>(null);
  const [error, setError] = useState("");

  const loadInventory = useCallback(async () => {
    const supplierId = getSupplierId();
    if (!supplierId) {
      setError("Complete supplier onboarding first.");
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError("");

    try {
      const [products, stock] = await Promise.all([
        apiFetch<Product[]>(`/api/products?supplierId=${supplierId}`),
        apiFetch<Stock[]>(`/api/inventory?supplierId=${supplierId}`),
      ]);
      const stockByVariant = new Map(
        stock.map((item) => [item.productVariantId, item]),
      );

      setRows(
        products.flatMap((product) =>
          product.variants.map((variant) => {
            const current = stockByVariant.get(variant.id);
            return {
              productId: product.id,
              productVariantId: variant.id,
              productName: product.name,
              supplierSku: variant.supplierSku,
              onHand: current?.onHand ?? 0,
              reserved: current?.reserved ?? 0,
              available: current?.available ?? 0,
              lowStockThreshold: current?.lowStockThreshold ?? 5,
            };
          }),
        ),
      );
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load inventory.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadInventory();
  }, [loadInventory]);

  function updateRow(
    variantId: string,
    field: "onHand" | "lowStockThreshold",
    value: number,
  ) {
    setRows((current) =>
      current.map((row) =>
        row.productVariantId === variantId
          ? { ...row, [field]: Math.max(0, value) }
          : row,
      ),
    );
  }

  async function save(row: EditableStock) {
    const supplierId = getSupplierId();
    if (!supplierId) {
      return;
    }

    setSavingId(row.productVariantId);
    setError("");

    try {
      await apiFetch(`/api/inventory/variants/${row.productVariantId}`, {
        method: "PUT",
        body: JSON.stringify({
          supplierId,
          productId: row.productId,
          onHand: row.onHand,
          lowStockThreshold: row.lowStockThreshold,
        }),
      });
      await loadInventory();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save stock.");
    } finally {
      setSavingId(null);
    }
  }

  return (
    <SupplierShell title="Inventory">
      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading inventory...</p> : null}

      {!isLoading && rows.length === 0 ? (
        <div className="supplier-card">
          Add a product variant before setting stock.
        </div>
      ) : null}

      {!isLoading && rows.length > 0 ? (
        <div className="supplier-table-wrap">
          <table className="supplier-table">
            <thead>
              <tr>
                <th>Product / SKU</th>
                <th>On hand</th>
                <th>Reserved</th>
                <th>Available</th>
                <th>Low stock at</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((row) => {
                const isLow = row.available <= row.lowStockThreshold;
                return (
                  <tr key={row.productVariantId}>
                    <td>
                      <strong>{row.productName}</strong>
                      <div className="supplier-card-label">{row.supplierSku}</div>
                    </td>
                    <td>
                      <input
                        className="inventory-number-input"
                        type="number"
                        min={row.reserved}
                        value={row.onHand}
                        onChange={(event) =>
                          updateRow(
                            row.productVariantId,
                            "onHand",
                            Number(event.target.value),
                          )
                        }
                      />
                    </td>
                    <td>{row.reserved}</td>
                    <td>
                      <span className={isLow ? "stock-low" : "stock-ok"}>
                        {row.available}
                      </span>
                    </td>
                    <td>
                      <input
                        className="inventory-number-input"
                        type="number"
                        min={0}
                        value={row.lowStockThreshold}
                        onChange={(event) =>
                          updateRow(
                            row.productVariantId,
                            "lowStockThreshold",
                            Number(event.target.value),
                          )
                        }
                      />
                    </td>
                    <td>
                      <button
                        className="button-primary"
                        type="button"
                        disabled={savingId === row.productVariantId}
                        onClick={() => void save(row)}
                      >
                        {savingId === row.productVariantId ? "Saving..." : "Save"}
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      ) : null}
    </SupplierShell>
  );
}
