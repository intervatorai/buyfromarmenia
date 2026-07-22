"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { PromptModal } from "@/components/ui/PromptModal";
import { VendorFormModal } from "@/components/ui/VendorFormModal";
import { ApiError, apiFetch } from "@/lib/api";

type SupplierListItem = {
  id: string;
  legalName: string;
  tradingName: string;
  status: string;
  contactPerson: string;
  email: string;
  phone: string;
  bankAccountsCount: number;
  documentsCount: number;
  createdAt: string;
};

export default function VendorsPage() {
  const router = useRouter();
  const [suppliers, setSuppliers] = useState<SupplierListItem[]>([]);
  const [filter, setFilter] = useState("ApplicationSubmitted");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [formOpen, setFormOpen] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [rejectId, setRejectId] = useState<string | null>(null);

  const loadSuppliers = useCallback(async () => {
    setIsLoading(true);
    setError("");

    try {
      const query = filter ? `?status=${encodeURIComponent(filter)}` : "";
      const data = await apiFetch<SupplierListItem[]>(`/api/suppliers${query}`);
      setSuppliers(data);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load suppliers.");
    } finally {
      setIsLoading(false);
    }
  }, [filter]);

  useEffect(() => {
    void loadSuppliers();
  }, [loadSuppliers]);

  async function handleApprove(id: string) {
    try {
      await apiFetch(`/api/suppliers/${id}/approve`, { method: "POST" });
      await loadSuppliers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to approve.");
    }
  }

  return (
    <AdminShell title="Vendors">
      <div style={{ marginBottom: 16, display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap" }}>
        <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
          {["ApplicationSubmitted", "UnderReview", "Active", "Suspended", "Rejected", ""].map((status) => (
            <button
              key={status || "all"}
              type="button"
              className={filter === status ? "button-primary" : "button-ghost"}
              onClick={() => setFilter(status)}
            >
              {status || "All"}
            </button>
          ))}
        </div>
        <button
          type="button"
          className="button-primary"
          onClick={() => {
            setEditId(null);
            setFormOpen(true);
          }}
        >
          Add vendor
        </button>
      </div>

      {error ? <div className="form-error">{error}</div> : null}
      {isLoading ? <p>Loading...</p> : null}

      <div className="admin-table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>Company</th>
              <th>Contact</th>
              <th>Status</th>
              <th>Docs</th>
              <th>Bank</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {suppliers.length === 0 ? (
              <tr>
                <td colSpan={6}>No suppliers found.</td>
              </tr>
            ) : (
              suppliers.map((supplier) => (
                <tr key={supplier.id}>
                  <td>
                    <Link href={`/vendors/${supplier.id}`}>
                      <strong>{supplier.tradingName}</strong>
                    </Link>
                    <br />
                    <span style={{ color: "#9ca3af", fontSize: 12 }}>{supplier.legalName}</span>
                  </td>
                  <td>
                    {supplier.contactPerson}
                    <br />
                    <span style={{ color: "#9ca3af", fontSize: 12 }}>{supplier.email}</span>
                  </td>
                  <td>{supplier.status}</td>
                  <td>{supplier.documentsCount}</td>
                  <td>{supplier.bankAccountsCount}</td>
                  <td style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                    <Link href={`/vendors/${supplier.id}`} className="button-ghost">
                      Open
                    </Link>
                    <button
                      type="button"
                      className="button-ghost"
                      onClick={() => {
                        setEditId(supplier.id);
                        setFormOpen(true);
                      }}
                    >
                      Edit
                    </button>
                    {(supplier.status === "ApplicationSubmitted"
                      || supplier.status === "UnderReview") && (
                      <>
                        <button
                          type="button"
                          className="button-primary"
                          onClick={() => void handleApprove(supplier.id)}
                        >
                          Approve
                        </button>
                        <button
                          type="button"
                          className="button-ghost"
                          onClick={() => setRejectId(supplier.id)}
                        >
                          Reject
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <VendorFormModal
        open={formOpen}
        vendorId={editId}
        onClose={() => setFormOpen(false)}
        onSaved={(id) => {
          void loadSuppliers();
          if (!editId) {
            router.push(`/vendors/${id}`);
          }
        }}
      />

      <PromptModal
        open={rejectId !== null}
        title="Reject vendor"
        confirmLabel="Reject"
        onClose={() => setRejectId(null)}
        onConfirm={async (reason) => {
          if (!rejectId) {
            return;
          }
          await apiFetch(`/api/suppliers/${rejectId}/reject`, {
            method: "POST",
            body: JSON.stringify({ reason }),
          });
          await loadSuppliers();
        }}
      />
    </AdminShell>
  );
}
