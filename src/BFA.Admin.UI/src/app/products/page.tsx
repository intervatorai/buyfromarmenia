"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { Fragment, useCallback, useEffect, useRef, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { ProductFormModal } from "@/components/ui/ProductFormModal";
import { ProductSubpanels } from "@/components/ui/ProductSubpanels";
import { PromptModal } from "@/components/ui/PromptModal";
import { ApiError, apiFetch } from "@/lib/api";

type AdminProduct = {
  id: string;
  supplierId: string;
  name: string;
  shortDescription: string;
  description: string;
  price: number;
  currency: string;
  status: string;
  tag: string;
  categoryId?: string | null;
  variantsCount: number;
  primaryImageUrl?: string | null;
  createdAt: string;
  updatedAt?: string | null;
};

const STATUS_FILTERS = [
  "",
  "PendingReview",
  "ChangesRequested",
  "Approved",
  "Published",
  "Rejected",
  "Draft",
] as const;

function statusClass(status: string) {
  switch (status) {
    case "PendingReview":
      return "pendingreview";
    case "ChangesRequested":
      return "changesrequested";
    case "Approved":
      return "approved";
    case "Published":
      return "published";
    case "Rejected":
      return "rejected";
    case "Draft":
      return "draft";
    default:
      return status.toLowerCase();
  }
}

type PromptState =
  | { kind: "reject"; productId: string }
  | { kind: "request-changes"; productId: string }
  | null;

