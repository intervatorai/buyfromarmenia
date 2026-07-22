"use client";

import { useEffect, useState } from "react";
import { SupplierShell } from "@/components/layout/SupplierShell";
import { ApiError, apiFetch } from "@/lib/api";
import { getSupplierId } from "@/lib/supplier-session";

type FinanceData = {
  pendingBalance: number;
  eligibleBalance: number;
  paidTotal: number;
  currency: string;
  settlements: Array<{
    id: string;
    supplierOrderId: string;
    grossAmount: number;
    platformFee: number;
    netAmount: number;
    currency: string;
    status: string;
    createdAtUtc: string;
    eligibleAtUtc?: string | null;
  }>;
  payouts: Array<{
    id: string;
    amount: number;
    currency: string;
    status: string;
    scheduledForUtc: string;
    completedAtUtc?: string | null;
  }>;
};

function formatMoney(amount: number, currency: string) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
  }).format(amount);
}

export default function FinancePage() {
  const [data, setData] = useState<FinanceData | null>(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function loadFinance() {
      const supplierId = getSupplierId();
      if (!supplierId) {
        setError("Complete supplier onboarding first.");
        setIsLoading(false);
        return;
      }

      try {
        setData(await apiFetch<FinanceData>(`/api/finance?supplierId=${supplierId}`));
      } catch (err) {
        setError(err instanceof ApiError ? err.message : "Failed to load finance.");
      } finally {
        setIsLoading(false);
      }
    }

    void loadFinance();
  }, []);

  const currency = data?.currency ?? "USD";

  return (
    <SupplierShell title="Finance">
      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading finance...</p> : null}

      {data ? (
        <>
          <div className="supplier-grid">
            <div className="supplier-card">
              <div className="supplier-card-label">Pending balance</div>
              <div className="supplier-card-value">
                {formatMoney(data.pendingBalance, currency)}
              </div>
            </div>
            <div className="supplier-card">
              <div className="supplier-card-label">Eligible for payout</div>
              <div className="supplier-card-value">
                {formatMoney(data.eligibleBalance, currency)}
              </div>
            </div>
            <div className="supplier-card">
              <div className="supplier-card-label">Paid total</div>
              <div className="supplier-card-value">
                {formatMoney(data.paidTotal, currency)}
              </div>
            </div>
          </div>

          <div className="supplier-section">
            <h2>Settlements</h2>
            <div className="supplier-table-wrap">
              <table className="supplier-table">
                <thead>
                  <tr>
                    <th>Order</th>
                    <th>Gross</th>
                    <th>Fee</th>
                    <th>Net</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {data.settlements.length === 0 ? (
                    <tr>
                      <td colSpan={5} style={{ textAlign: "center", color: "#64748b" }}>
                        No settlements yet — created when orders reach warehouse
                      </td>
                    </tr>
                  ) : (
                    data.settlements.map((s) => (
                      <tr key={s.id}>
                        <td>{s.supplierOrderId.slice(0, 8)}</td>
                        <td>{formatMoney(s.grossAmount, s.currency)}</td>
                        <td>{formatMoney(s.platformFee, s.currency)}</td>
                        <td>{formatMoney(s.netAmount, s.currency)}</td>
                        <td>{s.status}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </>
      ) : null}
    </SupplierShell>
  );
}
