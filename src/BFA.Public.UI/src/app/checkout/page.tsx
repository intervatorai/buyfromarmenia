"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useAuth } from "@/components/providers/AuthProvider";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { ApiError, apiFetch, type PublicCart } from "@/lib/api";
import type { CustomerDeliveryAddress } from "@/lib/auth";
import { getCartId, notifyCartUpdated } from "@/lib/cart-session";

function formatPrice(price: number, currency: string) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: currency || "USD",
  }).format(price);
}

export default function CheckoutPage() {
  const router = useRouter();
  const { translate } = useLanguage();
  const { user, isAuthenticated, isLoading: isAuthLoading } = useAuth();
  const [cart, setCart] = useState<PublicCart | null>(null);
  const [addresses, setAddresses] = useState<CustomerDeliveryAddress[]>([]);
  const [selectedAddressId, setSelectedAddressId] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [customerEmail, setCustomerEmail] = useState("");
  const [customerFullName, setCustomerFullName] = useState("");

  const loadCheckout = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      const [cartData, addressData] = await Promise.all([
        apiFetch<PublicCart>(`/api/carts/${getCartId()}`),
        apiFetch<CustomerDeliveryAddress[]>("/api/delivery-addresses"),
      ]);
      setCart(cartData);
      setAddresses(addressData);
      const defaultAddress =
        addressData.find((address) => address.isDefault) ?? addressData[0];
      setSelectedAddressId(defaultAddress?.id ?? "");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load checkout.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadCheckout();
  }, [loadCheckout]);

  useEffect(() => {
    if (!isAuthLoading && isAuthenticated && user) {
      setCustomerEmail(user.email);
      setCustomerFullName(user.fullName);
    }
  }, [isAuthLoading, isAuthenticated, user]);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selectedAddressId) {
      setError("Add a delivery address in your account before placing an order.");
      return;
    }

    setIsSubmitting(true);
    setError("");

    try {
      const result = await apiFetch<{ orderId: string; orderNumber: string }>(
        "/api/orders",
        {
          method: "POST",
          body: JSON.stringify({
            cartId: getCartId(),
            customerEmail,
            customerFullName,
            deliveryAddressId: selectedAddressId,
          }),
        },
      );
      notifyCartUpdated();
      router.push(`/orders/${result.orderId}`);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to place order.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (!isLoading && cart?.items.length === 0) {
    return (
      <PublicSiteLayout>
        <section className="section container catalog-page">
          <div className="empty-cart">
            <h2>{translate("cartIsEmpty")}</h2>
            <Link href="/products" className="button button-primary">
              {translate("continueShopping")}
            </Link>
          </div>
        </section>
      </PublicSiteLayout>
    );
  }

  return (
    <PublicSiteLayout>
      <section className="section container catalog-page">
        <div className="section-heading">
          <div>
            <p className="eyebrow">{translate("checkout")}</p>
            <h1>{translate("deliveryDetails")}</h1>
          </div>
        </div>

        <div className="checkout-layout">
          <form className="checkout-form" onSubmit={(event) => void handleSubmit(event)}>
            <label>
              {translate("fullName")}
              <input
                required
                value={customerFullName}
                onChange={(event) => setCustomerFullName(event.target.value)}
              />
            </label>
            <label>
              {translate("email")}
              <input
                required
                type="email"
                value={customerEmail}
                onChange={(event) => setCustomerEmail(event.target.value)}
              />
            </label>

            <div>
              <h2 style={{ margin: "8px 0 12px", fontSize: 16 }}>Delivery address</h2>
              {addresses.length === 0 ? (
                <p className="catalog-message">
                  No saved addresses.{" "}
                  <Link href="/account">Add one in your account</Link> first.
                </p>
              ) : (
                <div className="address-list">
                  {addresses.map((address) => (
                    <label key={address.id} className="address-card address-card-selectable">
                      <input
                        type="radio"
                        name="deliveryAddress"
                        checked={selectedAddressId === address.id}
                        onChange={() => setSelectedAddressId(address.id)}
                      />
                      <span>
                        <strong>
                          {address.label}
                          {address.isDefault ? " · Default" : ""}
                        </strong>
                        <br />
                        {address.line1}
                        {address.line2 ? `, ${address.line2}` : ""}
                        <br />
                        {address.city}
                        {address.region ? `, ${address.region}` : ""} {address.postalCode}
                        <br />
                        {address.countryCode}
                      </span>
                    </label>
                  ))}
                </div>
              )}
              <p className="catalog-message" style={{ marginTop: 12 }}>
                <Link href="/account">Manage addresses</Link>
              </p>
            </div>

            {error ? <p className="catalog-message catalog-error">{error}</p> : null}

            <button
              type="submit"
              className="button button-primary"
              disabled={isSubmitting || isLoading || addresses.length === 0}
            >
              {isSubmitting ? translate("placingOrder") : translate("placeOrder")}
            </button>
          </form>

          <aside className="cart-summary">
            <h2>{translate("orderSummary")}</h2>
            {cart?.items.map((item) => (
              <div key={item.id} className="checkout-summary-item">
                <span>
                  {item.productName} × {item.quantity}
                </span>
                <strong>{formatPrice(item.lineTotal, item.currency)}</strong>
              </div>
            ))}
            <div>
              <span>{translate("subtotal")}</span>
              <strong>{formatPrice(cart?.subtotal ?? 0, cart?.currency ?? "USD")}</strong>
            </div>
            <p>{translate("paymentStubNote")}</p>
          </aside>
        </div>
      </section>
    </PublicSiteLayout>
  );
}