export default function ProductsPage() {
  const router = useRouter();
  const [products, setProducts] = useState<AdminProduct[]>([]);
  const [templates, setTemplates] = useState<string[]>([]);
  const [filter, setFilter] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [actionId, setActionId] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [prompt, setPrompt] = useState<PromptState>(null);
  const [openMenuId, setOpenMenuId] = useState("");
  const [expandedId, setExpandedId] = useState("");
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

  const loadProducts = useCallback(async () => {
    setIsLoading(true);
    setError("");

    try {
      const query = filter ? `?status=${encodeURIComponent(filter)}` : "";
      const data = await apiFetch<AdminProduct[]>(`/api/products${query}`);
      setProducts(data);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load products.");
    } finally {
      setIsLoading(false);
    }
  }, [filter]);

  useEffect(() => {
    void loadProducts();
  }, [loadProducts]);

  useEffect(() => {
    async function loadTemplates() {
      try {
        setTemplates(await apiFetch<string[]>("/api/products/rejection-templates"));
      } catch {
        // optional
      }
    }

    void loadTemplates();
  }, []);

  async function approveProduct(productId: string) {
    setActionId(productId);
    try {
      await apiFetch(`/api/products/${productId}/approve`, { method: "POST" });
      await loadProducts();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to approve product.");
    } finally {
      setActionId(null);
    }
  }

  return (
    <AdminShell title="Products">
      <div style={{ marginBottom: 16, display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap" }}>
        <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
          {STATUS_FILTERS.map((status) => (
            <button
              key={status || "all"}
              type="button"
              className={filter === status ? "button-primary" : "button-ghost"}
              onClick={() => setFilter(status)}
            >
              {status || "All"}
            </button>
          ))}
        </div>
        <button
          type="button"
          className="button-primary"
          onClick={() => {
            setEditId(null);
            setFormOpen(true);
          }}
        >
          Add product
        </button>
      </div>

      <p style={{ color: "var(--admin-muted)", marginBottom: 16, fontSize: 13 }}>
        Supplier submissions appear in <strong>PendingReview</strong>. Use{" "}
        <strong>Approve &amp; publish</strong> to put them on the public site.
      </p>

      {isLoading ? <p>Loading products...</p> : null}
      {error ? <p className="form-error">{error}</p> : null}

      {!isLoading && products.length === 0 ? (
        <div className="admin-card">No products in this status.</div>
      ) : null}

      {!isLoading && products.length > 0 ? (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th className="product-expand-cell" aria-label="Expand" />
                <th>Product</th>
                <th>Supplier</th>
                <th>Price</th>
                <th>Status</th>
                <th>Tag</th>
                <th>Created</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.map((product) => {
                const isExpanded = expandedId === product.id;
                return (
                  <Fragment key={product.id}>
                    <tr>
                      <td className="product-expand-cell">
                        <button
                          type="button"
                          className="product-expand-btn"
                          aria-expanded={isExpanded}
                          aria-label={isExpanded ? "Collapse product" : "Expand product"}
                          onClick={() =>
                            setExpandedId((current) =>
                              current === product.id ? "" : product.id,
                            )
                          }
                        >
                          {isExpanded ? "▾" : "▸"}
                        </button>
                      </td>
                      <td>
                        <Link href={`/products/${product.id}`}>
                          <strong>{product.name}</strong>
                        </Link>
                        <div style={{ color: "var(--admin-muted)", fontSize: 12 }}>
                          {product.shortDescription || product.description}
                        </div>
                      </td>
                      <td style={{ fontSize: 12, color: "var(--admin-muted)" }}>
                        {product.supplierId.slice(0, 8)}…
                      </td>
                      <td>
                        {product.price.toFixed(2)} {product.currency}
                      </td>
                      <td>
                        <span className={`status-badge ${statusClass(product.status)}`}>
                          {product.status}
                        </span>
                      </td>
                      <td>{product.tag && product.tag !== "None" ? product.tag : "—"}</td>
                      <td>{new Date(product.createdAt).toLocaleDateString("en-GB")}</td>
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
                            disabled={actionId === product.id}
                            onClick={() =>
                              setOpenMenuId((current) =>
                                current === product.id ? "" : product.id,
                              )
                            }
                          >
                            {actionId === product.id ? "…" : "⋯"}
                          </button>
                          {openMenuId === product.id ? (
                            <div className="row-menu-dropdown" role="menu">
                              <Link
                                href={`/products/${product.id}`}
                                role="menuitem"
                                className="row-menu-item"
                                onClick={() => setOpenMenuId("")}
                              >
                                Open
                              </Link>
                              <button
                                type="button"
                                role="menuitem"
                                className="row-menu-item"
                                onClick={() => {
                                  setOpenMenuId("");
                                  setEditId(product.id);
                                  setFormOpen(true);
                                }}
                              >
                                Edit
                              </button>
                              {product.status === "PendingReview" ? (
                                <>
                                  <button
                                    type="button"
                                    role="menuitem"
                                    className="row-menu-item"
                                    onClick={() => {
                                      setOpenMenuId("");
                                      void approveProduct(product.id);
                                    }}
                                  >
                                    Approve &amp; publish
                                  </button>
                                  <button
                                    type="button"
                                    role="menuitem"
                                    className="row-menu-item"
                                    onClick={() => {
                                      setOpenMenuId("");
                                      setPrompt({
                                        kind: "request-changes",
                                        productId: product.id,
                                      });
                                    }}
                                  >
                                    Request changes
                                  </button>
                                  <button
                                    type="button"
                                    role="menuitem"
                                    className="row-menu-item danger"
                                    onClick={() => {
                                      setOpenMenuId("");
                                      setPrompt({ kind: "reject", productId: product.id });
                                    }}
                                  >
                                    Reject
                                  </button>
                                </>
                              ) : null}
                            </div>
                          ) : null}
                        </div>
                      </td>
                    </tr>
                    {isExpanded ? (
                      <tr className="product-expand-row">
                        <td colSpan={8}>
                          <ProductSubpanels
                            productId={product.id}
                            collapsible={false}
                            defaultOpen
                          />
                        </td>
                      </tr>
                    ) : null}
                  </Fragment>
                );
              })}
            </tbody>
          </table>
        </div>
      ) : null}

      <ProductFormModal
        open={formOpen}
        productId={editId}
        onClose={() => setFormOpen(false)}
        onSaved={(id) => {
          void loadProducts();
          if (!editId) {
            router.push(`/products/${id}`);
          }
        }}
      />

      <PromptModal
        open={prompt !== null}
        title={prompt?.kind === "reject" ? "Reject product" : "Request changes"}
        templates={templates}
        confirmLabel={prompt?.kind === "reject" ? "Reject" : "Send"}
        onClose={() => setPrompt(null)}
        onConfirm={async (reason) => {
          if (!prompt) {
            return;
          }
          setActionId(prompt.productId);
          try {
            const path =
              prompt.kind === "reject"
                ? `/api/products/${prompt.productId}/reject`
                : `/api/products/${prompt.productId}/request-changes`;
            await apiFetch(path, {
              method: "POST",
              body: JSON.stringify({ reason }),
            });
            await loadProducts();
          } catch (err) {
            setError(err instanceof ApiError ? err.message : "Action failed.");
            throw err;
          } finally {
            setActionId(null);
          }
        }}
      />
    </AdminShell>
  );
}
