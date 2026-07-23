"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { ApiError, apiFetch } from "@/lib/api";

type CustomerDetail = {
  id: string;
  email: string;
  fullName: string;
  phone?: string | null;
  status: string;
  createdAtUtc: string;
  lastLoginAtUtc?: string | null;
  profileUpdatedAtUtc: string;
};

type CustomerOrder = {
  id: string;
  orderNumber: string;
  status: string;
  paymentStatus: string;
  fulfillmentStatus: string;
  subtotal: number;
  shippingFee: number;
  total: number;
  currency: string;
  paymentProvider?: string | null;
  paymentAmount?: number | null;
  paymentRecordStatus?: string | null;
  paymentCapturedAtUtc?: string | null;
  shippingAddress: {
    countryCode: string;
    city: string;
    line1: string;
    line2?: string | null;
    postalCode?: string | null;
    region?: string | null;
  };
  createdAtUtc: string;
};

type ImpersonateResponse = {
  accessToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  fullName: string;
  phone?: string | null;
};

const PUBLIC_SITE_URL =
  process.env.NEXT_PUBLIC_PUBLIC_SITE_URL ?? "http://localhost:3200";

function formatMoney(amount: number, currency: string) {
  return `${amount.toFixed(2)} ${currency}`;
}

function formatAddress(address: CustomerOrder["shippingAddress"]) {
  const parts = [
    address.line1,
    address.line2,
    [address.city, address.postalCode].filter(Boolean).join(" "),
    address.region,
    address.countryCode,
  ].filter(Boolean);
  return parts.join(", ");
}

export default function CustomerDetailPage() {
  const params = useParams<{ id: string }>();
  const [customer, setCustomer] = useState<CustomerDetail | null>(null);
  const [orders, setOrders] = useState<CustomerOrder[]>([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      const [customerData, ordersData] = await Promise.all([
        apiFetch<CustomerDetail>(`/api/customers/${params.id}`),
        apiFetch<CustomerOrder[]>(`/api/customers/${params.id}/orders`),
      ]);
      setCustomer(customerData);
      setOrders(ordersData);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load customer.");
      setCustomer(null);
      setOrders([]);
    } finally {
      setIsLoading(false);
    }
  }, [params.id]);

  useEffect(() => {
    void load();
  }, [load]);

  async function toggleActive() {
    if (!customer) {
      return;
    }

    setBusy(true);
    setError("");
    try {
      const action = customer.status === "Active" ? "suspend" : "activate";
      await apiFetch(`/api/customers/${customer.id}/${action}`, { method: "POST" });
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update status.");
    } finally {
      setBusy(false);
    }
  }

  async function openAsCustomer() {
    if (!customer) {
      return;
    }

    setBusy(true);
    setError("");
    try {
      const session = await apiFetch<ImpersonateResponse>(
        `/api/customers/${customer.id}/impersonate`,
        { method: "POST" },
      );
      const payload = encodeURIComponent(JSON.stringify(session));
      window.open(`${PUBLIC_SITE_URL}/account/impersonate#${payload}`, "_blank");
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : "Failed to open public site as this customer.",
      );
    } finally {
      setBusy(false);
    }
  }

  return (
    <AdminShell title={customer?.fullName || customer?.email || "Customer"}>
      <div style={{ marginBottom: 16 }}>
        <Link href="/customers" className="button-ghost">
          ← Customers
        </Link>
      </div>

      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading customer...</p> : null}

      {!isLoading && customer ? (
        <>
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              gap: 16,
              flexWrap: "wrap",
              marginBottom: 24,
            }}
          >
            <div>
              <p style={{ margin: "0 0 4px" }}>
                <strong>{customer.fullName}</strong> · {customer.email}
              </p>
              <p style={{ margin: 0, opacity: 0.8 }}>
                {customer.phone || "No phone"} · {customer.status} · Registered{" "}
                {new Date(customer.createdAtUtc).toLocaleString("en-GB")}
              </p>
              <p style={{ margin: "4px 0 0", opacity: 0.8 }}>
                Last login:{" "}
                {customer.lastLoginAtUtc
                  ? new Date(customer.lastLoginAtUtc).toLocaleString("en-GB")
                  : "—"}
              </p>
            </div>
            <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
              <button
                type="button"
                className="button-primary"
                disabled={busy || customer.status !== "Active"}
                onClick={() => void openAsCustomer()}
              >
                Open as customer
              </button>
              <button
                type="button"
                className="button-ghost"
                disabled={busy}
                onClick={() => void toggleActive()}
              >
                {customer.status === "Active" ? "Suspend" : "Activate"}
              </button>
            </div>
          </div>

          <h2 style={{ margin: "0 0 12px", fontSize: "1.1rem" }}>Orders</h2>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Order</th>
                  <th>Status</th>
                  <th>Payment</th>
                  <th>Paid</th>
                  <th>Ship to</th>
                  <th>Total</th>
                  <th>Created</th>
                </tr>
              </thead>
              <tbody>
                {orders.length === 0 ? (
                  <tr>
                    <td colSpan={7}>No orders for this customer.</td>
                  </tr>
                ) : (
                  orders.map((order) => (
                    <tr key={order.id}>
                      <td>
                        <Link href={`/orders/${order.id}`}>{order.orderNumber}</Link>
                      </td>
                      <td>
                        {order.status}
                        <div style={{ opacity: 0.7, fontSize: "0.85em" }}>
                          {order.fulfillmentStatus}
                        </div>
                      </td>
                      <td>
                        {order.paymentProvider || "—"}
                        <div style={{ opacity: 0.7, fontSize: "0.85em" }}>
                          {order.paymentRecordStatus || order.paymentStatus}
                        </div>
                      </td>
                      <td>
                        {order.paymentAmount != null
                          ? formatMoney(order.paymentAmount, order.currency)
                          : formatMoney(order.total, order.currency)}
                        {order.paymentCapturedAtUtc ? (
                          <div style={{ opacity: 0.7, fontSize: "0.85em" }}>
                            {new Date(order.paymentCapturedAtUtc).toLocaleString("en-GB")}
                          </div>
                        ) : null}
                      </td>
                      <td style={{ maxWidth: 260 }}>{formatAddress(order.shippingAddress)}</td>
                      <td>{formatMoney(order.total, order.currency)}</td>
                      <td>{new Date(order.createdAtUtc).toLocaleString("en-GB")}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </>
      ) : null}
    </AdminShell>
  );
}
