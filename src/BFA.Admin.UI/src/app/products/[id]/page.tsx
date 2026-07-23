"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { ProductFormModal } from "@/components/ui/ProductFormModal";
import { ProductSubpanels } from "@/components/ui/ProductSubpanels";
import { PromptModal } from "@/components/ui/PromptModal";
import { ApiError, apiFetch } from "@/lib/api";

type ProductDetail = {
  id: string;
  supplierId: string;
  name: string;
  shortDescription: string;
  description: string;
  ingredients: string;
  usageInstructions: string;
  price: number;
  currency: string;
  status: string;
  tag: string;
  categoryId?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  translations: Array<{
    languageCode: string;
    name: string;
    shortDescription: string;
    description: string;
  }>;
  media: Array<{ id: string; url: string; isPrimary: boolean }>;
  documents: Array<{ id: string; fileName: string; fileUrl: string; documentType: string }>;
};

type CategoryOption = { id: string; name: string };

const LANGUAGE_NAMES: Record<string, string> = { en: "English", hy: "Armenian" };

type PromptKind = "request-changes" | "reject" | null;

export default function ProductDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [categories, setCategories] = useState<CategoryOption[]>([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [promptKind, setPromptKind] = useState<PromptKind>(null);

  const load = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      setProduct(await apiFetch<ProductDetail>(`/api/products/${params.id}`));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load product.");
    } finally {
      setIsLoading(false);
    }
  }, [params.id]);

  useEffect(() => {
    void load();
    void apiFetch<CategoryOption[]>("/api/categories")
      .then(setCategories)
      .catch(() => setCategories([]));
  }, [load]);

  const categoryName = product?.categoryId
    ? categories.find((category) => category.id === product.categoryId)?.name ?? "—"
    : "—";

  async function runAction(path: string, body?: object) {
    setBusy(true);
    setError("");
    try {
      await apiFetch(path, {
        method: "POST",
        body: body ? JSON.stringify(body) : undefined,
      });
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Action failed.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <AdminShell title={product?.name ?? "Product"}>
      <p style={{ marginBottom: 16 }}>
        <Link href="/products" className="button-ghost">
          ← Back to products
        </Link>
      </p>

      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading...</p> : null}

      {product ? (
        <>
          <div className="admin-grid" style={{ marginBottom: 24 }}>
            <div className="admin-card">
              <div className="admin-card-label">Status</div>
              <div className="admin-card-value" style={{ fontSize: 18 }}>
                {product.status}
              </div>
            </div>
            <div className="admin-card">
              <div className="admin-card-label">Tag</div>
              <div className="admin-card-value" style={{ fontSize: 18 }}>
                {product.tag && product.tag !== "None" ? product.tag : "—"}
              </div>
            </div>
            <div className="admin-card">
              <div className="admin-card-label">Price</div>
              <div className="admin-card-value" style={{ fontSize: 18 }}>
                {product.price.toFixed(2)} {product.currency}
              </div>
            </div>
            <div className="admin-card">
              <div className="admin-card-label">Category</div>
              <div className="admin-card-value" style={{ fontSize: 18 }}>
                {categoryName}
              </div>
            </div>
            <div className="admin-card">
              <div className="admin-card-label">Supplier</div>
              <div>
                <Link href={`/vendors/${product.supplierId}`}>
                  {product.supplierId.slice(0, 8)}
                </Link>
              </div>
            </div>
          </div>

          {(product.translations.length > 0
            ? product.translations
            : [
                {
                  languageCode: "en",
                  name: product.name,
                  shortDescription: product.shortDescription,
                  description: product.description,
                },
              ]
          ).map((translation) => (
            <div
              key={translation.languageCode}
              className="admin-card"
              style={{ marginBottom: 16 }}
            >
              <h2 style={{ marginTop: 0 }}>
                Description (
                {LANGUAGE_NAMES[translation.languageCode] ?? translation.languageCode})
              </h2>
              <p style={{ fontWeight: 600 }}>{translation.name}</p>
              {translation.shortDescription ? (
                <p style={{ color: "var(--admin-muted)" }}>{translation.shortDescription}</p>
              ) : null}
              <p style={{ whiteSpace: "pre-wrap" }}>{translation.description || "—"}</p>
            </div>
          ))}

          {product.ingredients || product.usageInstructions ? (
            <div className="admin-card" style={{ marginBottom: 24 }}>
              {product.ingredients ? (
                <>
                  <h2 style={{ marginTop: 0 }}>Ingredients / composition</h2>
                  <p style={{ whiteSpace: "pre-wrap" }}>{product.ingredients}</p>
                </>
              ) : null}
              {product.usageInstructions ? (
                <>
                  <h2>Usage instructions</h2>
                  <p style={{ whiteSpace: "pre-wrap" }}>{product.usageInstructions}</p>
                </>
              ) : null}
            </div>
          ) : null}

          <div style={{ display: "flex", gap: 8, flexWrap: "wrap", marginBottom: 24 }}>
            <button type="button" className="button-primary" onClick={() => setEditOpen(true)}>
              Edit
            </button>
            <button
              type="button"
              className="button-ghost"
              disabled={busy}
              onClick={() => setDeleteOpen(true)}
            >
              Delete
            </button>
            {product.status === "PendingReview" ? (
              <>
                <button
                  type="button"
                  className="button-primary"
                  disabled={busy}
                  onClick={() => void runAction(`/api/products/${product.id}/approve`)}
                >
                  Approve &amp; publish
                </button>
                <button
                  type="button"
                  className="button-ghost"
                  disabled={busy}
                  onClick={() => setPromptKind("request-changes")}
                >
                  Request changes
                </button>
                <button
                  type="button"
                  className="button-ghost"
                  disabled={busy}
                  onClick={() => setPromptKind("reject")}
                >
                  Reject
                </button>
              </>
            ) : null}
            {product.status === "Published" ? (
              <button
                type="button"
                className="button-ghost"
                disabled={busy}
                onClick={() => void runAction(`/api/products/${product.id}/suspend`)}
              >
                Suspend
              </button>
            ) : null}
            {product.status === "Suspended" ? (
              <button
                type="button"
                className="button-primary"
                disabled={busy}
                onClick={() => void runAction(`/api/products/${product.id}/publish`)}
              >
                Republish
              </button>
            ) : null}
            {product.status !== "Archived" ? (
              <button
                type="button"
                className="button-ghost"
                disabled={busy}
                onClick={() => void runAction(`/api/products/${product.id}/archive`)}
              >
                Archive
              </button>
            ) : null}
          </div>

          <div style={{ marginBottom: 24 }}>
            <ProductSubpanels productId={product.id} defaultOpen />
          </div>

          <h2>Media</h2>
          <div style={{ display: "flex", gap: 12, flexWrap: "wrap", marginBottom: 24 }}>
            {product.media.length === 0 ? (
              <p style={{ color: "var(--admin-muted)" }}>No media</p>
            ) : null}
            {product.media.map((media) => (
              <a key={media.id} href={media.url} target="_blank" rel="noreferrer">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={media.url}
                  alt=""
                  style={{
                    width: 96,
                    height: 96,
                    objectFit: "cover",
                    borderRadius: 8,
                    border: media.isPrimary
                      ? "2px solid var(--admin-accent)"
                      : "1px solid var(--admin-border)",
                  }}
                />
              </a>
            ))}
          </div>

          <h2>Documents</h2>
          <ul>
            {product.documents.length === 0 ? (
              <li style={{ color: "var(--admin-muted)" }}>None</li>
            ) : null}
            {product.documents.map((document) => (
              <li key={document.id}>
                <a href={document.fileUrl} target="_blank" rel="noreferrer">
                  {document.fileName}
                </a>{" "}
                ({document.documentType})
              </li>
            ))}
          </ul>
        </>
      ) : null}

      <ProductFormModal
        open={editOpen}
        productId={params.id}
        onClose={() => setEditOpen(false)}
        onSaved={() => void load()}
      />

      <ConfirmModal
        open={deleteOpen}
        title="Delete product"
        message="Delete this product permanently? This cannot be undone."
        confirmLabel="Delete"
        danger
        onClose={() => setDeleteOpen(false)}
        onConfirm={async () => {
          setBusy(true);
          try {
            await apiFetch(`/api/products/${params.id}`, { method: "DELETE" });
            router.push("/products");
          } catch (err) {
            setError(err instanceof ApiError ? err.message : "Delete failed.");
            setBusy(false);
            setDeleteOpen(false);
          }
        }}
      />

      <PromptModal
        open={promptKind !== null}
        title={promptKind === "reject" ? "Reject product" : "Request changes"}
        confirmLabel={promptKind === "reject" ? "Reject" : "Send"}
        onClose={() => setPromptKind(null)}
        onConfirm={async (reason) => {
          if (!product || !promptKind) {
            return;
          }
          const path =
            promptKind === "reject"
              ? `/api/products/${product.id}/reject`
              : `/api/products/${product.id}/request-changes`;
          await runAction(path, { reason });
        }}
      />
    </AdminShell>
  );
}
