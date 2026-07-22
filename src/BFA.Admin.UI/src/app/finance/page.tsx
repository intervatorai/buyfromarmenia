"use client";

import { useCallback, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { ApiError, apiFetch } from "@/lib/api";

type FinanceSummary = {
  activeSuppliersCount: number;
  totalPendingSettlements: number;
  totalEligibleSettlements: number;
  totalPaidSettlements: number;
  currency: string;
};

type Settlement = {
  id: string;
  supplierId: string;
  supplierOrderId: string;
  grossAmount: number;
  platformFee: number;
  netAmount: number;
  currency: string;
  status: string;
  createdAtUtc: string;
};

type Payout = {
  id: string;
  supplierId: string;
  amount: number;
  currency: string;
  status: string;
  scheduledForUtc: string;
  completedAtUtc?: string | null;
};

export default function FinancePage() {
  const [data, setData] = useState<FinanceSummary | null>(null);
  const [settlements, setSettlements] = useState<Settlement[]>([]);
  const [payouts, setPayouts] = useState<Payout[]>([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  const load = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      const [summary, settlementList, payoutList] = await Promise.all([
        apiFetch<FinanceSummary>("/api/finance/summary"),
        apiFetch<Settlement[]>("/api/finance/settlements"),
        apiFetch<Payout[]>("/api/finance/payouts"),
      ]);
      setData(summary);
      setSettlements(settlementList);
      setPayouts(payoutList);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load finance.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const currency = data?.currency ?? "USD";

  function format(amount: number, code = currency) {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: code,
    }).format(amount);
  }

  async function markEligible(id: string) {
    await apiFetch(`/api/finance/settlements/${id}/eligible`, { method: "POST" });
    await load();
  }

  async function createPayout(id: string) {
    await apiFetch(`/api/finance/settlements/${id}/payout`, { method: "POST" });
    await load();
  }

  async function completePayout(id: string) {
    await apiFetch(`/api/finance/payouts/${id}/complete`, { method: "POST" });
    await load();
  }

  return (
    <AdminShell title="Finance">
      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading...</p> : null}

      {data ? (
        <div className="admin-grid" style={{ marginBottom: 24 }}>
          <div className="admin-card">
            <div className="admin-card-label">Active suppliers</div>
            <div className="admin-card-value">{data.activeSuppliersCount}</div>
          </div>
          <div className="admin-card">
            <div className="admin-card-label">Pending settlements</div>
            <div className="admin-card-value">{format(data.totalPendingSettlements)}</div>
          </div>
          <div className="admin-card">
            <div className="admin-card-label">Eligible for payout</div>
            <div className="admin-card-value">{format(data.totalEligibleSettlements)}</div>
          </div>
          <div className="admin-card">
            <div className="admin-card-label">Paid to suppliers</div>
            <div className="admin-card-value">{format(data.totalPaidSettlements)}</div>
          </div>
        </div>
      ) : null}

      <h2>Settlements</h2>
      <div className="admin-table-wrap" style={{ marginBottom: 24 }}>
        <table className="admin-table">
          <thead>
            <tr>
              <th>Supplier</th>
              <th>Order</th>
              <th>Net</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {settlements.length === 0 ? (
              <tr>
                <td colSpan={5}>No settlements</td>
              </tr>
            ) : (
              settlements.map((settlement) => (
                <tr key={settlement.id}>
                  <td>{settlement.supplierId.slice(0, 8)}</td>
                  <td>{settlement.supplierOrderId.slice(0, 8)}</td>
                  <td>{format(settlement.netAmount, settlement.currency)}</td>
                  <td>{settlement.status}</td>
                  <td style={{ display: "flex", gap: 8 }}>
                    {settlement.status === "Pending" ? (
                      <button
                        type="button"
                        className="button-ghost"
                        onClick={() => void markEligible(settlement.id)}
                      >
                        Mark eligible
                      </button>
                    ) : null}
                    {settlement.status === "Eligible" ? (
                      <button
                        type="button"
                        className="button-primary"
                        onClick={() => void createPayout(settlement.id)}
                      >
                        Create payout
                      </button>
                    ) : null}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <h2>Payouts</h2>
      <div className="admin-table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>Supplier</th>
              <th>Amount</th>
              <th>Status</th>
              <th>Scheduled</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {payouts.length === 0 ? (
              <tr>
                <td colSpan={5}>No payouts</td>
              </tr>
            ) : (
              payouts.map((payout) => (
                <tr key={payout.id}>
                  <td>{payout.supplierId.slice(0, 8)}</td>
                  <td>{format(payout.amount, payout.currency)}</td>
                  <td>{payout.status}</td>
                  <td>{new Date(payout.scheduledForUtc).toLocaleDateString("en-GB")}</td>
                  <td>
                    {payout.status === "Scheduled" ? (
                      <button
                        type="button"
                        className="button-primary"
                        onClick={() => void completePayout(payout.id)}
                      >
                        Mark completed
                      </button>
                    ) : (
                      "—"
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </AdminShell>
  );
}
