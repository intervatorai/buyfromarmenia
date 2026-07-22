"use client";

import { useEffect, useState } from "react";
import { ProductForm, type ProductFormInitial } from "@/components/products/ProductForm";
import { ApiError, apiFetch } from "@/lib/api";
import { getSupplierId } from "@/lib/supplier-session";

type ProductTranslation = {
  languageCode: string;
  name: string;
  shortDescription: string;
  description: string;
};

type ProductDetail = {
  name: string;
  shortDescription: string;
  description: string;
  ingredients: string;
  price: number;
  currency: string;
  categoryId?: string | null;
  translations?: ProductTranslation[];
  variants: {
    supplierSku: string;
    weight: number;
    size?: string;
    color?: string;
    countryOfOrigin: string;
  }[];
  media: { storageKey: string; url: string }[];
  shipping?: {
    netWeight: number;
    grossWeight: number;
    packageLength: number;
    packageWidth: number;
    packageHeight: number;
    isFragile: boolean;
  } | null;
};

function mapProductToInitial(product: ProductDetail): ProductFormInitial {
  const variant = product.variants[0];
  const en = product.translations?.find((item) => item.languageCode === "en") ?? null;
  const hy = product.translations?.find((item) => item.languageCode === "hy") ?? null;

  return {
    en: {
      name: en?.name ?? product.name ?? "",
      shortDescription: en?.shortDescription ?? product.shortDescription ?? "",
      description: en?.description ?? product.description ?? "",
    },
    hy: {
      name: hy?.name ?? "",
      shortDescription: hy?.shortDescription ?? "",
      description: hy?.description ?? "",
    },
    ingredients: product.ingredients,
    price: String(product.price),
    currency: product.currency,
    categoryId: product.categoryId ?? "",
    supplierSku: variant?.supplierSku ?? "",
    variantWeight: variant ? String(variant.weight) : "",
    variantSize: variant?.size ?? "",
    variantColor: variant?.color ?? "",
    countryOfOrigin: variant?.countryOfOrigin ?? "AM",
    imageStorageKey: product.media[0]?.storageKey ?? "",
    netWeight: product.shipping ? String(product.shipping.netWeight) : "",
    grossWeight: product.shipping ? String(product.shipping.grossWeight) : "",
    packageLength: product.shipping ? String(product.shipping.packageLength) : "",
    packageWidth: product.shipping ? String(product.shipping.packageWidth) : "",
    packageHeight: product.shipping ? String(product.shipping.packageHeight) : "",
    isFragile: product.shipping?.isFragile ?? false,
  };
}

export function ProductFormModal({
  productId,
  onClose,
  onSaved,
}: {
  productId?: string;
  onClose: () => void;
  onSaved: (productId: string) => void;
}) {
  const isEdit = Boolean(productId);
  const [initial, setInitial] = useState<ProductFormInitial | null>(null);
  const [initialImageUrl, setInitialImageUrl] = useState("");
  const [loadError, setLoadError] = useState("");

  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        onClose();
      }
    }

    document.addEventListener("keydown", handleKeyDown);
    document.body.style.overflow = "hidden";
    return () => {
      document.removeEventListener("keydown", handleKeyDown);
      document.body.style.overflow = "";
    };
  }, [onClose]);

  useEffect(() => {
    if (!productId) return;
    const supplierId = getSupplierId();
    if (!supplierId) {
      setLoadError("Complete onboarding first.");
      return;
    }

    void apiFetch<ProductDetail>(`/api/products/${productId}?supplierId=${supplierId}`)
      .then((product) => {
        setInitial(mapProductToInitial(product));
        setInitialImageUrl(product.media[0]?.url ?? "");
      })
      .catch((err) =>
        setLoadError(err instanceof ApiError ? err.message : "Failed to load product."),
      );
  }, [productId]);

  const isReady = !isEdit || initial !== null;

  return (
    <div
      className="modal-overlay"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) {
          onClose();
        }
      }}
    >
      <div
        className="modal-dialog"
        role="dialog"
        aria-modal="true"
        aria-label={isEdit ? "Edit product" : "New product"}
      >
        <div className="modal-header">
          <h2>{isEdit ? "Edit product" : "New product"}</h2>
          <button type="button" className="modal-close" onClick={onClose} aria-label="Close">
            ×
          </button>
        </div>
        <div className="modal-body">
          {loadError ? (
            <p style={{ color: "#b91c1c", margin: 0 }}>{loadError}</p>
          ) : !isReady ? (
            <p style={{ color: "#64748b", margin: 0 }}>Loading...</p>
          ) : (
            <ProductForm
              productId={productId}
              initial={initial ?? undefined}
              initialImageUrl={initialImageUrl}
              onSuccess={onSaved}
              onCancel={onClose}
            />
          )}
        </div>
      </div>
    </div>
  );
}
