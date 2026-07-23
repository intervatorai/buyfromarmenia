"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { AdminShell } from "@/components/layout/AdminShell";
import { ApiError, apiFetch } from "@/lib/api";

type OrderDetail = {
  id: string;
  orderNumber: string;
  customerEmail: string;
  customerFullName: string;
  status: string;
  paymentStatus: string;
  fulfillmentStatus: string;
  subtotal: number;
  estimatedWeightKg: number;
  shippingFeeQuoted: number;
  shippingMarginPercent: number;
  shippingFee: number;
  total: number;
  shippingAdjustmentReason?: string | null;
  currency: string;
  createdAtUtc: string;
  shippingAddress: {
    countryCode: string;
    city: string;
    line1: string;
    line2?: string | null;
    postalCode?: string | null;
    region?: string | null;
  };
  items: Array<{
    productId: string;
    productName: string;
    supplierSku: string;
    supplierId: string;
    unitPrice: number;
    currency: string;
    quantity: number;
    lineTotal: number;
  }>;
  supplierOrders: Array<{
    id: string;
    supplierId: string;
    status: string;
    subtotal: number;
    currency: string;
    createdAtUtc: string;
    items: Array<{
      productName: string;
      supplierSku: string;
      unitPrice: number;
      quantity: number;
    }>;
  }>;
  shipment?: {
    id: string;
    referenceNumber: string;
    carrier: string;
    trackingNumber: string;
    status: string;
    createdAtUtc: string;
    updatedAtUtc: string;
  } | null;
};

const SHIPMENT_STATUSES = [
  "Created",
  "PickedUp",
  "InTransit",
  "OutForDelivery",
  "Delivered",
] as const;

