"use client";

import Link from "next/link";
import { useParams, useSearchParams } from "next/navigation";
import { Suspense, useCallback, useEffect, useState } from "react";
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

const SHIPMENT_STATUS_KEYS: Record<string, TranslationKey> = {
  Created: "shipmentStatusCreated",
  PickedUp: "shipmentStatusPickedUp",
  InTransit: "shipmentStatusInTransit",
  OutForDelivery: "shipmentStatusOutForDelivery",
  Delivered: "shipmentStatusDelivered",
};

const TRACKING_STAGE_INDEX: Record<string, number> = {
  OrderPlaced: 0,
  Confirmed: 1,
  BeingPrepared: 2,
  AtWarehouse: 3,
  Shipped: 4,
  InTransit: 5,
  OutForDelivery: 6,
  Delivered: 7,
};

function getTrackingStage(
  order: PublicOrderDetail,
  shipmentStatus?: string,
) {
  if (order.trackingStage && TRACKING_STAGE_INDEX[order.trackingStage] !== undefined) {
    return TRACKING_STAGE_INDEX[order.trackingStage];
  }

  const shipmentStages: Record<string, number> = {
    Created: 3,
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
    if (order.paymentStatus === "Paid" || order.status === "Confirmed") return 1;
    return 0;
  }

  if (order.fulfillmentStatus === "InProgress") return 2;
  if (order.status === "Confirmed" || order.paymentStatus === "Paid") return 1;
  return 0;
}

