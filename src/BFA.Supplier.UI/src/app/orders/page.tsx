"use client";

import { useCallback, useEffect, useState } from "react";
import { SupplierShell } from "@/components/layout/SupplierShell";
import { ApiError, apiFetch } from "@/lib/api";
import { getSupplierId } from "@/lib/supplier-session";

type SupplierOrder = {
  id: string;
  customerOrderId: string;
  status: string;
  subtotal: number;
  currency: string;
  itemsCount: number;
  createdAtUtc: string;
};

const NEXT_STATUSES: Record<string, string> = {
  New: "Confirmed",
  Confirmed: "Preparing",
  Preparing: "ReadyForPickup",
};

const ACTION_LABELS: Record<string, string> = {
  New: "Mark Confirmed",
  Confirmed: "Mark Preparing",
  Preparing: "Mark ReadyForPickup",
};

export default function OrdersPage() {
  const [orders, setOrders] = useState<SupplierOrder[]>([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [updatingId, setUpdatingId] = useState<string | null>(null);

  const loadOrders = useCallback(async () => {
    const supplierId = getSupplierId();
    if (!supplierId) {
      setError("Complete supplier onboarding first.");
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError("");
    try {
      const data = await apiFetch<SupplierOrder[]>(
        `/api/orders?supplierId=${supplierId}`,
      );
      setOrders(data);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load orders.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadOrders();
  }, [loadOrders]);

  async function advanceStatus(order: SupplierOrder) {
    const nextStatus = NEXT_STATUSES[order.status];
    if (!nextStatus) return;

    const supplierId = getSupplierId();
    if (!supplierId) return;

    setUpdatingId(order.id);
    try {
      await apiFetch(`/api/orders/${order.id}/status`, {
        method: "POST",
        body: JSON.stringify({ supplierId, status: nextStatus }),
      });
      await loadOrders();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update order.");
    } finally {
      setUpdatingId(null);
    }
  }

  async function transferToWarehouse(order: SupplierOrder) {
    const supplierId = getSupplierId();
    if (!supplierId) return;

    setUpdatingId(order.id);
    try {
      await apiFetch(`/api/orders/${order.id}/transfer`, {
        method: "POST",
        body: JSON.stringify({ supplierId }),
      });
      await loadOrders();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to transfer order.");
    } finally {
      setUpdatingId(null);
    }
  }

  return (
    <SupplierShell title="Orders">
      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading orders...</p> : null}

      <div className="supplier-table-wrap">
        <table className="supplier-table">
          <thead>
            <tr>
              <th>Order #</th>
              <th>Items</th>
              <th>Total</th>
              <th>Created</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {orders.length === 0 && !isLoading ? (
              <tr>
                <td colSpan={6} style={{ textAlign: "center", color: "#64748b" }}>
                  No supplier orders yet.
                </td>
              </tr>
            ) : null}
            {orders.map((order) => (
              <tr key={order.id}>
                <td>{order.id.slice(0, 8)}…</td>
                <td>{order.itemsCount}</td>
                <td>
                  {order.subtotal.toFixed(2)} {order.currency}
                </td>
                <td>{new Date(order.createdAtUtc).toLocaleDateString("en-GB")}</td>
                <td>
                  <span className={`status-badge ${order.status.toLowerCase()}`}>
                    {order.status}
                  </span>
                </td>
                <td>
                  {NEXT_STATUSES[order.status] ? (
                    <button
                      className="button-secondary"
                      type="button"
                      disabled={updatingId === order.id}
                      onClick={() => void advanceStatus(order)}
                    >
                      {updatingId === order.id
                        ? "Updating..."
                        : ACTION_LABELS[order.status]}
                    </button>
                  ) : order.status === "ReadyForPickup" ? (
                    <button
                      className="button-primary"
                      type="button"
                      disabled={updatingId === order.id}
                      onClick={() => void transferToWarehouse(order)}
                    >
                      {updatingId === order.id
                        ? "Transferring..."
                        : "Transfer to warehouse"}
                    </button>
                  ) : (
                    "—"
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </SupplierShell>
  );
}