export default function OrderDetailPage() {
  const params = useParams<{ id: string }>();
  const [order, setOrder] = useState<OrderDetail | null>(null);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [actualWeightKg, setActualWeightKg] = useState("");
  const [manualFee, setManualFee] = useState("");
  const [reason, setReason] = useState("");
  const [isAdjusting, setIsAdjusting] = useState(false);
  const [orderStatus, setOrderStatus] = useState("");
  const [paymentStatus, setPaymentStatus] = useState("");
  const [isUpdatingStatus, setIsUpdatingStatus] = useState(false);
  const [shipmentStatus, setShipmentStatus] = useState("");
  const [isUpdatingShipment, setIsUpdatingShipment] = useState(false);

  async function load() {
    try {
      const data = await apiFetch<OrderDetail>(`/api/orders/${params.id}`);
      setOrder(data);
      setActualWeightKg(String(data.estimatedWeightKg || ""));
      setManualFee(String(data.shippingFee));
      setOrderStatus(data.status);
      setPaymentStatus(data.paymentStatus);
      setShipmentStatus(data.shipment?.status ?? "");
      setError("");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load order.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, [params.id]);

  async function adjustShipping(event: FormEvent, mode: "weight" | "manual") {
    event.preventDefault();
    if (!order) {
      return;
    }

    setIsAdjusting(true);
    setError("");
    setMessage("");

    try {
      const body =
        mode === "weight"
          ? {
              actualWeightKg: Number(actualWeightKg),
              manualShippingFee: null,
              reason: reason || "Recalculated from actual weight",
            }
          : {
              actualWeightKg: null,
              manualShippingFee: Number(manualFee),
              reason: reason || "Manual shipping adjustment",
            };

      await apiFetch(`/api/orders/${order.id}/adjust-shipping`, {
        method: "POST",
        body: JSON.stringify(body),
      });
      setMessage("Shipping fee updated.");
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to adjust shipping.");
    } finally {
      setIsAdjusting(false);
    }
  }

  async function updateStatus(event: FormEvent) {
    event.preventDefault();
    if (!order) return;

    setIsUpdatingStatus(true);
    setError("");
    setMessage("");
    try {
      const body: {
        orderStatus?: string;
        paymentStatus?: string;
      } = {};
      if (orderStatus !== order.status) {
        body.orderStatus = orderStatus;
      }
      if (paymentStatus !== order.paymentStatus) {
        body.paymentStatus = paymentStatus;
      }
      if (!body.orderStatus && !body.paymentStatus) {
        setMessage("No status changes to save.");
        return;
      }

      await apiFetch(`/api/orders/${order.id}/status`, {
        method: "PUT",
        body: JSON.stringify(body),
      });
      setMessage("Order status updated.");
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update status.");
    } finally {
      setIsUpdatingStatus(false);
    }
  }

  async function updateShipmentStatus(event: FormEvent) {
    event.preventDefault();
    if (!order?.shipment) return;

    setIsUpdatingShipment(true);
    setError("");
    setMessage("");
    try {
      if (shipmentStatus === order.shipment.status) {
        setMessage("Shipment status is unchanged.");
        return;
      }

      await apiFetch(`/api/logistics/shipments/${order.shipment.id}/status`, {
        method: "PUT",
        body: JSON.stringify({ status: shipmentStatus }),
      });
      setMessage(
        shipmentStatus === "Delivered"
          ? "Shipment marked Delivered. Order completed."
          : "Shipment status updated.",
      );
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update shipment status.");
    } finally {
      setIsUpdatingShipment(false);
    }
  }

  return (
    <AdminShell title={order ? `Order ${order.orderNumber}` : "Order"}>
      <p style={{ marginBottom: 16 }}>
        <Link href="/orders" className="button-ghost">
          ← Back to orders
        </Link>
      </p>

      {error ? <p className="form-error">{error}</p> : null}
      {message ? <p className="form-success">{message}</p> : null}
      {isLoading ? <p>Loading...</p> : null}

      {order ? (
        <>
          <div className="admin-grid" style={{ marginBottom: 24 }}>
            <div className="admin-card">
              <div className="admin-card-label">Customer</div>
              <div className="admin-card-value" style={{ fontSize: 18 }}>
                {order.customerFullName}
              </div>
              <div style={{ color: "var(--admin-muted)", marginTop: 8 }}>{order.customerEmail}</div>
            </div>
            <div className="admin-card">
              <div className="admin-card-label">Statuses</div>
              <div>
                {order.status} · {order.paymentStatus} · {order.fulfillmentStatus}
              </div>
              <form className="order-status-form" onSubmit={(event) => void updateStatus(event)}>
                <div className="form-field">
                  <label htmlFor="order-status">Order status</label>
                  <select
                    id="order-status"
                    className="form-control"
                    value={orderStatus}
                    onChange={(event) => setOrderStatus(event.target.value)}
                  >
                    <option value="Placed">Placed</option>
                    <option value="Confirmed">Confirmed</option>
                    <option value="Completed">Completed</option>
                    <option value="Cancelled">Cancelled</option>
                  </select>
                </div>
                <div className="form-field">
                  <label htmlFor="payment-status">Payment status</label>
                  <select
                    id="payment-status"
                    className="form-control"
                    value={paymentStatus}
                    onChange={(event) => setPaymentStatus(event.target.value)}
                  >
                    <option value="Pending">Pending</option>
                    <option value="Paid">Paid</option>
                    <option value="Failed">Failed</option>
                    <option value="Refunded">Refunded</option>
                  </select>
                </div>
                <p className="order-status-hint">
                  Confirming or completing an unpaid order marks payment as Paid.
                  Fulfillment updates automatically with order status.
                </p>
                <button
                  type="submit"
                  className="button-primary"
                  disabled={isUpdatingStatus}
                >
                  {isUpdatingStatus ? "Saving…" : "Update status"}
                </button>
              </form>
            </div>
            <div className="admin-card">
              <div className="admin-card-label">Totals</div>
              <div>Subtotal: {order.subtotal.toFixed(2)} {order.currency}</div>
              <div>
                Shipping: {order.shippingFee.toFixed(2)} {order.currency}
                {order.shippingFee !== order.shippingFeeQuoted
                  ? ` (quoted ${order.shippingFeeQuoted.toFixed(2)})`
                  : ""}
              </div>
              <div className="admin-card-value" style={{ fontSize: 18, marginTop: 8 }}>
                Total: {order.total.toFixed(2)} {order.currency}
              </div>
              <div style={{ color: "var(--admin-muted)", marginTop: 8, fontSize: 13 }}>
                Est. weight {order.estimatedWeightKg} kg · margin {order.shippingMarginPercent}%
              </div>
              {order.shippingAdjustmentReason ? (
                <div style={{ marginTop: 8, fontSize: 13 }}>
                  Adjustment: {order.shippingAdjustmentReason}
                </div>
              ) : null}
            </div>
            <div className="admin-card">
              <div className="admin-card-label">Ship to</div>
              <div>
                {order.shippingAddress.line1}
                {order.shippingAddress.line2 ? `, ${order.shippingAddress.line2}` : ""}
                <br />
                {order.shippingAddress.city}
                {order.shippingAddress.postalCode ? ` ${order.shippingAddress.postalCode}` : ""}
                <br />
                {order.shippingAddress.countryCode}
              </div>
            </div>
          </div>

          <div className="admin-card shipping-status-card">
            <div className="shipping-adjust-header">
              <h2>Shipment tracking</h2>
              <p>
                Carrier pipeline status for the international shipment. Moving to
                Delivered also completes the order.
              </p>
            </div>

            {order.shipment ? (
              <form
                className="shipping-status-form"
                onSubmit={(event) => void updateShipmentStatus(event)}
              >
                <div className="shipping-status-meta">
                  <div>
                    <span className="admin-card-label">Reference</span>
                    <div>{order.shipment.referenceNumber}</div>
                  </div>
                  <div>
                    <span className="admin-card-label">Tracking</span>
                    <div>{order.shipment.trackingNumber}</div>
                  </div>
                  <div>
                    <span className="admin-card-label">Carrier</span>
                    <div>{order.shipment.carrier}</div>
                  </div>
                  <div>
                    <span className="admin-card-label">Current</span>
                    <div>{order.shipment.status}</div>
                  </div>
                </div>

                <div className="form-field">
                  <label htmlFor="shipment-status">Shipment status</label>
                  <select
                    id="shipment-status"
                    className="form-control"
                    value={shipmentStatus}
                    onChange={(event) => setShipmentStatus(event.target.value)}
                  >
                    {SHIPMENT_STATUSES.map((status) => {
                      const currentIndex = SHIPMENT_STATUSES.indexOf(
                        order.shipment!.status as (typeof SHIPMENT_STATUSES)[number],
                      );
                      const optionIndex = SHIPMENT_STATUSES.indexOf(status);
                      const disabled = optionIndex < currentIndex;
                      return (
                        <option key={status} value={status} disabled={disabled}>
                          {status}
                          {disabled ? " (past)" : ""}
                        </option>
                      );
                    })}
                  </select>
                </div>
                <p className="order-status-hint">
                  Status can only move forward: Created → PickedUp → InTransit →
                  OutForDelivery → Delivered.
                </p>
                <button
                  type="submit"
                  className="button-primary"
                  disabled={isUpdatingShipment || order.shipment.status === "Delivered"}
                >
                  {isUpdatingShipment ? "Saving…" : "Update shipment status"}
                </button>
              </form>
            ) : (
              <p className="order-status-hint" style={{ marginBottom: 0 }}>
                No international shipment yet. Create one from{" "}
                <Link href="/logistics">Logistics</Link> after the consolidation is
                sealed.
              </p>
            )}
          </div>

          <div className="admin-card shipping-adjust">
            <div className="shipping-adjust-header">
              <h2>Adjust shipping</h2>
              <p>
                After warehouse weighing, recalculate from rate tables or set a
                manual fee.
              </p>
            </div>

            <div className="shipping-adjust-grid">
              <form
                className="shipping-adjust-block"
                onSubmit={(event) => void adjustShipping(event, "weight")}
              >
                <div className="shipping-adjust-block-title">From weight</div>
                <p className="shipping-adjust-block-hint">
                  Recalculate shipping from rate brackets using the actual package
                  weight.
                </p>
                <div className="form-field">
                  <label htmlFor="actual-weight">Actual weight (kg)</label>
                  <input
                    id="actual-weight"
                    className="form-control"
                    type="number"
                    min={0}
                    step="0.001"
                    value={actualWeightKg}
                    onChange={(event) => setActualWeightKg(event.target.value)}
                  />
                </div>
                <div className="form-field">
                  <label htmlFor="weight-reason">Reason</label>
                  <input
                    id="weight-reason"
                    className="form-control"
                    value={reason}
                    onChange={(event) => setReason(event.target.value)}
                    placeholder="Optional"
                  />
                </div>
                <button
                  type="submit"
                  className="button-primary shipping-adjust-action"
                  disabled={isAdjusting}
                >
                  Recalculate from weight
                </button>
              </form>

              <form
                className="shipping-adjust-block"
                onSubmit={(event) => void adjustShipping(event, "manual")}
              >
                <div className="shipping-adjust-block-title">Manual fee</div>
                <p className="shipping-adjust-block-hint">
                  Override the shipping fee directly when needed.
                </p>
                <div className="form-field">
                  <label htmlFor="manual-fee">Shipping fee ({order.currency})</label>
                  <input
                    id="manual-fee"
                    className="form-control"
                    type="number"
                    min={0}
                    step="0.01"
                    value={manualFee}
                    onChange={(event) => setManualFee(event.target.value)}
                  />
                </div>
                <div className="form-field">
                  <label htmlFor="manual-reason">Reason</label>
                  <input
                    id="manual-reason"
                    className="form-control"
                    value={reason}
                    onChange={(event) => setReason(event.target.value)}
                    placeholder="Optional"
                  />
                </div>
                <button
                  type="submit"
                  className="button-secondary shipping-adjust-action"
                  disabled={isAdjusting}
                >
                  Set manual fee
                </button>
              </form>
            </div>
          </div>


          <h2 style={{ marginBottom: 12 }}>Line items</h2>
          <div className="admin-table-wrap" style={{ marginBottom: 24 }}>
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>SKU</th>
                  <th>Supplier</th>
                  <th>Qty</th>
                  <th>Line total</th>
                </tr>
              </thead>
              <tbody>
                {order.items.map((item, index) => (
                  <tr key={`${item.productId}-${index}`}>
                    <td>{item.productName}</td>
                    <td>{item.supplierSku}</td>
                    <td>{item.supplierId.slice(0, 8)}</td>
                    <td>{item.quantity}</td>
                    <td>
                      {item.lineTotal.toFixed(2)} {item.currency}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <h2 style={{ marginBottom: 12 }}>Supplier orders</h2>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Id</th>
                  <th>Supplier</th>
                  <th>Status</th>
                  <th>Subtotal</th>
                  <th>Items</th>
                </tr>
              </thead>
              <tbody>
                {order.supplierOrders.map((supplierOrder) => (
                  <tr key={supplierOrder.id}>
                    <td>{supplierOrder.id.slice(0, 8)}</td>
                    <td>{supplierOrder.supplierId.slice(0, 8)}</td>
                    <td>{supplierOrder.status}</td>
                    <td>
                      {supplierOrder.subtotal.toFixed(2)} {supplierOrder.currency}
                    </td>
                    <td>{supplierOrder.items.length}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      ) : null}
    </AdminShell>
  );
}
