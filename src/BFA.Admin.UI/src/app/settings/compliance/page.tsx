"use client";

import { FormEvent, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { Modal } from "@/components/ui/Modal";
import { ApiError, apiFetch } from "@/lib/api";

type TradeRestriction = {
  id: string;
  destinationCountryCode: string;
  categoryId?: string | null;
  reason: string;
  isActive: boolean;
  createdAtUtc: string;
};

export default function CompliancePage() {
  const [restrictions, setRestrictions] = useState<TradeRestriction[]>([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [countryCode, setCountryCode] = useState("");
  const [reason, setReason] = useState("");
  const [formError, setFormError] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  async function loadRestrictions() {
    try {
      const data = await apiFetch<TradeRestriction[]>("/api/compliance/restrictions");
      setRestrictions(data);
      setError("");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load trade restrictions.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadRestrictions();
  }, []);

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSaving(true);
    setFormError("");

    try {
      await apiFetch("/api/compliance/restrictions", {
        method: "POST",
        body: JSON.stringify({
          destinationCountryCode: countryCode,
          reason,
        }),
      });

      setModalOpen(false);
      setCountryCode("");
      setReason("");
      await loadRestrictions();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to create restriction.");
    } finally {
      setIsSaving(false);
    }
  }

  async function handleDeactivate(id: string) {
    setError("");

    try {
      await apiFetch(`/api/compliance/restrictions/${id}/deactivate`, {
        method: "POST",
      });
      await loadRestrictions();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to deactivate restriction.");
    }
  }

  return (
    <AdminShell title="Export compliance">
      <div style={{ marginBottom: 16, display: "flex", justifyContent: "flex-end" }}>
        <button
          type="button"
          className="button-primary"
          onClick={() => {
            setCountryCode("");
            setReason("");
            setFormError("");
            setModalOpen(true);
          }}
        >
          Add restriction
        </button>
      </div>

      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading restrictions...</p> : null}

      {!isLoading ? (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Country</th>
                <th>Category</th>
                <th>Reason</th>
                <th>Status</th>
                <th>Created</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {restrictions.length === 0 ? (
                <tr>
                  <td colSpan={6} style={{ textAlign: "center", color: "#94a3b8" }}>
                    No trade restrictions configured
                  </td>
                </tr>
              ) : (
                restrictions.map((restriction) => (
                  <tr key={restriction.id}>
                    <td>{restriction.destinationCountryCode}</td>
                    <td>{restriction.categoryId?.slice(0, 8) ?? "All products"}</td>
                    <td>{restriction.reason}</td>
                    <td>{restriction.isActive ? "Active" : "Inactive"}</td>
                    <td>{new Date(restriction.createdAtUtc).toLocaleString("en-GB")}</td>
                    <td>
                      {restriction.isActive ? (
                        <button
                          className="button-ghost"
                          type="button"
                          onClick={() => void handleDeactivate(restriction.id)}
                        >
                          Deactivate
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
      ) : null}

      <Modal
        open={modalOpen}
        title="Add country restriction"
        onClose={() => setModalOpen(false)}
        footer={
          <>
            <button
              type="button"
              className="button-ghost"
              onClick={() => setModalOpen(false)}
              disabled={isSaving}
            >
              Cancel
            </button>
            <button
              type="submit"
              form="compliance-form-modal"
              className="button-primary"
              disabled={isSaving}
            >
              {isSaving ? "Saving..." : "Add restriction"}
            </button>
          </>
        }
      >
        {formError ? <p className="form-error">{formError}</p> : null}
        <form id="compliance-form-modal" onSubmit={(event) => void handleCreate(event)}>
          <div className="form-field">
            <label htmlFor="countryCode">Country code (ISO-2)</label>
            <input
              id="countryCode"
              maxLength={2}
              required
              value={countryCode}
              onChange={(event) => setCountryCode(event.target.value.toUpperCase())}
            />
          </div>
          <div className="form-field">
            <label htmlFor="reason">Reason</label>
            <input
              id="reason"
              required
              value={reason}
              onChange={(event) => setReason(event.target.value)}
            />
          </div>
        </form>
      </Modal>
    </AdminShell>
  );
}
