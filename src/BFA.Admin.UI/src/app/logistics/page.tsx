"use client";

import { useCallback, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { Modal } from "@/components/ui/Modal";
import { ApiError, apiFetch } from "@/lib/api";

type Shipment = {
  id: string;
  referenceNumber: string;
  customerOrderId: string;
  consolidationId: string;
  carrier: string;
  trackingNumber: string;
  status: string;
  declaredValue: number;
  currency: string;
  createdAtUtc: string;
};

type Consolidation = {
  id: string;
  referenceNumber: string;
  customerOrderId: string;
  status: string;
};

const NEXT_STATUS: Record<string, string> = {
  Created: "PickedUp",
  PickedUp: "InTransit",
  InTransit: "OutForDelivery",
  OutForDelivery: "Delivered",
};

const SHIPMENT_STATUSES = [
  "Created",
  "PickedUp",
  "InTransit",
  "OutForDelivery",
  "Delivered",
] as const;

export default function LogisticsPage() {
  const [shipments, setShipments] = useState<Shipment[]>([]);
  const [sealedConsolidations, setSealedConsolidations] = useState<Consolidation[]>([]);
  const [selectedConsolidationId, setSelectedConsolidationId] = useState("");
  const [carrier, setCarrier] = useState("Stub");
  const [customsDescription, setCustomsDescription] = useState("Armenian goods export");
  const [error, setError] = useState("");
  const [formError, setFormError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [actionId, setActionId] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);

  const loadData = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      const [shipmentsData, consolidationsData] = await Promise.all([
        apiFetch<Shipment[]>("/api/logistics/shipments"),
        apiFetch<Consolidation[]>("/api/warehouse/consolidations?status=Sealed"),
      ]);
      setShipments(shipmentsData);
      const shippedIds = new Set(shipmentsData.map((s) => s.consolidationId));
      setSealedConsolidations(
        consolidationsData.filter((c) => !shippedIds.has(c.id)),
      );
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load logistics data.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  async function createShipment() {
    if (!selectedConsolidationId) {
      setFormError("Select a sealed consolidation.");
      return;
    }

    setActionId("create");
    setFormError("");
    try {
      await apiFetch("/api/logistics/shipments", {
        method: "POST",
        body: JSON.stringify({
          consolidationId: selectedConsolidationId,
          carrier,
          customsDescription,
        }),
      });
      setSelectedConsolidationId("");
      setModalOpen(false);
      await loadData();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to create shipment.");
    } finally {
      setActionId(null);
    }
  }

  async function advanceShipment(id: string) {
    setActionId(id);
    setError("");
    try {
      await apiFetch(`/api/logistics/shipments/${id}/advance`, { method: "POST" });
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update shipment.");
    } finally {
      setActionId(null);
    }
  }

  async function setShipmentStatus(id: string, status: string, current: string) {
    if (status === current) return;
    setActionId(id);
    setError("");
    try {
      await apiFetch(`/api/logistics/shipments/${id}/status`, {
        method: "PUT",
        body: JSON.stringify({ status }),
      });
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update shipment.");
    } finally {
      setActionId(null);
    }
  }

  return (
    <AdminShell title="Logistics">
      <div style={{ marginBottom: 16, display: "flex", justifyContent: "flex-end" }}>
        <button
          type="button"
          className="button-primary"
          onClick={() => {
            setFormError("");
            setSelectedConsolidationId("");
            setCarrier("Stub");
            setCustomsDescription("Armenian goods export");
            setModalOpen(true);
          }}
        >
          Create shipment
        </button>
      </div>

      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading...</p> : null}

      {!isLoading ? (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Reference</th>
                <th>Tracking</th>
                <th>Carrier</th>
                <th>Status</th>
                <th>Declared</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {shipments.length === 0 ? (
                <tr>
                  <td colSpan={6} style={{ textAlign: "center", color: "var(--admin-muted)" }}>
                    No shipments yet
                  </td>
                </tr>
              ) : (
                shipments.map((shipment) => (
                  <tr key={shipment.id}>
                    <td>
                      <strong>{shipment.referenceNumber}</strong>
                      <div style={{ color: "var(--admin-muted)", fontSize: 12 }}>
                        Order {shipment.customerOrderId.slice(0, 8)}
                      </div>
                    </td>
                    <td>{shipment.trackingNumber}</td>
                    <td>{shipment.carrier}</td>
                    <td>
                      <span className={`status-badge ${shipment.status.toLowerCase()}`}>
                        {shipment.status}
                      </span>
                    </td>
                    <td>
                      {shipment.declaredValue.toFixed(2)} {shipment.currency}
                    </td>
                    <td>
                      <div style={{ display: "flex", gap: 8, alignItems: "center", flexWrap: "wrap" }}>
                        <select
                          className="form-control"
                          style={{ minHeight: 36, width: "auto", minWidth: 150 }}
                          value={shipment.status}
                          disabled={actionId === shipment.id || shipment.status === "Delivered"}
                          onChange={(event) =>
                            void setShipmentStatus(
                              shipment.id,
                              event.target.value,
                              shipment.status,
                            )
                          }
                        >
                          {SHIPMENT_STATUSES.map((status) => {
                            const currentIndex = SHIPMENT_STATUSES.indexOf(
                              shipment.status as (typeof SHIPMENT_STATUSES)[number],
                            );
                            const optionIndex = SHIPMENT_STATUSES.indexOf(status);
                            return (
                              <option
                                key={status}
                                value={status}
                                disabled={optionIndex < currentIndex}
                              >
                                {status}
                              </option>
                            );
                          })}
                        </select>
                        {NEXT_STATUS[shipment.status] ? (
                          <button
                            type="button"
                            className="button-secondary"
                            disabled={actionId === shipment.id}
                            onClick={() => void advanceShipment(shipment.id)}
                          >
                            → {NEXT_STATUS[shipment.status]}
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

      <Modal
        open={modalOpen}
        title="Create international shipment"
        onClose={() => setModalOpen(false)}
        footer={
          <>
            <button
              type="button"
              className="button-ghost"
              onClick={() => setModalOpen(false)}
              disabled={actionId === "create"}
            >
              Cancel
            </button>
            <button
              type="button"
              className="button-primary"
              disabled={actionId === "create"}
              onClick={() => void createShipment()}
            >
              {actionId === "create" ? "Creating..." : "Create shipment"}
            </button>
          </>
        }
      >
        {formError ? <p className="form-error">{formError}</p> : null}
        <div className="form-field">
          <label htmlFor="consolidation">Sealed consolidation</label>
          <select
            id="consolidation"
            className="form-control"
            value={selectedConsolidationId}
            onChange={(e) => setSelectedConsolidationId(e.target.value)}
          >
            <option value="">Select consolidation</option>
            {sealedConsolidations.map((c) => (
              <option key={c.id} value={c.id}>
                {c.referenceNumber} — order {c.customerOrderId.slice(0, 8)}
              </option>
            ))}
          </select>
        </div>
        <div className="form-field">
          <label htmlFor="carrier">Carrier</label>
          <select
            id="carrier"
            className="form-control"
            value={carrier}
            onChange={(e) => setCarrier(e.target.value)}
          >
            <option value="Stub">Stub</option>
            <option value="Dhl">DHL</option>
            <option value="FedEx">FedEx</option>
          </select>
        </div>
        <div className="form-field">
          <label htmlFor="customs">Customs description</label>
          <input
            id="customs"
            value={customsDescription}
            onChange={(e) => setCustomsDescription(e.target.value)}
          />
        </div>
      </Modal>
    </AdminShell>
  );
}
