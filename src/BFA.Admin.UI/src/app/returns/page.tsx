"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { PromptModal } from "@/components/ui/PromptModal";
import { ApiError, apiFetch } from "@/lib/api";

type ReturnRequestItem = {
  id: string;
  customerOrderId: string;
  customerEmail: string;
  reason: string;
  status: string;
  createdAtUtc: string;
  resolvedAtUtc?: string | null;
};

type PromptState =
  | { kind: "approve"; id: string }
  | { kind: "reject"; id: string }
  | { kind: "receive"; id: string }
  | null;

export default function ReturnsPage() {
  const [returns, setReturns] = useState<ReturnRequestItem[]>([]);
  const [filter, setFilter] = useState("Requested");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [prompt, setPrompt] = useState<PromptState>(null);

  const loadReturns = useCallback(async () => {
    setIsLoading(true);
    setError("");

    try {
      const query = filter ? `?status=${encodeURIComponent(filter)}` : "";
      setReturns(await apiFetch<ReturnRequestItem[]>(`/api/returns${query}`));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load returns.");
    } finally {
      setIsLoading(false);
    }
  }, [filter]);

  useEffect(() => {
    void loadReturns();
  }, [loadReturns]);

  async function handleRefund(id: string) {
    try {
      await apiFetch(`/api/returns/${id}/refund`, { method: "POST" });
      await loadReturns();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to refund.");
    }
  }

  return (
    <AdminShell title="Returns">
      <div className="admin-filters" style={{ marginBottom: 16 }}>
        {["Requested", "Approved", "Received", "Rejected", "Refunded", ""].map((status) => (
          <button
            key={status || "all"}
            type="button"
            className={`button-ghost${filter === status ? " active" : ""}`}
            onClick={() => setFilter(status)}
          >
            {status || "All"}
          </button>
        ))}
      </div>

      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading returns...</p> : null}

      {!isLoading ? (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Order</th>
                <th>Customer</th>
                <th>Reason</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {returns.length === 0 ? (
                <tr>
                  <td colSpan={5} style={{ textAlign: "center", color: "#94a3b8" }}>
                    No return requests
                  </td>
                </tr>
              ) : (
                returns.map((item) => (
                  <tr key={item.id}>
                    <td>
                      <Link href={`/orders/${item.customerOrderId}`}>
                        {item.customerOrderId.slice(0, 8)}
                      </Link>
                    </td>
                    <td>{item.customerEmail}</td>
                    <td style={{ maxWidth: 280 }}>{item.reason}</td>
                    <td>{item.status}</td>
                    <td>
                      <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                        {item.status === "Requested" ? (
                          <>
                            <button
                              type="button"
                              className="button-primary"
                              onClick={() => setPrompt({ kind: "approve", id: item.id })}
                            >
                              Approve
                            </button>
                            <button
                              type="button"
                              className="button-ghost"
                              onClick={() => setPrompt({ kind: "reject", id: item.id })}
                            >
                              Reject
                            </button>
                          </>
                        ) : null}
                        {item.status === "Approved" ? (
                          <>
                            <button
                              type="button"
                              className="button-ghost"
                              onClick={() => setPrompt({ kind: "receive", id: item.id })}
                            >
                              Mark received
                            </button>
                            <button
                              type="button"
                              className="button-secondary"
                              onClick={() => void handleRefund(item.id)}
                            >
                              Mark refunded
                            </button>
                          </>
                        ) : null}
                        {item.status === "Received" ? (
                          <button
                            type="button"
                            className="button-secondary"
                            onClick={() => void handleRefund(item.id)}
                          >
                            Mark refunded
                          </button>
                        ) : null}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      ) : null}

      <PromptModal
        open={prompt !== null}
        title={
          prompt?.kind === "approve"
            ? "Approve return"
            : prompt?.kind === "reject"
              ? "Reject return"
              : "Mark received"
        }
        label="Notes"
        initialValue={prompt?.kind === "approve" ? "Approved by admin" : ""}
        required={prompt?.kind === "reject"}
        confirmLabel={
          prompt?.kind === "approve"
            ? "Approve"
            : prompt?.kind === "reject"
              ? "Reject"
              : "Mark received"
        }
        onClose={() => setPrompt(null)}
        onConfirm={async (notes) => {
          if (!prompt) {
            return;
          }
          const path =
            prompt.kind === "approve"
              ? `/api/returns/${prompt.id}/approve`
              : prompt.kind === "reject"
                ? `/api/returns/${prompt.id}/reject`
                : `/api/returns/${prompt.id}/receive`;
          await apiFetch(path, {
            method: "POST",
            body: JSON.stringify({ notes }),
          });
          await loadReturns();
        }}
      />
    </AdminShell>
  );
}
