"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, Fragment, useCallback, useEffect, useRef, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { Modal } from "@/components/ui/Modal";
import { PromptModal } from "@/components/ui/PromptModal";
import { VendorFormModal } from "@/components/ui/VendorFormModal";
import { VendorSubpanels } from "@/components/ui/VendorSubpanels";
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

type PasswordTarget = {
  id: string;
  tradingName: string;
  email: string;
};

export default function VendorsPage() {
  const router = useRouter();
  const [suppliers, setSuppliers] = useState<SupplierListItem[]>([]);
  const [filter, setFilter] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [formOpen, setFormOpen] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [rejectId, setRejectId] = useState<string | null>(null);
  const [expandedId, setExpandedId] = useState("");
  const [openMenuId, setOpenMenuId] = useState("");
  const [actionId, setActionId] = useState<string | null>(null);
  const [passwordTarget, setPasswordTarget] = useState<PasswordTarget | null>(null);
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [passwordError, setPasswordError] = useState("");
  const [passwordSaving, setPasswordSaving] = useState(false);
  const menuRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!openMenuId) {
      return;
    }

    function handlePointerDown(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setOpenMenuId("");
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setOpenMenuId("");
      }
    }

    document.addEventListener("mousedown", handlePointerDown);
    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.removeEventListener("mousedown", handlePointerDown);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [openMenuId]);

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
    setActionId(id);
    try {
      await apiFetch(`/api/suppliers/${id}/approve`, { method: "POST" });
      await loadSuppliers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to approve.");
    } finally {
      setActionId(null);
    }
  }

  function openPasswordModal(supplier: SupplierListItem) {
    setPasswordTarget({
      id: supplier.id,
      tradingName: supplier.tradingName,
      email: supplier.email,
    });
    setNewPassword("");
    setConfirmPassword("");
    setPasswordError("");
  }

  async function handleSetPassword(event: FormEvent) {
    event.preventDefault();
    if (!passwordTarget) {
      return;
    }

    setPasswordError("");

    if (newPassword.length < 6) {
      setPasswordError("Password must be at least 6 characters.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setPasswordError("Password and confirmation do not match.");
      return;
    }

    setPasswordSaving(true);
    try {
      await apiFetch(`/api/suppliers/${passwordTarget.id}/set-password`, {
        method: "POST",
        body: JSON.stringify({ newPassword }),
      });
      setPasswordTarget(null);
    } catch (err) {
      setPasswordError(err instanceof ApiError ? err.message : "Failed to set password.");
    } finally {
      setPasswordSaving(false);
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
              <th className="product-expand-cell" aria-label="Expand" />
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
                <td colSpan={7}>No suppliers found.</td>
              </tr>
            ) : (
              suppliers.map((supplier) => {
                const isExpanded = expandedId === supplier.id;
                const canReview =
                  supplier.status === "ApplicationSubmitted"
                  || supplier.status === "UnderReview";

                return (
                  <Fragment key={supplier.id}>
                    <tr>
                      <td className="product-expand-cell">
                        <button
                          type="button"
                          className="product-expand-btn"
                          aria-expanded={isExpanded}
                          aria-label={isExpanded ? "Collapse vendor" : "Expand vendor"}
                          onClick={() =>
                            setExpandedId((current) =>
                              current === supplier.id ? "" : supplier.id,
                            )
                          }
                        >
                          {isExpanded ? "▾" : "▸"}
                        </button>
                      </td>
                      <td>
                        <strong>{supplier.tradingName}</strong>
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
                      <td>
                        <div
                          className="row-menu"
                          ref={openMenuId === supplier.id ? menuRef : undefined}
                        >
                          <button
                            type="button"
                            className="row-menu-trigger"
                            aria-haspopup="menu"
                            aria-expanded={openMenuId === supplier.id}
                            aria-label="Vendor actions"
                            disabled={actionId === supplier.id}
                            onClick={() =>
                              setOpenMenuId((current) =>
                                current === supplier.id ? "" : supplier.id,
                              )
                            }
                          >
                            {actionId === supplier.id ? "…" : "⋯"}
                          </button>
                          {openMenuId === supplier.id ? (
                            <div className="row-menu-dropdown" role="menu">
                              <Link
                                href={`/vendors/${supplier.id}`}
                                role="menuitem"
                                className="row-menu-item"
                                onClick={() => setOpenMenuId("")}
                              >
                                View
                              </Link>
                              <button
                                type="button"
                                role="menuitem"
                                className="row-menu-item"
                                onClick={() => {
                                  setOpenMenuId("");
                                  setEditId(supplier.id);
                                  setFormOpen(true);
                                }}
                              >
                                Edit
                              </button>
                              <button
                                type="button"
                                role="menuitem"
                                className="row-menu-item"
                                onClick={() => {
                                  setOpenMenuId("");
                                  openPasswordModal(supplier);
                                }}
                              >
                                Change password
                              </button>
                              {canReview ? (
                                <>
                                  <button
                                    type="button"
                                    role="menuitem"
                                    className="row-menu-item"
                                    onClick={() => {
                                      setOpenMenuId("");
                                      void handleApprove(supplier.id);
                                    }}
                                  >
                                    Approve
                                  </button>
                                  <button
                                    type="button"
                                    role="menuitem"
                                    className="row-menu-item danger"
                                    onClick={() => {
                                      setOpenMenuId("");
                                      setRejectId(supplier.id);
                                    }}
                                  >
                                    Reject
                                  </button>
                                </>
                              ) : null}
                            </div>
                          ) : null}
                        </div>
                      </td>
                    </tr>
                    {isExpanded ? (
                      <tr className="product-expand-row">
                        <td colSpan={7}>
                          <VendorSubpanels
                            vendorId={supplier.id}
                            collapsible={false}
                            defaultOpen
                            onChanged={() => void loadSuppliers()}
                          />
                        </td>
                      </tr>
                    ) : null}
                  </Fragment>
                );
              })
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

      <Modal
        open={passwordTarget !== null}
        title="Change password"
        onClose={() => setPasswordTarget(null)}
        footer={
          <>
            <button
              type="button"
              className="button-ghost"
              onClick={() => setPasswordTarget(null)}
            >
              Cancel
            </button>
            <button
              type="submit"
              form="vendor-password-form"
              className="button-primary"
              disabled={passwordSaving}
            >
              {passwordSaving ? "Saving…" : "Update password"}
            </button>
          </>
        }
      >
        {passwordTarget ? (
          <form
            id="vendor-password-form"
            className="form-field"
            onSubmit={(event) => void handleSetPassword(event)}
          >
            <p style={{ marginTop: 0, color: "var(--admin-muted)", fontSize: 14 }}>
              Set portal password for <strong>{passwordTarget.tradingName}</strong> (
              {passwordTarget.email}).
            </p>
            <label>
              New password
              <input
                type="password"
                autoComplete="new-password"
                required
                minLength={6}
                value={newPassword}
                onChange={(event) => setNewPassword(event.target.value)}
              />
            </label>
            <label>
              Confirm password
              <input
                type="password"
                autoComplete="new-password"
                required
                minLength={6}
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
              />
            </label>
            {passwordError ? <p className="form-error">{passwordError}</p> : null}
          </form>
        ) : null}
      </Modal>
    </AdminShell>
  );
}
