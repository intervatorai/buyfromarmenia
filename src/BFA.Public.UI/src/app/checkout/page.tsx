"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useMemo, useState } from "react";
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

function formatAddressLines(address: CustomerDeliveryAddress) {
  const line = [
    address.line1,
    address.line2,
    [address.city, address.region, address.postalCode].filter(Boolean).join(", "),
    address.countryCode,
  ]
    .filter(Boolean)
    .join(", ");
  return line;
}

type ShippingQuote = {
  estimatedWeightKg: number;
  basePrice: number;
  errorMarginPercent: number;
  shippingFee: number;
  currency: string;
  bracketId: string;
  countryIsoCode: string;
};

export default function CheckoutPage() {
  const router = useRouter();
  const { translate } = useLanguage();
  const { user, isAuthenticated, isLoading: isAuthLoading } = useAuth();
  const [cart, setCart] = useState<PublicCart | null>(null);
  const [addresses, setAddresses] = useState<CustomerDeliveryAddress[]>([]);
  const [selectedAddressId, setSelectedAddressId] = useState("");
  const [isAddressModalOpen, setIsAddressModalOpen] = useState(false);
  const [pendingAddressId, setPendingAddressId] = useState("");
  const [quote, setQuote] = useState<ShippingQuote | null>(null);
  const [quoteError, setQuoteError] = useState("");
  const [isQuoteLoading, setIsQuoteLoading] = useState(false);
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
      if ((cartData.removedUnavailableItems ?? 0) > 0) {
        notifyCartUpdated();
        setError(
          "Some items are no longer available and were removed from your cart.",
        );
      }
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

  useEffect(() => {
    if (!isAddressModalOpen) {
      return;
    }

    document.body.classList.add("modal-open");
    return () => document.body.classList.remove("modal-open");
  }, [isAddressModalOpen]);

  useEffect(() => {
    async function loadQuote() {
      if (!selectedAddressId || !cart || cart.items.length === 0) {
        setQuote(null);
        setQuoteError("");
        return;
      }

      setIsQuoteLoading(true);
      setQuoteError("");
      try {
        const data = await apiFetch<ShippingQuote>(
          `/api/shipping/quote?cartId=${encodeURIComponent(getCartId())}&deliveryAddressId=${encodeURIComponent(selectedAddressId)}`,
        );
        setQuote(data);
        // Refresh cart in case unavailable variants were purged during quote.
        const refreshed = await apiFetch<PublicCart>(`/api/carts/${getCartId()}`);
        setCart(refreshed);
        if ((refreshed.removedUnavailableItems ?? 0) > 0) {
          notifyCartUpdated();
          setError(
            "Some items are no longer available and were removed from your cart.",
          );
        }
      } catch (err) {
        setQuote(null);
        const message =
          err instanceof ApiError ? err.message : "Failed to calculate shipping.";
        setQuoteError(message);
        try {
          const refreshed = await apiFetch<PublicCart>(`/api/carts/${getCartId()}`);
          setCart(refreshed);
          if ((refreshed.removedUnavailableItems ?? 0) > 0 || refreshed.items.length === 0) {
            notifyCartUpdated();
          }
        } catch {
          // ignore refresh failure
        }
      } finally {
        setIsQuoteLoading(false);
      }
    }

    void loadQuote();
  }, [selectedAddressId, cart?.id, cart?.items.length]);

  const selectedAddress = useMemo(
    () => addresses.find((address) => address.id === selectedAddressId) ?? null,
    [addresses, selectedAddressId],
  );

  const orderTotal = useMemo(() => {
    const subtotal = cart?.subtotal ?? 0;
    const shipping = quote?.shippingFee ?? 0;
    return subtotal + shipping;
  }, [cart?.subtotal, quote?.shippingFee]);

  function openAddressModal() {
    setPendingAddressId(selectedAddressId);
    setIsAddressModalOpen(true);
  }

  function closeAddressModal() {
    setIsAddressModalOpen(false);
  }

  function confirmAddressSelection() {
    if (pendingAddressId) {
      setSelectedAddressId(pendingAddressId);
    }
    setIsAddressModalOpen(false);
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selectedAddressId) {
      setError("Add a shipping address in My account → Shipping addresses before placing an order.");
      return;
    }
    if (!quote) {
      setError(quoteError || "Shipping quote is required before placing an order.");
      return;
    }

    setIsSubmitting(true);
    setError("");

    try {
      const result = await apiFetch<{
        orderId: string;
        orderNumber: string;
        checkoutUrl?: string | null;
      }>("/api/orders", {
        method: "POST",
        body: JSON.stringify({
          cartId: getCartId(),
          customerEmail,
          customerFullName,
          deliveryAddressId: selectedAddressId,
        }),
      });
      notifyCartUpdated();
      if (result.checkoutUrl) {
        window.location.href = result.checkoutUrl;
        return;
      }
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

            <div className="checkout-address-block">
              <div className="checkout-address-heading">
                <h2>{translate("deliveryAddress")}</h2>
                {addresses.length > 1 ? (
                  <button
                    type="button"
                    className="button button-secondary checkout-change-address"
                    onClick={openAddressModal}
                  >
                    {translate("changeAddress")}
                  </button>
                ) : null}
              </div>

              {addresses.length === 0 ? (
                <p className="catalog-message">
                  {translate("noSavedAddresses")}{" "}
                  <Link href="/account/addresses">{translate("addAddressBeforeOrder")}</Link>
                </p>
              ) : selectedAddress ? (
                <div className="address-card checkout-selected-address">
                  <span>
                    <strong>
                      {selectedAddress.label}
                      {selectedAddress.isDefault
                        ? ` · ${translate("defaultAddress")}`
                        : ""}
                    </strong>
                    <p>{formatAddressLines(selectedAddress)}</p>
                  </span>
                </div>
              ) : null}

              <p className="catalog-message checkout-manage-addresses">
                <Link href="/account/addresses">{translate("manageAddresses")}</Link>
              </p>
            </div>

            {error ? <p className="catalog-message catalog-error">{error}</p> : null}
            {quoteError ? <p className="catalog-message catalog-error">{quoteError}</p> : null}

            <button
              type="submit"
              className="button button-primary"
              disabled={
                isSubmitting ||
                isLoading ||
                isQuoteLoading ||
                addresses.length === 0 ||
                !quote
              }
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
            <div>
              <span>{translate("shipping")}</span>
              <strong>
                {isQuoteLoading
                  ? translate("calculatingShipping")
                  : quote
                    ? formatPrice(quote.shippingFee, quote.currency)
                    : "—"}
              </strong>
            </div>
            <div className="checkout-summary-total">
              <span>{translate("total")}</span>
              <strong>
                {formatPrice(orderTotal, quote?.currency ?? cart?.currency ?? "USD")}
              </strong>
            </div>
            <p>{translate("paymentStubNote")}</p>
          </aside>
        </div>
      </section>

      {isAddressModalOpen ? (
        <div
          className="seller-thankyou-overlay"
          role="presentation"
          onClick={(event) => {
            if (event.target === event.currentTarget) {
              closeAddressModal();
            }
          }}
        >
          <div
            className="checkout-address-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="checkout-address-modal-title"
          >
            <div className="checkout-address-modal-header">
              <h2 id="checkout-address-modal-title">
                {translate("selectDeliveryAddress")}
              </h2>
              <button
                type="button"
                className="checkout-address-modal-close"
                onClick={closeAddressModal}
                aria-label={translate("cancel")}
              >
                ×
              </button>
            </div>

            <div className="address-list checkout-address-modal-list">
              {addresses.map((address) => (
                <label
                  key={address.id}
                  className={
                    pendingAddressId === address.id
                      ? "address-card address-card-selectable address-card-selected"
                      : "address-card address-card-selectable"
                  }
                >
                  <input
                    type="radio"
                    name="checkoutAddressPicker"
                    checked={pendingAddressId === address.id}
                    onChange={() => setPendingAddressId(address.id)}
                  />
                  <span>
                    <strong>
                      {address.label}
                      {address.isDefault ? ` · ${translate("defaultAddress")}` : ""}
                    </strong>
                    <p>{formatAddressLines(address)}</p>
                  </span>
                </label>
              ))}
            </div>

            <div className="checkout-address-modal-actions">
              <Link href="/account/addresses" className="catalog-message">
                {translate("manageAddresses")}
              </Link>
              <div className="checkout-address-modal-buttons">
                <button
                  type="button"
                  className="button button-secondary"
                  onClick={closeAddressModal}
                >
                  {translate("cancel")}
                </button>
                <button
                  type="button"
                  className="button button-primary"
                  disabled={!pendingAddressId}
                  onClick={confirmAddressSelection}
                >
                  {translate("useThisAddress")}
                </button>
              </div>
            </div>
          </div>
        </div>
      ) : null}
    </PublicSiteLayout>
  );
}
