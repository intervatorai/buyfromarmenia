"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
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
};

export default function OrderDetailPage() {
  const params = useParams<{ id: string }>();
  const [order, setOrder] = useState<OrderDetail | null>(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function load() {
      try {
        setOrder(await apiFetch<OrderDetail>(`/api/orders/${params.id}`));
      } catch (err) {
        setError(err instanceof ApiError ? err.message : "Failed to load order.");
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [params.id]);

  return (
    <AdminShell title={order ? `Order ${order.orderNumber}` : "Order"}>
      <p style={{ marginBottom: 16 }}>
        <Link href="/orders" className="button-ghost">
          ← Back to orders
        </Link>
      </p>

      {error ? <p className="form-error">{error}</p> : null}
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
              <div>{order.status} · {order.paymentStatus} · {order.fulfillmentStatus}</div>
            </div>
            <div className="admin-card">
              <div className="admin-card-label">Total</div>
              <div className="admin-card-value" style={{ fontSize: 18 }}>
                {order.subtotal.toFixed(2)} {order.currency}
              </div>
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