function OrderDetailPageContent() {
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

  function labelShipmentStatus(status: string) {
    const key = SHIPMENT_STATUS_KEYS[status];
    return key ? translate(key) : status.replace(/([a-z])([A-Z])/g, "$1 $2");
  }

  function renderSellerBlock(fulfillment: PublicSupplierFulfillment, index: number) {
    return (
      <div key={`${fulfillment.status}-${index}`} className="order-panel-row">
        <div>
          <div className="order-panel-row-title">
            {translate("sellerLabel")} {index + 1}
          </div>
          {fulfillment.productNames.length > 0 ? (
            <p className="order-panel-row-meta">{fulfillment.productNames.join(", ")}</p>
          ) : (
            <p className="order-panel-row-meta">
              {fulfillment.itemsCount} {translate("items")}
            </p>
          )}
        </div>
        <span className={`order-status seller-${fulfillment.status.toLowerCase()}`}>
          {labelSupplierStatus(fulfillment.status)}
        </span>
      </div>
    );
  }

  const currentStage = order
    ? getTrackingStage(order, shipment?.status)
    : 0;
  const currentStageLabel = order
    ? translate(TRACKING_STAGE_KEYS[currentStage])
    : "";

  return (
    <PublicSiteLayout>
      <section className="section container order-page">
        <Link href="/orders" className="catalog-back-link">
          ← {translate("backToOrders")}
        </Link>

        {isLoading ? <p className="catalog-message">{translate("loadingOrders")}</p> : null}
        {error ? <p className="catalog-message catalog-error">{error}</p> : null}

        {order ? (
          <div className="order-view">
            <header className="order-view-header">
              <div>
                <p className="eyebrow">{translate("orderNumber")}</p>
                <h1>{order.orderNumber}</h1>
                <p className="order-view-date">
                  {translate("placedOn")}{" "}
                  {new Date(order.createdAtUtc).toLocaleString("en-GB")}
                </p>
              </div>
              <div className="order-view-badges">
                <span className="order-status">{labelOrderStatus(order.status)}</span>
                <span className="order-status">
                  {labelPaymentStatus(order.paymentStatus)}
                </span>
              </div>
            </header>

            {order.paymentStatus === "Pending" ? (
              <p className="order-alert">
                {checkoutFlag === "success"
                  ? "Payment received — confirming your order…"
                  : "Waiting for payment. If you closed Stripe Checkout, restart checkout from your cart."}
              </p>
            ) : null}

            <div className="order-panel order-progress-panel">
              <div className="order-progress-hero">
                <span className="order-progress-label">
                  {translate("currentTrackingStatus")}
                </span>
                <strong className="order-progress-current">{currentStageLabel}</strong>
                <div
                  className="order-progress-meter"
                  aria-hidden="true"
                >
                  {TRACKING_STAGE_KEYS.map((_, index) => (
                    <span
                      key={index}
                      className={`order-progress-seg${
                        index <= currentStage ? " on" : ""
                      }${index === currentStage ? " now" : ""}`}
                    />
                  ))}
                </div>
              </div>

              <ol className="order-progress-list" aria-label={translate("orderProgress")}>
                {TRACKING_STAGE_KEYS.map((stageKey, index) => (
                  <li
                    key={stageKey}
                    className={`order-progress-item${
                      index < currentStage ? " done" : ""
                    }${index === currentStage ? " now" : ""}`}
                  >
                    <span className="order-progress-marker" aria-hidden="true" />
                    <span className="order-progress-text">{translate(stageKey)}</span>
                  </li>
                ))}
              </ol>
            </div>

            <div className="order-view-grid">
              <div className="order-panel">
                <h2>{translate("deliveryDetails")}</h2>
                <dl className="order-facts">
                  <div>
                    <dt>{translate("fullName")}</dt>
                    <dd>{order.customerFullName}</dd>
                  </div>
                  <div>
                    <dt>{translate("email")}</dt>
                    <dd>{order.customerEmail}</dd>
                  </div>
                  <div>
                    <dt>{translate("deliveryAddress")}</dt>
                    <dd>
                      {order.shippingAddress.line1}
                      {order.shippingAddress.line2
                        ? `, ${order.shippingAddress.line2}`
                        : ""}
                      <br />
                      {order.shippingAddress.city}
                      {order.shippingAddress.region
                        ? `, ${order.shippingAddress.region}`
                        : ""}{" "}
                      {order.shippingAddress.postalCode}
                      <br />
                      {order.shippingAddress.countryCode}
                    </dd>
                  </div>
                </dl>
              </div>

              <div className="order-panel">
                <h2>{translate("orderSummary")}</h2>
                <ul className="order-lines">
                  {order.items.map((item) => (
                    <li key={item.id}>
                      <div className="order-line-main">
                        <strong>{item.productName}</strong>
                        <span>
                          {item.supplierSku} · ×{item.quantity}
                        </span>
                      </div>
                      <strong className="order-line-price">
                        {formatPrice(item.lineTotal, item.currency)}
                      </strong>
                    </li>
                  ))}
                </ul>
                <div className="order-total-row">
                  <span>{translate("subtotal")}</span>
                  <strong>{formatPrice(order.subtotal, order.currency)}</strong>
                </div>
                {order.paymentReference ? (
                  <p className="order-ref">
                    {translate("paymentReference")}: {order.paymentReference}
                  </p>
                ) : null}
              </div>
            </div>

            {(order.supplierFulfillments?.length ?? 0) > 0 ? (
              <div className="order-panel">
                <h2>{translate("sellerProgress")}</h2>
                <div className="order-panel-stack">
                  {order.supplierFulfillments.map((fulfillment, index) =>
                    renderSellerBlock(fulfillment, index),
                  )}
                </div>
              </div>
            ) : null}

            {shipment ? (
              <div className="order-panel">
                <h2>{translate("shipmentTracking")}</h2>
                <dl className="order-facts">
                  <div>
                    <dt>{translate("shipmentStatusLabel")}</dt>
                    <dd>{labelShipmentStatus(shipment.status)}</dd>
                  </div>
                  <div>
                    <dt>{translate("shipmentTrackingNumber")}</dt>
                    <dd>
                      {shipment.carrier} · {shipment.trackingNumber}
                    </dd>
                  </div>
                </dl>
              </div>
            ) : null}

            <div className="order-panel">
              <h2>{translate("requestReturn")}</h2>
              <form
                className="checkout-form order-return-form"
                onSubmit={(event) => void handleReturnRequest(event)}
              >
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
        ) : null}
      </section>
    </PublicSiteLayout>
  );
}

export default function OrderDetailPage() {
  return (
    <Suspense fallback={null}>
      <OrderDetailPageContent />
    </Suspense>
  );
}
