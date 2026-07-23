"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { ApiError, apiFetch, type PublicCart } from "@/lib/api";
import { getCartId, notifyCartUpdated } from "@/lib/cart-session";

function formatPrice(price: number, currency: string) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: currency || "USD",
  }).format(price);
}

export default function CartPage() {
  const { translate } = useLanguage();
  const [cart, setCart] = useState<PublicCart | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [updatingId, setUpdatingId] = useState<string | null>(null);
  const [error, setError] = useState("");

  const loadCart = useCallback(async () => {
    setIsLoading(true);
    setError("");

    try {
      const data = await apiFetch<PublicCart>(`/api/carts/${getCartId()}`);
      setCart(data);
      if ((data.removedUnavailableItems ?? 0) > 0) {
        notifyCartUpdated();
        setError(
          "Some items are no longer available and were removed from your cart.",
        );
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load cart.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadCart();
  }, [loadCart]);

  async function changeQuantity(itemId: string, quantity: number) {
    if (quantity <= 0) {
      await removeItem(itemId);
      return;
    }

    setUpdatingId(itemId);
    setError("");
    try {
      await apiFetch(`/api/carts/${getCartId()}/items/${itemId}`, {
        method: "PUT",
        body: JSON.stringify({ quantity }),
      });
      await loadCart();
      notifyCartUpdated();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update cart.");
    } finally {
      setUpdatingId(null);
    }
  }

  async function removeItem(itemId: string) {
    setUpdatingId(itemId);
    setError("");
    try {
      await apiFetch(`/api/carts/${getCartId()}/items/${itemId}`, {
        method: "DELETE",
      });
      await loadCart();
      notifyCartUpdated();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to remove item.");
    } finally {
      setUpdatingId(null);
    }
  }

  return (
    <PublicSiteLayout>
      <section className="section container catalog-page">
        <div className="section-heading">
          <div>
            <p className="eyebrow">{translate("shoppingCart")}</p>
            <h1>{translate("yourCart")}</h1>
          </div>
        </div>

        {isLoading ? <p className="catalog-message">{translate("loadingCart")}</p> : null}
        {error ? <p className="catalog-message catalog-error">{error}</p> : null}

        {!isLoading && cart?.items.length === 0 ? (
          <div className="empty-cart">
            <h2>{translate("cartIsEmpty")}</h2>
            <Link href="/products" className="button button-primary">
              {translate("continueShopping")}
            </Link>
          </div>
        ) : null}

        {!isLoading && cart && cart.items.length > 0 ? (
          <div className="cart-layout">
            <div className="cart-items">
              {cart.items.map((item) => (
                <article key={item.id} className="cart-item">
                  <Link href={`/products/${item.productId}`} className="cart-item-image">
                    {item.imageUrl ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img src={item.imageUrl} alt={item.productName} />
                    ) : (
                      <div className="catalog-image-placeholder" />
                    )}
                  </Link>
                  <div className="cart-item-content">
                    <Link href={`/products/${item.productId}`}>
                      <h2>{item.productName}</h2>
                    </Link>
                    <p>{formatPrice(item.unitPrice, item.currency)}</p>
                    <div className="cart-item-actions">
                      <button
                        type="button"
                        disabled={updatingId === item.id}
                        onClick={() => void changeQuantity(item.id, item.quantity - 1)}
                      >
                        −
                      </button>
                      <span>{item.quantity}</span>
                      <button
                        type="button"
                        disabled={updatingId === item.id}
                        onClick={() => void changeQuantity(item.id, item.quantity + 1)}
                      >
                        +
                      </button>
                      <button
                        type="button"
                        className="cart-remove"
                        disabled={updatingId === item.id}
                        onClick={() => void removeItem(item.id)}
                      >
                        {translate("remove")}
                      </button>
                    </div>
                  </div>
                  <strong>{formatPrice(item.lineTotal, item.currency)}</strong>
                </article>
              ))}
            </div>

            <aside className="cart-summary">
              <h2>{translate("orderSummary")}</h2>
              <div>
                <span>{translate("subtotal")}</span>
                <strong>{formatPrice(cart.subtotal, cart.currency)}</strong>
              </div>
              <p>{translate("shippingCalculatedAtCheckout")}</p>
              <Link href="/checkout" className="button button-primary">
                {translate("proceedToCheckout")}
              </Link>
            </aside>
          </div>
        ) : null}
      </section>
    </PublicSiteLayout>
  );
}
