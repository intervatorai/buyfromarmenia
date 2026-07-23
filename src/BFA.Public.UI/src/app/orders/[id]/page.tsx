"use client";

import Link from "next/link";
import { useParams, useSearchParams } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useLanguage } from "@/components/providers/LanguageProvider";
import {
  ApiError,
  apiFetch,
  type PublicOrderDetail,
  type PublicSupplierFulfillment,
} from "@/lib/api";
import type { TranslationKey } from "@/lib/i18n";

function formatPrice(price: number, currency: string) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: currency || "USD",
  }).format(price);
}

const TRACKING_STAGE_KEYS = [
  "trackingStagePlaced",
  "trackingStageConfirmed",
  "trackingStagePreparing",
  "trackingStageAtWarehouse",
  "trackingStageShipped",
  "trackingStageInTransit",
  "trackingStageOutForDelivery",
  "trackingStageDelivered",
] as const satisfies TranslationKey[];

const SUPPLIER_STATUS_KEYS: Record<string, TranslationKey> = {
  New: "supplierStatusNew",
  Confirmed: "supplierStatusConfirmed",
  Preparing: "supplierStatusPreparing",
  ReadyForPickup: "supplierStatusReadyForPickup",
  TransferredToWarehouse: "supplierStatusTransferredToWarehouse",
  Cancelled: "supplierStatusCancelled",
};

const ORDER_STATUS_KEYS: Record<string, TranslationKey> = {
  Placed: "orderStatusPlaced",
  Confirmed: "orderStatusConfirmed",
  Cancelled: "orderStatusCancelled",
  Completed: "orderStatusCompleted",
};

const PAYMENT_STATUS_KEYS: Record<string, TranslationKey> = {
  Pending: "paymentStatusPending",
  Paid: "paymentStatusPaid",
  Failed: "paymentStatusFailed",
  Refunded: "paymentStatusRefunded",
};

const SUPPLIER_STAGE_RANK: Record<string, number> = {
  New: 0,
  Confirmed: 1,
  Preparing: 2,
  ReadyForPickup: 3,
  TransferredToWarehouse: 3,
  Cancelled: -1,
};

function getTrackingStage(
  order: PublicOrderDetail,
  shipmentStatus?: string,
) {
  const shipmentStages: Record<string, number> = {
    Created: 4,
    PickedUp: 4,
    InTransit: 5,
    OutForDelivery: 6,
    Delivered: 7,
  };
  if (shipmentStatus && shipmentStages[shipmentStatus] !== undefined) {
    return shipmentStages[shipmentStatus];
  }
  if (order.status === "Completed" || order.fulfillmentStatus === "Completed") {
    return 7;
  }

  const activeFulfillments = (order.supplierFulfillments ?? []).filter(
    (item) => item.status !== "Cancelled",
  );
  if (activeFulfillments.length > 0) {
    const minRank = Math.min(
      ...activeFulfillments.map(
        (item) => SUPPLIER_STAGE_RANK[item.status] ?? 0,
      ),
    );
    if (minRank >= 3) return 3;
    if (minRank >= 2) return 2;
    if (minRank >= 1) return 1;
    return order.paymentStatus === "Paid" || order.status === "Confirmed" ? 1 : 0;
  }

  if (order.fulfillmentStatus === "InProgress") return 2;
  if (order.status === "Confirmed") return 1;
  return 0;
}

