"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { AdminShell } from "@/components/layout/AdminShell";
import { Modal } from "@/components/ui/Modal";
import { ApiError, apiFetch } from "@/lib/api";

type InboundShipment = {
  id: string;
  referenceNumber: string;
  supplierOrderId: string;
  customerOrderId: string;
  supplierId: string;
  supplierName?: string | null;
  status: string;
  itemsCount: number;
  scanReference?: string | null;
  weightKg?: number | null;
  createdAtUtc: string;
  receivedAtUtc?: string | null;
};

const STATUS_FILTERS = ["", "Pending", "Arrived", "Received"] as const;

export default function WarehousePage() {
  const [shipments, setShipments] = useState<InboundShipment[]>([]);
  const [statusFilter, setStatusFilter] = useState("");
  const [error, setError] = useState("");
  const [formError, setFormError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [activeReceiveId, setActiveReceiveId] = useState<string | null>(null);
  const [scanReference, setScanReference] = useState("");
  const [weightKg, setWeightKg] = useState("");
  const [inspectionNotes, setInspectionNotes] = useState("");
  const [photoUrl, setPhotoUrl] = useState("");
  const [actionId, setActionId] = useState<string | null>(null);

  const loadShipments = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      const query = statusFilter ? `?status=${statusFilter}` : "";
      const data = await apiFetch<InboundShipment[]>(
        `/api/warehouse/inbound${query}`,
      );
      setShipments(data);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load inbound shipments.");
    } finally {
      setIsLoading(false);
    }
  }, [statusFilter]);

  useEffect(() => {
    void loadShipments();
  }, [loadShipments]);

  async function markArrived(id: string) {
    setActionId(id);
    setError("");
    try {
      await apiFetch(`/api/warehouse/inbound/${id}/arrived`, { method: "POST" });
      await loadShipments();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to mark as arrived.");
    } finally {
      setActionId(null);
    }
  }

  async function receiveShipment(id: string) {
    const weight = Number(weightKg);
    if (!scanReference.trim() || !weight || weight <= 0) {
      setFormError("Scan reference and positive weight are required.");
      return;
    }

    setActionId(id);
    setFormError("");
    try {
      await apiFetch(`/api/warehouse/inbound/${id}/receive`, {
        method: "POST",
        body: JSON.stringify({
          scanReference: scanReference.trim(),
          weightKg: weight,
          inspectionNotes: inspectionNotes.trim() || null,
          photoUrl: photoUrl.trim() || null,
        }),
      });
      setActiveReceiveId(null);
      setScanReference("");
      setWeightKg("");
      setInspectionNotes("");
      setPhotoUrl("");
      await loadShipments();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to receive shipment.");
    } finally {
      setActionId(null);
    }
  }

  function openReceive(id: string) {
    setActiveReceiveId(id);
    setScanReference("");
    setWeightKg("");
    setInspectionNotes("");
    setPhotoUrl("");
    setFormError("");
  }

  return (
    <AdminShell title="Warehouse">
      <p style={{ marginBottom: 16 }}>
        <Link href="/warehouse/consolidations" className="button-secondary">
          Consolidations
        </Link>
      </p>
      <div style={{ marginBottom: 16 }}>
        <label style={{ fontSize: 13, color: "var(--admin-muted)" }}>
          Status{" "}
          <select
            className="form-control"
            value={statusFilter}
            onChange={(event) => setStatusFilter(event.target.value)}
            style={{ marginLeft: 8, width: "auto", display: "inline-block" }}
          >
            {STATUS_FILTERS.map((status) => (
              <option key={status || "all"} value={status}>
                {status || "All"}
              </option>
            ))}
          </select>
        </label>
      </div>

      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading inbound shipments...</p> : null}

      {!isLoading && shipments.length === 0 ? (
        <div className="admin-card">No inbound shipments yet.</div>
      ) : null}

      {!isLoading && shipments.length > 0 ? (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Reference</th>
                <th>Supplier</th>
                <th>Items</th>
                <th>Status</th>
                <th>Receipt</th>
                <th>Created</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {shipments.map((shipment) => (
                <tr key={shipment.id}>
                  <td>
                    <strong>{shipment.referenceNumber}</strong>
                    <div style={{ color: "var(--admin-muted)", fontSize: 12 }}>
                      SO {shipment.supplierOrderId.slice(0, 8)}
                    </div>
                  </td>
                  <td>{shipment.supplierName ?? shipment.supplierId.slice(0, 8)}</td>
                  <td>{shipment.itemsCount}</td>
                  <td>
                    <span className={`status-badge ${shipment.status.toLowerCase()}`}>
                      {shipment.status}
                    </span>
                  </td>
                  <td>
                    {shipment.scanReference ? (
                      <>
                        <div>{shipment.scanReference}</div>
                        <div style={{ color: "var(--admin-muted)", fontSize: 12 }}>
                          {shipment.weightKg} kg
                        </div>
                      </>
                    ) : (
                      "—"
                    )}
                  </td>
                  <td>
                    {new Date(shipment.createdAtUtc).toLocaleDateString("en-GB")}
                  </td>
                  <td>
                    {shipment.status === "Pending" ? (
                      <button
                        type="button"
                        className="button-secondary"
                        disabled={actionId === shipment.id}
                        onClick={() => void markArrived(shipment.id)}
                      >
                        Mark arrived
                      </button>
                    ) : null}
                    {shipment.status === "Pending" || shipment.status === "Arrived" ? (
                      <button
                        type="button"
                        className="button-primary"
                        style={{ marginLeft: 8 }}
                        disabled={actionId === shipment.id}
                        onClick={() => openReceive(shipment.id)}
                      >
                        Receive
                      </button>
                    ) : null}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}

      <Modal
        open={activeReceiveId !== null}
        title="Receive shipment"
        onClose={() => setActiveReceiveId(null)}
        footer={
          <>
            <button type="button" className="button-ghost" onClick={() => setActiveReceiveId(null)}>
              Cancel
            </button>
            <button
              type="button"
              className="button-primary"
              disabled={actionId === activeReceiveId}
              onClick={() => {
                if (activeReceiveId) {
                  void receiveShipment(activeReceiveId);
                }
              }}
            >
              Confirm receipt
            </button>
          </>
        }
      >
        {formError ? <p className="form-error">{formError}</p> : null}
        <div className="form-field">
          <label htmlFor="scanReference">Scan reference</label>
          <input
            id="scanReference"
            value={scanReference}
            onChange={(event) => setScanReference(event.target.value)}
            placeholder="Barcode / SKU scan"
          />
        </div>
        <div className="form-field">
          <label htmlFor="weightKg">Weight (kg)</label>
          <input
            id="weightKg"
            type="number"
            min="0.001"
            step="0.001"
            value={weightKg}
            onChange={(event) => setWeightKg(event.target.value)}
          />
        </div>
        <div className="form-field">
          <label htmlFor="photoUrl">Photo URL</label>
          <input
            id="photoUrl"
            value={photoUrl}
            onChange={(event) => setPhotoUrl(event.target.value)}
            placeholder="https://..."
          />
        </div>
        <div className="form-field">
          <label htmlFor="inspectionNotes">Inspection notes</label>
          <textarea
            id="inspectionNotes"
            className="form-control"
            value={inspectionNotes}
            onChange={(event) => setInspectionNotes(event.target.value)}
            rows={3}
          />
        </div>
      </Modal>
    </AdminShell>
  );
}
