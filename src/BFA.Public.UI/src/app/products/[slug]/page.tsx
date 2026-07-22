"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { ApiError, apiFetch, type PublicProductDetail } from "@/lib/api";
import { getCartId, notifyCartUpdated } from "@/lib/cart-session";

function formatPrice(price: number, currency: string) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: currency || "USD",
  }).format(price);
}

export default function ProductDetailPage() {
  const params = useParams<{ slug: string }>();
  const { translate, language } = useLanguage();
  const [product, setProduct] = useState<PublicProductDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [selectedVariantId, setSelectedVariantId] = useState("");
  const [quantity, setQuantity] = useState(1);
  const [isAdding, setIsAdding] = useState(false);
  const [message, setMessage] = useState("");

  useEffect(() => {
    async function loadProduct() {
      if (!params.slug) {
        return;
      }

      setIsLoading(true);
      setError("");

      try {
        const data = await apiFetch<PublicProductDetail>(
          `/api/products/${encodeURIComponent(params.slug)}?lang=${language}`,
        );
        setProduct(data);
        setSelectedVariantId(
          data.variants.find((variant) => variant.available > 0)?.id ??
            data.variants[0]?.id ??
            "",
        );
      } catch (err) {
        if (err instanceof ApiError && err.status === 404) {
          setError(translate("productNotFound"));
        } else {
          setError(err instanceof ApiError ? err.message : "Failed to load product.");
        }
      } finally {
        setIsLoading(false);
      }
    }

    void loadProduct();
  }, [params.slug, language, translate]);

  const primaryImage =
    product?.images.find((image) => image.isPrimary)?.url ??
    product?.images[0]?.url ??
    product?.primaryImageUrl;
  const selectedVariant = product?.variants.find(
    (variant) => variant.id === selectedVariantId,
  );

  async function addToCart() {
    if (!product || !selectedVariantId) {
      return;
    }

    setIsAdding(true);
    setError("");
    setMessage("");

    try {
      await apiFetch(`/api/carts/${getCartId()}/items`, {
        method: "POST",
        body: JSON.stringify({
          productId: product.id,
          productVariantId: selectedVariantId,
          quantity,
        }),
      });
      notifyCartUpdated();
      setMessage(translate("addedToCart"));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to add product.");
    } finally {
      setIsAdding(false);
    }
  }

  const backHref = product?.categorySlug
    ? `/categories/${product.categorySlug}`
    : "/products";

  return (
    <PublicSiteLayout>
      <section className="section container catalog-page">
        <Link href={backHref} className="catalog-back-link">
          ← {translate("backToProducts")}
        </Link>

        {isLoading ? <p className="catalog-message">{translate("loadingProducts")}</p> : null}
        {error ? <p className="catalog-message catalog-error">{error}</p> : null}

        {!isLoading && product ? (
          <div className="product-detail">
            <div className="product-detail-gallery">
              <div className="product-detail-image">
                {primaryImage ? (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img src={primaryImage} alt={product.name} />
                ) : (
                  <div className="catalog-image-placeholder" />
                )}
              </div>

              {product.images.length > 1 ? (
                <div className="product-detail-thumbs">
                  {product.images.map((image) => (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img key={image.url} src={image.url} alt={image.altText ?? product.name} />
                  ))}
                </div>
              ) : null}
            </div>

            <div className="product-detail-content">
              <p className="eyebrow">{translate("madeInArmenia")}</p>
              <h1>{product.name}</h1>
              <div className="product-price product-detail-price">
                {formatPrice(product.price, product.currency)}
              </div>

              {product.shortDescription ? (
                <p className="product-detail-lead">{product.shortDescription}</p>
              ) : null}

              {product.description ? (
                <div className="product-detail-block">
                  <h2>{translate("description")}</h2>
                  <p>{product.description}</p>
                </div>
              ) : null}

              {product.ingredients ? (
                <div className="product-detail-block">
                  <h2>{translate("ingredients")}</h2>
                  <p>{product.ingredients}</p>
                </div>
              ) : null}

              {product.usageInstructions ? (
                <div className="product-detail-block">
                  <h2>{translate("usageInstructions")}</h2>
                  <p>{product.usageInstructions}</p>
                </div>
              ) : null}

              {product.variants.length > 0 ? (
                <div className="product-detail-block">
                  <h2>{translate("variants")}</h2>
                  <div className="product-variant-options">
                    {product.variants.map((variant) => (
                      <button
                        key={variant.id}
                        type="button"
                        disabled={variant.available === 0}
                        className={
                          selectedVariantId === variant.id
                            ? "product-variant-option active"
                            : "product-variant-option"
                        }
                        onClick={() => setSelectedVariantId(variant.id)}
                      >
                        <strong>{variant.supplierSku}</strong>
                        {variant.size ? ` · ${variant.size}` : ""}
                        {variant.color ? ` · ${variant.color}` : ""}
                        {` · ${variant.available} ${translate("inStock")}`}
                      </button>
                    ))}
                  </div>
                </div>
              ) : null}

              <div className="product-cart-actions">
                <input
                  type="number"
                  min={1}
                  max={selectedVariant?.available ?? 1}
                  value={quantity}
                  onChange={(event) =>
                    setQuantity(Math.max(1, Number(event.target.value)))
                  }
                  aria-label={translate("quantity")}
                />
                <button
                  type="button"
                  className="button button-primary product-detail-cta"
                  disabled={
                    isAdding ||
                    !selectedVariant ||
                    selectedVariant.available < quantity
                  }
                  onClick={() => void addToCart()}
                >
                  {isAdding ? translate("addingToCart") : translate("addToCart")}
                </button>
              </div>
              {message ? <p className="cart-success">{message}</p> : null}
            </div>
          </div>
        ) : null}
      </section>
    </PublicSiteLayout>
  );
}
