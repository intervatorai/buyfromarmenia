"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { ApiError, apiFetch, type PublicCart, type PublicProduct } from "@/lib/api";
import { CART_UPDATED_EVENT, getCartId } from "@/lib/cart-session";

function formatPrice(price: number, currency: string) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: currency || "USD",
  }).format(price);
}

function tagLabelKey(tag?: string | null) {
  switch (tag) {
    case "Bestseller":
      return "bestseller" as const;
    case "Popular":
      return "popular" as const;
    case "New":
      return "newTag" as const;
    default:
      return null;
  }
}

export function ProductsSection() {
  const { translate, language } = useLanguage();
  const [products, setProducts] = useState<PublicProduct[]>([]);
  const [wishlistProductIds, setWishlistProductIds] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadProducts() {
      setIsLoading(true);
      setError("");
      try {
        let data = await apiFetch<PublicProduct[]>(
          `/api/products?featuredOnly=true&take=6&lang=${language}`,
        );
        if (data.length === 0) {
          data = await apiFetch<PublicProduct[]>(
            `/api/products?take=6&lang=${language}`,
          );
        }
        setProducts(data);
      } catch (err) {
        setError(err instanceof ApiError ? err.message : translate("noProductsYet"));
      } finally {
        setIsLoading(false);
      }
    }

    void loadProducts();
  }, [language, translate]);

  useEffect(() => {
    async function loadWishlist() {
      const cartId = getCartId();
      if (!cartId) {
        return;
      }

      try {
        const cart = await apiFetch<PublicCart>(`/api/carts/${cartId}`);
        setWishlistProductIds(cart.wishlistProductIds);
      } catch {
        setWishlistProductIds([]);
      }
    }

    void loadWishlist();
    window.addEventListener(CART_UPDATED_EVENT, loadWishlist);
    return () => window.removeEventListener(CART_UPDATED_EVENT, loadWishlist);
  }, []);

  async function toggleFavorite(productId: string) {
    const isFavorite = wishlistProductIds.includes(productId);
    try {
      await apiFetch(`/api/carts/${getCartId()}/wishlist/${productId}`, {
        method: "PUT",
        body: JSON.stringify({ isFavorite: !isFavorite }),
      });
      setWishlistProductIds((current) =>
        isFavorite
          ? current.filter((id) => id !== productId)
          : [...current, productId],
      );
      window.dispatchEvent(new Event(CART_UPDATED_EVENT));
    } catch {
      // ignore wishlist failures on home
    }
  }

  return (
    <section className="section container" id="products">
      <div className="section-heading">
        <div>
          <p className="eyebrow">{translate("selectedForYou")}</p>
          <h2>{translate("popularProducts")}</h2>
        </div>

        <Link href="/products" className="view-all">
          {translate("viewAllProducts")}
        </Link>
      </div>

      {isLoading ? <p className="catalog-message">{translate("loadingProducts")}</p> : null}
      {error ? <p className="catalog-message catalog-error">{error}</p> : null}

      {!isLoading && !error && products.length === 0 ? (
        <p className="catalog-message">{translate("noProductsYet")}</p>
      ) : null}

      {!isLoading && products.length > 0 ? (
        <div className="product-grid">
          {products.map((product) => {
            const isFavorite = wishlistProductIds.includes(product.id);
            const badgeKey = tagLabelKey(product.tag);

            return (
              <article key={product.id} className="product-card">
                {badgeKey ? (
                  <span className="product-badge">{translate(badgeKey)}</span>
                ) : null}

                <button
                  type="button"
                  className={`favorite-button${isFavorite ? " active" : ""}`}
                  onClick={() => void toggleFavorite(product.id)}
                  aria-label={translate("favorites")}
                >
                  {isFavorite ? "♥" : "♡"}
                </button>

                <Link
                  href={`/products/${product.slug || product.id}`}
                  className="catalog-card-link"
                >
                  <div className="product-image catalog-product-image">
                    {product.primaryImageUrl ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img src={product.primaryImageUrl} alt={product.name} />
                    ) : (
                      <div className="catalog-image-placeholder" />
                    )}
                  </div>

                  <h3>{product.name}</h3>
                  <div className="product-price">
                    {formatPrice(product.price, product.currency)}
                  </div>
                </Link>
              </article>
            );
          })}
        </div>
      ) : null}
    </section>
  );
}
