"use client";

import Link from "next/link";
import { useState, type FormEvent } from "react";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { RequireAuth } from "@/components/providers/RequireAuth";
import { ApiError, apiFetch } from "@/lib/api";

function ChangePasswordContent() {
  const { translate } = useLanguage();
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
      setError(translate("passwordTooShort"));
      return;
    }

    if (newPassword !== confirmPassword) {
      setError(translate("passwordMismatch"));
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
      setSuccess(translate("passwordUpdated"));
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : translate("passwordChangeFailed"),
      );
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <PublicSiteLayout>
      <section className="section container account-page">
        <div className="account-hero" style={{ marginBottom: 28 }}>
          <div className="account-hero-content">
            <p className="account-eyebrow">{translate("accountTitle")}</p>
            <h1>{translate("changePassword")}</h1>
            <p className="account-hero-hint">{translate("changePasswordHint")}</p>
          </div>
          <Link href="/account" className="button button-secondary account-signout">
            {translate("backToAccount")}
          </Link>
        </div>

        <form
          className="checkout-form"
          onSubmit={(event) => void handleSubmit(event)}
          style={{ maxWidth: 420 }}
        >
          <label>
            {translate("currentPassword")}
            <input
              type="password"
              autoComplete="current-password"
              required
              value={currentPassword}
              onChange={(event) => setCurrentPassword(event.target.value)}
            />
          </label>
          <label>
            {translate("newPassword")}
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
            {translate("confirmNewPassword")}
            <input
              type="password"
              autoComplete="new-password"
              required
              minLength={6}
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
            />
          </label>

          {error ? <p className="catalog-message catalog-error">{error}</p> : null}
          {success ? <p className="account-success">{success}</p> : null}

          <div className="account-form-actions">
            <button type="submit" className="button button-primary" disabled={isSaving}>
              {isSaving ? translate("savingPassword") : translate("updatePassword")}
            </button>
          </div>
        </form>
      </section>
    </PublicSiteLayout>
  );
}

export default function ChangePasswordPage() {
  return (
    <RequireAuth>
      <ChangePasswordContent />
    </RequireAuth>
  );
}
