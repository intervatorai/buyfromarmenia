"use client";

import { FormEvent, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { ApiError, apiFetch } from "@/lib/api";
import { useAuth } from "@/components/providers/AuthProvider";

export default function AdminAccountPage() {
  const { user } = useAuth();
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError("");
    setSuccess("");

    if (newPassword.length < 6) {
      setError("New password must be at least 6 characters.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("New password and confirmation do not match.");
      return;
    }

    setIsSaving(true);
    try {
      await apiFetch("/api/auth/change-password", {
        method: "POST",
        body: JSON.stringify({ currentPassword, newPassword }),
      });
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
      setSuccess("Password updated.");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to change password.");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <AdminShell title="My account">
      <div className="admin-card" style={{ maxWidth: 480 }}>
        <p className="admin-card-label">Signed in as</p>
        <p style={{ marginTop: 4, marginBottom: 20 }}>
          <strong>{user?.fullName}</strong>
          <br />
          <span style={{ color: "#64748b" }}>
            {user?.email} · {user?.role}
          </span>
        </p>

        <h2 style={{ fontSize: 16, marginBottom: 12 }}>Change password</h2>
        <form onSubmit={handleSubmit} className="form-field" style={{ display: "grid", gap: 12 }}>
          <label>
            Current password
            <input
              type="password"
              autoComplete="current-password"
              required
              value={currentPassword}
              onChange={(event) => setCurrentPassword(event.target.value)}
            />
          </label>
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
            Confirm new password
            <input
              type="password"
              autoComplete="new-password"
              required
              minLength={6}
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
            />
          </label>

          {error ? (
            <p style={{ color: "#b91c1c", margin: 0, fontSize: 14 }}>{error}</p>
          ) : null}
          {success ? (
            <p style={{ color: "#15803d", margin: 0, fontSize: 14 }}>{success}</p>
          ) : null}

          <button className="button-primary" type="submit" disabled={isSaving}>
            {isSaving ? "Saving..." : "Update password"}
          </button>
        </form>
      </div>
    </AdminShell>
  );
}
