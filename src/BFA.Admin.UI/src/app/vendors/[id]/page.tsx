"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { FormEvent, useCallback, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { PromptModal } from "@/components/ui/PromptModal";
import { VendorFormModal } from "@/components/ui/VendorFormModal";
import { VendorSubpanels } from "@/components/ui/VendorSubpanels";
import { ApiError, apiFetch } from "@/lib/api";

type SupplierDetail = {
  id: string;
  legalName: string;
  tradingName: string;
  status: string;
  contactPerson: string;
  email: string;
  phone: string;
  hasPortalLogin: boolean;
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

  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [passwordError, setPasswordError] = useState("");
  const [passwordSuccess, setPasswordSuccess] = useState("");
  const [isSavingPassword, setIsSavingPassword] = useState(false);

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

  async function handleSetPassword(event: FormEvent) {
    event.preventDefault();
    if (!supplier) {
      return;
    }

    setPasswordError("");
    setPasswordSuccess("");

    if (newPassword.length < 6) {
      setPasswordError("Password must be at least 6 characters.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setPasswordError("Password and confirmation do not match.");
      return;
    }

    setIsSavingPassword(true);
    try {
      await apiFetch(`/api/suppliers/${supplier.id}/set-password`, {
        method: "POST",
        body: JSON.stringify({ newPassword }),
      });
      setNewPassword("");
      setConfirmPassword("");
      setPasswordSuccess(
        supplier.hasPortalLogin
          ? "Password updated."
          : "Portal login created and password set.",
      );
      await load();
    } catch (err) {
      setPasswordError(err instanceof ApiError ? err.message : "Failed to set password.");
    } finally {
      setIsSavingPassword(false);
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
            <div className="admin-card">
              <div className="admin-card-label">Partner portal</div>
              <div>{supplier.hasPortalLogin ? "Login enabled" : "No login yet"}</div>
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

          <h2>Portal password</h2>
          <div className="admin-card" style={{ maxWidth: 480, marginBottom: 24 }}>
            <p style={{ marginTop: 0, color: "#64748b", fontSize: 14 }}>
              {supplier.hasPortalLogin
                ? `Set a new password for ${supplier.email}.`
                : `Create partner portal login for ${supplier.email} and set the password.`}
            </p>
            <form
              onSubmit={(event) => void handleSetPassword(event)}
              className="form-field"
              style={{ display: "grid", gap: 12 }}
            >
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
              {passwordSuccess ? (
                <p style={{ margin: 0, color: "#15803d", fontSize: 14 }}>{passwordSuccess}</p>
              ) : null}
              <button className="button-primary" type="submit" disabled={isSavingPassword}>
                {isSavingPassword
                  ? "Saving..."
                  : supplier.hasPortalLogin
                    ? "Update password"
                    : "Create login & set password"}
              </button>
            </form>
          </div>

          <h2 style={{ marginBottom: 12 }}>Documents &amp; bank</h2>
          <VendorSubpanels vendorId={supplier.id} collapsible={false} defaultOpen />
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
