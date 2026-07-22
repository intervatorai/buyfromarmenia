"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useAuth } from "@/components/providers/AuthProvider";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { apiFetch, type PublicOrderSummary } from "@/lib/api";

function formatPrice(price: number, currency: string) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: currency || "USD",
  }).format(price);
}

export default function OrdersPage() {
  const { translate } = useLanguage();
  const { isLoading: isAuthLoading } = useAuth();
  const [orders, setOrders] = useState<PublicOrderSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (isAuthLoading) {
      return;
    }

    async function loadOrders() {
      try {
        const data = await apiFetch<PublicOrderSummary[]>("/api/orders");
        setOrders(data);
      } finally {
        setIsLoading(false);
      }
    }

    void loadOrders();
  }, [isAuthLoading]);

  return (
    <PublicSiteLayout>
      <section className="section container catalog-page">
        <div className="section-heading">
          <div>
            <p className="eyebrow">{translate("yourOrders")}</p>
            <h1>{translate("orderHistory")}</h1>
          </div>
        </div>

        {isLoading || isAuthLoading ? (
          <p className="catalog-message">{translate("loadingOrders")}</p>
        ) : null}
        {!isLoading && !isAuthLoading && orders.length === 0 ? (
          <div className="empty-cart">
            <h2>{translate("noOrdersYet")}</h2>
            <Link href="/products" className="button button-primary">
              {translate("continueShopping")}
            </Link>
          </div>
        ) : null}

        {!isLoading && !isAuthLoading && orders.length > 0 ? (
          <div className="orders-list">
            {orders.map((order) => (
              <Link key={order.id} href={`/orders/${order.id}`} className="order-card">
                <div>
                  <strong>{order.orderNumber}</strong>
                  <p>
                    {new Date(order.createdAtUtc).toLocaleString("en-GB")} ·{" "}
                    {order.itemsCount} {translate("items")}
                  </p>
                </div>
                <div className="order-card-meta">
                  <span className="order-status">{order.status}</span>
                  <strong>{formatPrice(order.subtotal, order.currency)}</strong>
                </div>
              </Link>
            ))}
          </div>
        ) : null}
      </section>
    </PublicSiteLayout>
  );
}
