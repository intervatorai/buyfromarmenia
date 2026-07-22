"use client";

import { useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { ApiError, apiFetch } from "@/lib/api";

type DashboardData = {
  pendingReviewCount: number;
  publishedProductsCount: number;
  activeSellersCount: number;
  ordersTodayCount: number;
};

export default function DashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function loadDashboard() {
      setIsLoading(true);
      setError("");
      try {
        const response = await apiFetch<DashboardData>("/api/dashboard");
        setData(response);
      } catch (err) {
        setError(err instanceof ApiError ? err.message : "Failed to load dashboard.");
      } finally {
        setIsLoading(false);
      }
    }

    void loadDashboard();
  }, []);

  return (
    <AdminShell title="Dashboard">
      {error ? <p className="admin-error">{error}</p> : null}
      {isLoading ? <p>Loading dashboard...</p> : null}

      {data ? (
        <div className="admin-grid">
          <div className="admin-card">
            <div className="admin-card-label">Pending review</div>
            <div className="admin-card-value">{data.pendingReviewCount}</div>
          </div>
          <div className="admin-card">
            <div className="admin-card-label">Published products</div>
            <div className="admin-card-value">{data.publishedProductsCount}</div>
          </div>
          <div className="admin-card">
            <div className="admin-card-label">Active sellers</div>
            <div className="admin-card-value">{data.activeSellersCount}</div>
          </div>
          <div className="admin-card">
            <div className="admin-card-label">Orders today</div>
            <div className="admin-card-value">{data.ordersTodayCount}</div>
          </div>
        </div>
      ) : null}
    </AdminShell>
  );
}
