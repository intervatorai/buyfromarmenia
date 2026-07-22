"use client";

import { SupplierShell } from "@/components/layout/SupplierShell";
import { ProductFormModal } from "@/components/products/ProductFormModal";
import { ApiError, apiFetch } from "@/lib/api";
import { getSupplierId } from "@/lib/supplier-session";
import { useEffect, useRef, useState } from "react";

type Product = {
  id: string;
  name: string;
  shortDescription: string;
  price: number;
  currency: string;
  status: string;
  variantsCount: number;
  mediaCount: number;
  primaryImageUrl?: string | null;
  createdAt: string;
};

function statusClass(status: string) {
  const normalized = status.toLowerCase();
  if (normalized.includes("pending")) return "pending";
  if (normalized.includes("published") || normalized.includes("approved")) return "published";
  if (normalized.includes("archiv")) return "draft";
  return "draft";
}

function canHardDelete(status: string) {
  return status === "Draft" || status === "ChangesRequested" || status === "Rejected";
}

export function ProductsList() {
  const [products, setProducts] = useState<Product[]>([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [busyId, setBusyId] = useState("");
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editId, setEditId] = useState("");
  const [openMenuId, setOpenMenuId] = useState("");
  const menuRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!openMenuId) return;

    function handlePointerDown(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setOpenMenuId("");
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setOpenMenuId("");
      }
    }

    document.addEventListener("mousedown", handlePointerDown);
    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.removeEventListener("mousedown", handlePointerDown);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [openMenuId]);

  async function loadProducts() {
    const supplierId = getSupplierId();
    if (!supplierId) {
      setError("Complete onboarding first.");
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    try {
      const data = await apiFetch<Product[]>(`/api/products?supplierId=${supplierId}`);
      setProducts(data);
      setError("");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load products.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadProducts();
  }, []);

  async function handleSubmit(id: string) {
    const supplierId = getSupplierId();
    if (!supplierId) return;

    setBusyId(id);
    try {
      await apiFetch(`/api/products/${id}/submit`, {
        method: "POST",
        body: JSON.stringify({ supplierId }),
      });
      setProducts((prev) =>
        prev.map((p) => (p.id === id ? { ...p, status: "PendingReview" } : p)),
      );
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to submit product.");
    } finally {
      setBusyId("");
    }
  }

  async function handleDelete(product: Product) {
    const supplierId = getSupplierId();
    if (!supplierId) return;

    const action = canHardDelete(product.status) ? "delete" : "archive";
    const confirmed = window.confirm(
      action === "delete"
        ? `Delete "${product.name}" permanently?`
        : `Archive "${product.name}"? It will be hidden from the catalog.`,
    );
    if (!confirmed) return;

    setBusyId(product.id);
    try {
      await apiFetch(`/api/products/${product.id}?supplierId=${supplierId}`, {
        method: "DELETE",
      });
      if (action === "delete") {
        setProducts((prev) => prev.filter((p) => p.id !== product.id));
      } else {
        setProducts((prev) =>
          prev.map((p) => (p.id === product.id ? { ...p, status: "Archived" } : p)),
        );
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete product.");
    } finally {
      setBusyId("");
    }
  }

  return (
    <SupplierShell
      title="Products"
      action={
        <button
          type="button"
          className="button-primary"
          onClick={() => setIsCreateOpen(true)}
        >
          New product
        </button>
      }
    >
      {isCreateOpen || editId ? (
        <ProductFormModal
          productId={editId || undefined}
          onClose={() => {
            setIsCreateOpen(false);
            setEditId("");
          }}
          onSaved={() => {
            setIsCreateOpen(false);
            setEditId("");
            void loadProducts();
          }}
        />
      ) : null}

      {error ? <div style={{ color: "#b91c1c", marginBottom: 16 }}>{error}</div> : null}
      {isLoading ? <p>Loading...</p> : null}

      {!isLoading && products.length === 0 ? (
        <div className="supplier-empty">
          <p>No products yet.</p>
          <button
            type="button"
            className="button-primary"
            onClick={() => setIsCreateOpen(true)}
          >
            Create first product
          </button>
        </div>
      ) : null}

      {!isLoading && products.length > 0 ? (
        <div className="supplier-table-wrap">
          <table className="supplier-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Price</th>
                <th>Variants</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.map((product) => (
                <tr key={product.id}>
                  <td>
                    <strong>{product.name}</strong>
                    {product.shortDescription ? (
                      <>
                        <br />
                        <span style={{ color: "#64748b", fontSize: 12 }}>
                          {product.shortDescription}
                        </span>
                      </>
                    ) : null}
                  </td>
                  <td>
                    {product.price.toFixed(2)} {product.currency}
                  </td>
                  <td>{product.variantsCount}</td>
                  <td>
                    <span className={`status-badge ${statusClass(product.status)}`}>
                      {product.status}
                    </span>
                  </td>
                  <td>
                    <div
                      className="row-menu"
                      ref={openMenuId === product.id ? menuRef : undefined}
                    >
                      <button
                        type="button"
                        className="row-menu-trigger"
                        aria-haspopup="menu"
                        aria-expanded={openMenuId === product.id}
                        aria-label="Product actions"
                        disabled={busyId === product.id}
                        onClick={() =>
                          setOpenMenuId((current) =>
                            current === product.id ? "" : product.id,
                          )
                        }
                      >
                        ⋯
                      </button>
                      {openMenuId === product.id ? (
                        <div className="row-menu-dropdown" role="menu">
                          <button
                            type="button"
                            role="menuitem"
                            className="row-menu-item"
                            onClick={() => {
                              setOpenMenuId("");
                              setEditId(product.id);
                            }}
                          >
                            Edit
                          </button>
                          {(product.status === "Draft" ||
                            product.status === "ChangesRequested") && (
                            <button
                              type="button"
                              role="menuitem"
                              className="row-menu-item"
                              onClick={() => {
                                setOpenMenuId("");
                                void handleSubmit(product.id);
                              }}
                            >
                              Submit
                            </button>
                          )}
                          {product.status !== "Archived" ? (
                            <button
                              type="button"
                              role="menuitem"
                              className="row-menu-item danger"
                              onClick={() => {
                                setOpenMenuId("");
                                void handleDelete(product);
                              }}
                            >
                              {canHardDelete(product.status) ? "Delete" : "Archive"}
                            </button>
                          ) : null}
                        </div>
                      ) : null}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}
    </SupplierShell>
  );
}