export default function OrderDetailPage() {
  const params = useParams<{ id: string }>();
  const searchParams = useSearchParams();
  const { translate } = useLanguage();
  const [order, setOrder] = useState<PublicOrderDetail | null>(null);
  const [shipment, setShipment] = useState<{
    referenceNumber: string;
    carrier: string;
    trackingNumber: string;
    status: string;
  } | null>(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [returnReason, setReturnReason] = useState("");
  const [returnMessage, setReturnMessage] = useState("");
  const [isSubmittingReturn, setIsSubmittingReturn] = useState(false);
  const checkoutFlag = searchParams.get("checkout");

  const loadOrder = useCallback(async () => {
    if (!params.id) return;
    setOrder(await apiFetch<PublicOrderDetail>(`/api/orders/${params.id}`));
    try {
      setShipment(await apiFetch(`/api/orders/${params.id}/shipment`));
    } catch {
      setShipment(null);
    }
  }, [params.id]);

  useEffect(() => {
    async function initialLoad() {
      if (!params.id) return;
      try {
        await loadOrder();
      } catch (err) {
        setError(err instanceof ApiError ? err.message : "Failed to load order.");
      } finally {
        setIsLoading(false);
      }
    }

    void initialLoad();
  }, [params.id, loadOrder]);

  useEffect(() => {
    if (checkoutFlag !== "success" || !order || order.paymentStatus === "Paid") {
      return;
    }

    const timer = window.setInterval(() => {
      void loadOrder().catch(() => undefined);
    }, 2000);

    return () => window.clearInterval(timer);
  }, [checkoutFlag, order, loadOrder]);

  async function handleReturnRequest(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!order) {
      return;
    }

    setIsSubmittingReturn(true);
    setReturnMessage("");

    try {
      await apiFetch("/api/returns", {
        method: "POST",
        body: JSON.stringify({
          customerOrderId: order.id,
          customerEmail: order.customerEmail,
          reason: returnReason,
        }),
      });
      setReturnMessage(translate("returnRequestSubmitted"));
      setReturnReason("");
    } catch (err) {
      setReturnMessage(
        err instanceof ApiError ? err.message : translate("returnRequestFailed"),
      );
    } finally {
      setIsSubmittingReturn(false);
    }
  }

  function labelOrderStatus(status: string) {
    const key = ORDER_STATUS_KEYS[status];
    return key ? translate(key) : status;
  }

  function labelPaymentStatus(status: string) {
    const key = PAYMENT_STATUS_KEYS[status];
    return key ? translate(key) : status;
  }

  function labelSupplierStatus(status: string) {
    const key = SUPPLIER_STATUS_KEYS[status];
    return key ? translate(key) : status;
  }

  function renderSellerBlock(fulfillment: PublicSupplierFulfillment, index: number) {
    return (
      <div key={`${fulfillment.status}-${index}`} className="seller-progress-item">
        <div className="seller-progress-header">
          <strong>
            {translate("sellerLabel")} {index + 1}
          </strong>
          <span className={`order-status seller-${fulfillment.status.toLowerCase()}`}>
            {labelSupplierStatus(fulfillment.status)}
          </span>
        </div>
        {fulfillment.productNames.length > 0 ? (
          <p className="seller-progress-products">
            {fulfillment.productNames.join(", ")}
          </p>
        ) : (
          <p className="seller-progress-products">
            {fulfillment.itemsCount} {translate("items")}
          </p>
        )}
      </div>
    );
  }

  return (
    <PublicSiteLayout>
      <section className="section container catalog-page">
        <Link href="/orders" className="catalog-back-link">
          ← {translate("backToOrders")}
        </Link>

        {isLoading ? <p className="catalog-message">{translate("loadingOrders")}</p> : null}
        {error ? <p className="catalog-message catalog-error">{error}</p> : null}

        {order ? (
          <div className="order-detail">
            <div>
              <p className="eyebrow">{translate("orderNumber")}</p>
              <h1>{order.orderNumber}</h1>
              <p className="order-detail-meta">
                {translate("placedOn")}{" "}
                {new Date(order.createdAtUtc).toLocaleString("en-GB")}
              </p>
              <div className="order-status-row">
                <span className="order-status">{labelOrderStatus(order.status)}</span>
                <span className="order-status">
                  {labelPaymentStatus(order.paymentStatus)}
                </span>
              </div>
              {order.paymentStatus === "Pending" ? (
                <p className="catalog-message">
                  {checkoutFlag === "success"
                    ? "Payment received — confirming your order…"
                    : "Waiting for payment. If you closed Stripe Checkout, restart checkout from your cart."}
                </p>
              ) : null}
              <div className="tracking-timeline" aria-label="Order tracking">
                {TRACKING_STAGE_KEYS.map((stageKey, index) => {
                  const currentStage = getTrackingStage(order, shipment?.status);
                  return (
                    <div
                      key={stageKey}
                      className={`tracking-step${index <= currentStage ? " complete" : ""}${
                        index === currentStage ? " current" : ""
                      }`}
                    >
                      <span className="tracking-dot" aria-hidden="true" />
                      <span>{translate(stageKey)}</span>
                    </div>
                  );
                })}
              </div>
              {order.paymentReference ? (
                <p className="order-detail-meta">
                  Payment ref: {order.paymentReference}
                </p>
              ) : null}
            </div>

            <div className="order-detail-grid">
              <div className="product-detail-block">
                <h2>{translate("deliveryDetails")}</h2>
                <p>
                  {order.customerFullName}
                  <br />
                  {order.customerEmail}
                  <br />
                  {order.shippingAddress.line1}
                  {order.shippingAddress.line2
                    ? `, ${order.shippingAddress.line2}`
                    : ""}
                  <br />
                  {order.shippingAddress.city}, {order.shippingAddress.region}{" "}
                  {order.shippingAddress.postalCode}
                  <br />
                  {order.shippingAddress.countryCode}
                </p>
              </div>

              <div className="product-detail-block">
                <h2>{translate("orderSummary")}</h2>
                {order.items.map((item) => (
                  <div key={item.id} className="checkout-summary-item">
                    <span>
                      {item.productName} ({item.supplierSku}) × {item.quantity}
                    </span>
                    <strong>{formatPrice(item.lineTotal, item.currency)}</strong>
                  </div>
                ))}
                <div className="checkout-summary-total">
                  <span>{translate("subtotal")}</span>
                  <strong>{formatPrice(order.subtotal, order.currency)}</strong>
                </div>
              </div>

              {(order.supplierFulfillments?.length ?? 0) > 0 ? (
                <div className="product-detail-block" style={{ marginTop: 24 }}>
                  <h2>{translate("sellerProgress")}</h2>
                  <div className="seller-progress-list">
                    {order.supplierFulfillments.map((fulfillment, index) =>
                      renderSellerBlock(fulfillment, index),
                    )}
                  </div>
                </div>
              ) : null}

              {shipment ? (
                <div className="product-detail-block" style={{ marginTop: 24 }}>
                  <h2>{translate("shipmentTracking")}</h2>
                  <p>
                    {shipment.carrier} — {shipment.trackingNumber}
                    <br />
                    Status: {shipment.status}
                  </p>
                </div>
              ) : null}

              <div className="product-detail-block" style={{ marginTop: 24 }}>
                <h2>{translate("requestReturn")}</h2>
                <form className="checkout-form" onSubmit={(event) => void handleReturnRequest(event)}>
                  <label>
                    {translate("returnReason")}
                    <textarea
                      required
                      rows={4}
                      value={returnReason}
                      onChange={(event) => setReturnReason(event.target.value)}
                    />
                  </label>
                  <button
                    type="submit"
                    className="button button-secondary"
                    disabled={isSubmittingReturn}
                  >
                    {isSubmittingReturn
                      ? translate("submittingReturn")
                      : translate("submitReturn")}
                  </button>
                </form>
                {returnMessage ? (
                  <p className="catalog-message" style={{ marginTop: 12 }}>
                    {returnMessage}
                  </p>
                ) : null}
              </div>
            </div>
          </div>
        ) : null}
      </section>
    </PublicSiteLayout>
  );
}
