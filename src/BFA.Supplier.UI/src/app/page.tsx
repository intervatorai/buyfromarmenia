"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { SupplierShell } from "@/components/layout/SupplierShell";
import { ApiError, apiFetch } from "@/lib/api";
import { getSupplierId } from "@/lib/supplier-session";

type DashboardData = {
  salesToday: number;
  salesThisMonth: number;
  ordersCount: number;
  lowStockItemsCount: number;
  pendingModerationCount: number;
  awaitingShipmentCount: number;
  returnsCount: number;
  estimatedBalance: number;
  currency: string;
  recentOrders: Array<{
    id: string;
    customerOrderId: string;
    status: string;
    shipmentStatus: string | null;
    subtotal: number;
    currency: string;
    itemsCount: number;
    createdAtUtc: string;
  }>;
};

function formatShipmentStatus(status: string | null | undefined) {
  if (!status) return "—";
  return status.replace(/([a-z])([A-Z])/g, "$1 $2");
}

function formatMoney(amount: number, currency: string) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(amount);
}

export default function DashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  const loadDashboard = useCallback(async () => {
    const supplierId = getSupplierId();
    if (!supplierId) {
      setError("Complete supplier onboarding first.");
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError("");
    try {
      const response = await apiFetch<DashboardData>(
        `/api/dashboard?supplierId=${supplierId}`,
      );
      setData(response);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load dashboard.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadDashboard();
  }, [loadDashboard]);

  const currency = data?.currency ?? "USD";
  const metrics = data
    ? [
        {
          label: "Sales today",
          value: formatMoney(data.salesToday, currency),
          warning: false,
        },
        {
          label: "Sales this month",
          value: formatMoney(data.salesThisMonth, currency),
          warning: false,
        },
        { label: "Orders", value: String(data.ordersCount), warning: false },
        {
          label: "Low stock items",
          value: String(data.lowStockItemsCount),
          warning: data.lowStockItemsCount > 0,
        },
        {
          label: "Pending moderation",
          value: String(data.pendingModerationCount),
          warning: data.pendingModerationCount > 0,
        },
        {
          label: "Awaiting shipment",
          value: String(data.awaitingShipmentCount),
          warning: data.awaitingShipmentCount > 0,
        },
        { label: "Returns", value: String(data.returnsCount), warning: false },
        {
          label: "Balance",
          value: formatMoney(data.estimatedBalance, currency),
          warning: false,
        },
      ]
    : [];

  return (
    <SupplierShell
      title="Dashboard"
      action={
        <Link href="/products/new" className="button-primary">
          Add product
        </Link>
      }
    >
      {error ? <p className="supplier-error">{error}</p> : null}
      {isLoading ? <p>Loading dashboard...</p> : null}

      {!isLoading && data ? (
        <>
          <div className="supplier-grid">
            {metrics.map((metric) => (
              <div key={metric.label} className="supplier-card">
                <div className="supplier-card-label">{metric.label}</div>
                <div className="supplier-card-value">{metric.value}</div>
                {metric.warning ? (
                  <div className="supplier-card-delta warning">Needs attention</div>
                ) : null}
              </div>
            ))}
          </div>

          <div className="supplier-grid-2 supplier-section">
            <div>
              <h2>Recent orders</h2>
              <div className="supplier-table-wrap">
                <table className="supplier-table">
                  <thead>
                    <tr>
                      <th>Order</th>
                      <th>Items</th>
                      <th>Total</th>
                      <th>Fulfillment</th>
                      <th>Shipping</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.recentOrders.length === 0 ? (
                      <tr>
                        <td colSpan={5} style={{ textAlign: "center", color: "#64748b" }}>
                          No orders yet
                        </td>
                      </tr>
                    ) : (
                      data.recentOrders.map((order) => (
                        <tr key={order.id}>
                          <td>{order.id.slice(0, 8)}</td>
                          <td>{order.itemsCount}</td>
                          <td>{formatMoney(order.subtotal, order.currency)}</td>
                          <td>{order.status}</td>
                          <td>{formatShipmentStatus(order.shipmentStatus)}</td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </div>

            <div>
              <h2>Next payout</h2>
              <div className="supplier-card">
                <div className="supplier-card-label">Estimated payout</div>
                <div className="supplier-card-value">
                  {formatMoney(data.estimatedBalance, currency)}
                </div>
                <p style={{ margin: "12px 0 0", fontSize: 13, color: "#64748b" }}>
                  Payouts are calculated after delivery and return window.
                </p>
                <Link
                  href="/finance"
                  className="button-secondary"
                  style={{ display: "inline-block", marginTop: 16 }}
                >
                  View finance
                </Link>
              </div>
            </div>
          </div>
        </>
      ) : null}
    </SupplierShell>
  );
}
