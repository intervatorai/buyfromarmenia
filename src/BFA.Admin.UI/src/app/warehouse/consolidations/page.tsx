"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { AdminShell } from "@/components/layout/AdminShell";
import { Modal } from "@/components/ui/Modal";
import { ApiError, apiFetch } from "@/lib/api";

type Consolidation = {
  id: string;
  referenceNumber: string;
  customerOrderId: string;
  status: string;
  inboundShipmentsCount: number;
  packagesCount: number;
  totalWeightKg: number;
  createdAtUtc: string;
};

type EligibleShipment = {
  id: string;
  referenceNumber: string;
  customerOrderId: string;
  supplierOrderId: string;
  itemsCount: number;
  weightKg?: number | null;
};

export default function ConsolidationsPage() {
  const [consolidations, setConsolidations] = useState<Consolidation[]>([]);
  const [eligible, setEligible] = useState<EligibleShipment[]>([]);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [customerOrderId, setCustomerOrderId] = useState("");
  const [packageWeight, setPackageWeight] = useState("");
  const [activeConsolidationId, setActiveConsolidationId] = useState<string | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [error, setError] = useState("");
  const [formError, setFormError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [actionId, setActionId] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      const [consolidationsData, eligibleData] = await Promise.all([
        apiFetch<Consolidation[]>("/api/warehouse/consolidations"),
        apiFetch<EligibleShipment[]>("/api/warehouse/inbound/eligible"),
      ]);
      setConsolidations(consolidationsData);
      setEligible(eligibleData);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load consolidations.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  function toggleShipment(id: string, orderId: string) {
    setSelectedIds((current) => {
      if (current.includes(id)) {
        return current.filter((item) => item !== id);
      }

      if (current.length === 0) {
        setCustomerOrderId(orderId);
        setFormError("");
        return [id];
      }

      const shipment = eligible.find((item) => item.id === id);
      if (shipment?.customerOrderId !== customerOrderId) {
        setFormError("All inbound shipments must belong to the same customer order.");
        return current;
      }

      setFormError("");
      return [...current, id];
    });
  }

  async function createConsolidation() {
    if (!customerOrderId || selectedIds.length === 0) {
      setFormError("Select at least one inbound shipment.");
      return;
    }

    setActionId("create");
    setFormError("");
    try {
      await apiFetch("/api/warehouse/consolidations", {
        method: "POST",
        body: JSON.stringify({
          customerOrderId,
          inboundShipmentIds: selectedIds,
        }),
      });
      setSelectedIds([]);
      setCustomerOrderId("");
      setCreateOpen(false);
      await loadData();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to create consolidation.");
    } finally {
      setActionId(null);
    }
  }

  async function addPackage(consolidationId: string) {
    const weight = Number(packageWeight);
    if (!weight || weight <= 0) {
      setFormError("Package weight must be positive.");
      return;
    }

    setActionId(consolidationId);
    setFormError("");
    try {
      await apiFetch(`/api/warehouse/consolidations/${consolidationId}/packages`, {
        method: "POST",
        body: JSON.stringify({ weightKg: weight }),
      });
      setPackageWeight("");
      setActiveConsolidationId(null);
      await loadData();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to add package.");
    } finally {
      setActionId(null);
    }
  }

  async function sealConsolidation(consolidationId: string) {
    setActionId(`seal-${consolidationId}`);
    setError("");
    try {
      await apiFetch(`/api/warehouse/consolidations/${consolidationId}/seal`, {
        method: "POST",
      });
      await loadData();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to seal consolidation.");
    } finally {
      setActionId(null);
    }
  }

  return (
    <AdminShell title="Consolidations">
      <div style={{ marginBottom: 16, display: "flex", justifyContent: "space-between", gap: 12 }}>
        <Link href="/warehouse" className="button-ghost">
          ← Back to warehouse
        </Link>
        <button
          type="button"
          className="button-primary"
          onClick={() => {
            setSelectedIds([]);
            setCustomerOrderId("");
            setFormError("");
            setCreateOpen(true);
          }}
        >
          Create consolidation
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
                <th>Customer order</th>
                <th>Inbound</th>
                <th>Packages</th>
                <th>Weight</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {consolidations.length === 0 ? (
                <tr>
                  <td colSpan={7} style={{ textAlign: "center", color: "var(--admin-muted)" }}>
                    No consolidations yet
                  </td>
                </tr>
              ) : (
                consolidations.map((consolidation) => (
                  <tr key={consolidation.id}>
                    <td>
                      <strong>{consolidation.referenceNumber}</strong>
                    </td>
                    <td>{consolidation.customerOrderId.slice(0, 8)}</td>
                    <td>{consolidation.inboundShipmentsCount}</td>
                    <td>{consolidation.packagesCount}</td>
                    <td>{consolidation.totalWeightKg.toFixed(2)} kg</td>
                    <td>
                      <span className={`status-badge ${consolidation.status.toLowerCase()}`}>
                        {consolidation.status}
                      </span>
                    </td>
                    <td>
                      {consolidation.status !== "Sealed" ? (
                        <>
                          <button
                            type="button"
                            className="button-secondary"
                            disabled={actionId === consolidation.id}
                            onClick={() => {
                              setFormError("");
                              setPackageWeight("");
                              setActiveConsolidationId(consolidation.id);
                            }}
                          >
                            Add package
                          </button>
                          <button
                            type="button"
                            className="button-primary"
                            style={{ marginLeft: 8 }}
                            disabled={
                              actionId === `seal-${consolidation.id}`
                              || consolidation.packagesCount === 0
                            }
                            onClick={() => void sealConsolidation(consolidation.id)}
                          >
                            Seal
                          </button>
                        </>
                      ) : (
                        "Ready for logistics"
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
        open={createOpen}
        title="Create consolidation"
        onClose={() => setCreateOpen(false)}
        wide
        footer={
          <>
            <button
              type="button"
              className="button-ghost"
              onClick={() => setCreateOpen(false)}
              disabled={actionId === "create"}
            >
              Cancel
            </button>
            <button
              type="button"
              className="button-primary"
              disabled={actionId === "create" || selectedIds.length === 0}
              onClick={() => void createConsolidation()}
            >
              {actionId === "create" ? "Creating..." : "Create consolidation"}
            </button>
          </>
        }
      >
        {formError && createOpen ? <p className="form-error">{formError}</p> : null}
        <p style={{ color: "var(--admin-muted)", fontSize: 14, marginTop: 0 }}>
          Select received inbound shipments from the same customer order.
        </p>
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th />
                <th>Reference</th>
                <th>Customer order</th>
                <th>Items</th>
                <th>Weight</th>
              </tr>
            </thead>
            <tbody>
              {eligible.length === 0 ? (
                <tr>
                  <td colSpan={5} style={{ textAlign: "center", color: "var(--admin-muted)" }}>
                    No eligible inbound shipments
                  </td>
                </tr>
              ) : (
                eligible.map((shipment) => (
                  <tr key={shipment.id}>
                    <td>
                      <input
                        type="checkbox"
                        checked={selectedIds.includes(shipment.id)}
                        onChange={() =>
                          toggleShipment(shipment.id, shipment.customerOrderId)
                        }
                      />
                    </td>
                    <td>{shipment.referenceNumber}</td>
                    <td>{shipment.customerOrderId.slice(0, 8)}</td>
                    <td>{shipment.itemsCount}</td>
                    <td>{shipment.weightKg ?? "—"} kg</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </Modal>

      <Modal
        open={activeConsolidationId !== null}
        title="Add package"
        onClose={() => setActiveConsolidationId(null)}
        footer={
          <>
            <button
              type="button"
              className="button-ghost"
              onClick={() => setActiveConsolidationId(null)}
            >
              Cancel
            </button>
            <button
              type="button"
              className="button-primary"
              disabled={actionId === activeConsolidationId}
              onClick={() => {
                if (activeConsolidationId) {
                  void addPackage(activeConsolidationId);
                }
              }}
            >
              Save package
            </button>
          </>
        }
      >
        {formError && activeConsolidationId ? <p className="form-error">{formError}</p> : null}
        <div className="form-field">
          <label htmlFor="packageWeight">Weight (kg)</label>
          <input
            id="packageWeight"
            type="number"
            min="0.001"
            step="0.001"
            value={packageWeight}
            onChange={(event) => setPackageWeight(event.target.value)}
          />
        </div>
      </Modal>
    </AdminShell>
  );
}
