"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useLanguage } from "@/components/providers/LanguageProvider";
import {
  apiFetch,
  type PublicCart,
  type PublicProduct,
} from "@/lib/api";
import { getCartId } from "@/lib/cart-session";

function formatPrice(price: number, currency: string) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: currency || "USD",
  }).format(price);
}

export default function WishlistPage() {
  const { translate, language } = useLanguage();
  const [products, setProducts] = useState<PublicProduct[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function loadWishlist() {
      try {
        const [cart, allProducts] = await Promise.all([
          apiFetch<PublicCart>(`/api/carts/${getCartId()}`),
          apiFetch<PublicProduct[]>(`/api/products?lang=${language}`),
        ]);
        const favoriteIds = new Set(cart.wishlistProductIds);
        setProducts(allProducts.filter((product) => favoriteIds.has(product.id)));
      } finally {
        setIsLoading(false);
      }
    }

    void loadWishlist();
  }, [language]);

  async function remove(productId: string) {
    await apiFetch(`/api/carts/${getCartId()}/wishlist/${productId}`, {
      method: "PUT",
      body: JSON.stringify({ isFavorite: false }),
    });
    setProducts((current) => current.filter((product) => product.id !== productId));
  }

  return (
    <PublicSiteLayout>
      <section className="section container catalog-page">
        <div className="section-heading">
          <div>
            <p className="eyebrow">{translate("favorites")}</p>
            <h1>{translate("yourWishlist")}</h1>
          </div>
        </div>

        {isLoading ? <p className="catalog-message">{translate("loadingProducts")}</p> : null}
        {!isLoading && products.length === 0 ? (
          <div className="empty-cart">
            <h2>{translate("wishlistIsEmpty")}</h2>
            <Link href="/products" className="button button-primary">
              {translate("continueShopping")}
            </Link>
          </div>
        ) : null}

        {products.length > 0 ? (
          <div className="product-grid catalog-grid">
            {products.map((product) => (
              <article key={product.id} className="product-card catalog-product-card">
                <button
                  type="button"
                  className="favorite-button active"
                  aria-label={translate("remove")}
                  onClick={() => void remove(product.id)}
                >
                  ♥
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
            ))}
          </div>
        ) : null}
      </section>
    </PublicSiteLayout>
  );
}
