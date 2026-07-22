"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { ApiError, apiFetch } from "@/lib/api";

type AdminOrder = {
  id: string;
  orderNumber: string;
  customerEmail: string;
  customerFullName: string;
  status: string;
  paymentStatus: string;
  fulfillmentStatus: string;
  subtotal: number;
  currency: string;
  itemsCount: number;
  supplierOrdersCount: number;
  createdAtUtc: string;
};

export default function OrdersPage() {
  const [orders, setOrders] = useState<AdminOrder[]>([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function loadOrders() {
      try {
        const data = await apiFetch<AdminOrder[]>("/api/orders");
        setOrders(data);
      } catch (err) {
        setError(err instanceof ApiError ? err.message : "Failed to load orders.");
      } finally {
        setIsLoading(false);
      }
    }

    void loadOrders();
  }, []);

  return (
    <AdminShell title="Orders">
      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading orders...</p> : null}

      {!isLoading && orders.length === 0 ? (
        <div className="admin-card">No customer orders yet.</div>
      ) : null}

      {!isLoading && orders.length > 0 ? (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Order</th>
                <th>Customer</th>
                <th>Total</th>
                <th>Status</th>
                <th>Supplier orders</th>
                <th>Created</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {orders.map((order) => (
                <tr key={order.id}>
                  <td>
                    <Link href={`/orders/${order.id}`}>
                      <strong>{order.orderNumber}</strong>
                    </Link>
                    <div style={{ color: "var(--admin-muted)", fontSize: 12 }}>
                      {order.itemsCount} items
                    </div>
                  </td>
                  <td>
                    <strong>{order.customerFullName}</strong>
                    <div style={{ color: "var(--admin-muted)", fontSize: 12 }}>
                      {order.customerEmail}
                    </div>
                  </td>
                  <td>
                    {order.subtotal.toFixed(2)} {order.currency}
                  </td>
                  <td>
                    <span className={`status-badge ${order.status.toLowerCase()}`}>
                      {order.status}
                    </span>
                  </td>
                  <td>{order.supplierOrdersCount}</td>
                  <td>
                    {new Date(order.createdAtUtc).toLocaleDateString("en-GB")}
                  </td>
                  <td>
                    <Link href={`/orders/${order.id}`} className="button-ghost">
                      Open
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}
    </AdminShell>
  );
}
