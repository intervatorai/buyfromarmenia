"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { PromptModal } from "@/components/ui/PromptModal";
import { VendorFormModal } from "@/components/ui/VendorFormModal";
import { ApiError, apiFetch } from "@/lib/api";

type SupplierDetail = {
  id: string;
  legalName: string;
  tradingName: string;
  status: string;
  contactPerson: string;
  email: string;
  phone: string;
  documents: Array<{
    id: string;
    documentType: string;
    fileName: string;
    fileUrl: string;
    status: string;
  }>;
  bankAccounts: Array<{
    id: string;
    bankName: string;
    accountHolder: string;
    iban: string;
    swift?: string | null;
    currency: string;
    isPrimary: boolean;
  }>;
};

type PromptKind = "reject" | "request-changes" | "suspend" | null;

export default function VendorDetailPage() {
  const params = useParams<{ id: string }>();
  const [supplier, setSupplier] = useState<SupplierDetail | null>(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [promptKind, setPromptKind] = useState<PromptKind>(null);

  const load = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      setSupplier(await apiFetch<SupplierDetail>(`/api/suppliers/${params.id}`));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load vendor.");
    } finally {
      setIsLoading(false);
    }
  }, [params.id]);

  useEffect(() => {
    void load();
  }, [load]);

  async function runAction(path: string, body?: object) {
    setBusy(true);
    setError("");
    try {
      await apiFetch(path, {
        method: "POST",
        body: body ? JSON.stringify(body) : undefined,
      });
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Action failed.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <AdminShell title={supplier?.tradingName ?? "Vendor"}>
      <p style={{ marginBottom: 16 }}>
        <Link href="/vendors" className="button-ghost">
          ← Back to vendors
        </Link>
      </p>

      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading...</p> : null}

      {supplier ? (
        <>
          <div className="admin-grid" style={{ marginBottom: 24 }}>
            <div className="admin-card">
              <div className="admin-card-label">Legal name</div>
              <div>{supplier.legalName}</div>
            </div>
            <div className="admin-card">
              <div className="admin-card-label">Status</div>
              <div className="admin-card-value" style={{ fontSize: 18 }}>{supplier.status}</div>
            </div>
            <div className="admin-card">
              <div className="admin-card-label">Contact</div>
              <div>
                {supplier.contactPerson}
                <br />
                {supplier.email}
                <br />
                {supplier.phone}
              </div>
            </div>
          </div>

          <div style={{ display: "flex", gap: 8, flexWrap: "wrap", marginBottom: 24 }}>
            <button type="button" className="button-primary" onClick={() => setEditOpen(true)}>
              Edit
            </button>
            {(supplier.status === "ApplicationSubmitted" || supplier.status === "UnderReview") && (
              <>
                <button
                  type="button"
                  className="button-primary"
                  disabled={busy}
                  onClick={() => void runAction(`/api/suppliers/${supplier.id}/approve`)}
                >
                  Approve
                </button>
                <button
                  type="button"
                  className="button-ghost"
                  disabled={busy}
                  onClick={() => setPromptKind("reject")}
                >
                  Reject
                </button>
                <button
                  type="button"
                  className="button-ghost"
                  disabled={busy}
                  onClick={() => setPromptKind("request-changes")}
                >
                  Request changes
                </button>
              </>
            )}
            {supplier.status === "Active" ? (
              <button
                type="button"
                className="button-ghost"
                disabled={busy}
                onClick={() => setPromptKind("suspend")}
              >
                Suspend
              </button>
            ) : null}
            {supplier.status === "Suspended" ? (
              <button
                type="button"
                className="button-primary"
                disabled={busy}
                onClick={() => void runAction(`/api/suppliers/${supplier.id}/activate`)}
              >
                Activate
              </button>
            ) : null}
          </div>

          <h2>Documents</h2>
          <div className="admin-table-wrap" style={{ marginBottom: 24 }}>
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Type</th>
                  <th>File</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {supplier.documents.length === 0 ? (
                  <tr>
                    <td colSpan={3}>No documents</td>
                  </tr>
                ) : (
                  supplier.documents.map((document) => (
                    <tr key={document.id}>
                      <td>{document.documentType}</td>
                      <td>
                        <a href={document.fileUrl} target="_blank" rel="noreferrer">
                          {document.fileName}
                        </a>
                      </td>
                      <td>{document.status}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          <h2>Bank accounts</h2>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Bank</th>
                  <th>Holder</th>
                  <th>IBAN</th>
                  <th>Currency</th>
                  <th>Primary</th>
                </tr>
              </thead>
              <tbody>
                {supplier.bankAccounts.length === 0 ? (
                  <tr>
                    <td colSpan={5}>No bank accounts</td>
                  </tr>
                ) : (
                  supplier.bankAccounts.map((account) => (
                    <tr key={account.id}>
                      <td>{account.bankName}</td>
                      <td>{account.accountHolder}</td>
                      <td>{account.iban}</td>
                      <td>{account.currency}</td>
                      <td>{account.isPrimary ? "Yes" : "No"}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </>
      ) : null}

      <VendorFormModal
        open={editOpen}
        vendorId={params.id}
        onClose={() => setEditOpen(false)}
        onSaved={() => void load()}
      />

      <PromptModal
        open={promptKind !== null}
        title={
          promptKind === "reject"
            ? "Reject vendor"
            : promptKind === "suspend"
              ? "Suspend vendor"
              : "Request changes"
        }
        confirmLabel={
          promptKind === "reject" ? "Reject" : promptKind === "suspend" ? "Suspend" : "Send"
        }
        onClose={() => setPromptKind(null)}
        onConfirm={async (reason) => {
          if (!supplier || !promptKind) {
            return;
          }
          const path =
            promptKind === "reject"
              ? `/api/suppliers/${supplier.id}/reject`
              : promptKind === "suspend"
                ? `/api/suppliers/${supplier.id}/suspend`
                : `/api/suppliers/${supplier.id}/request-changes`;
          await runAction(path, { reason });
        }}
      />
    </AdminShell>
  );
}
