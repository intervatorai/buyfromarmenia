"use client";

import { ApiError, apiFetch } from "@/lib/api";
import { saveAuth, type SupplierAuthResponse } from "@/lib/auth";
import { getSupplierId, setSupplierId } from "@/lib/supplier-session";
import { useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";

type Step = 1 | 2 | 3 | 4;

type SupplierDetail = {
  id: string;
  legalName: string;
  tradingName: string;
  status: string;
  contactPerson: string;
  email: string;
  phone: string;
  taxNumber?: string | null;
  registrationNumber?: string | null;
  bankAccounts: { id: string; iban: string }[];
  documents: { id: string; fileName: string }[];
};

const initialCompany = {
  legalName: "",
  tradingName: "",
  contactPerson: "",
  email: "",
  phone: "",
  taxNumber: "",
  registrationNumber: "",
  legalCountryCode: "AM",
  legalCity: "",
  legalLine1: "",
};

const initialBank = {
  bankName: "",
  accountHolder: "",
  iban: "",
  currency: "USD",
  swift: "",
};

export function OnboardingWizard() {
  const router = useRouter();
  const [step, setStep] = useState<Step>(1);
  const [supplierId, setSupplierIdState] = useState<string | null>(null);
  const [company, setCompany] = useState(initialCompany);
  const [password, setPassword] = useState("");
  const [bank, setBank] = useState(initialBank);
  const [documentFileName, setDocumentFileName] = useState("");
  const [documentUrl, setDocumentUrl] = useState("");
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [supplier, setSupplier] = useState<SupplierDetail | null>(null);

  useEffect(() => {
    const existingId = getSupplierId();
    if (existingId) {
      setSupplierIdState(existingId);
      void loadSupplier(existingId);
    }
  }, []);

  async function loadSupplier(id: string) {
    try {
      const data = await apiFetch<SupplierDetail>(`/api/suppliers/${id}`);
      setSupplier(data);
      setCompany({
        legalName: data.legalName,
        tradingName: data.tradingName,
        contactPerson: data.contactPerson,
        email: data.email,
        phone: data.phone,
        taxNumber: data.taxNumber ?? "",
        registrationNumber: data.registrationNumber ?? "",
        legalCountryCode: "AM",
        legalCity: "",
        legalLine1: "",
      });

      if (data.status === "ApplicationSubmitted" || data.status === "UnderReview") {
        setStep(4);
      } else if (data.bankAccounts.length > 0) {
        setStep(3);
      } else if (data.documents.length > 0) {
        setStep(3);
      } else if (data.status !== "Draft") {
        setStep(4);
      }
    } catch {
      // Fresh onboarding
    }
  }

  async function handleRegister(event: FormEvent) {
    event.preventDefault();
    setError("");
    setIsSubmitting(true);

    try {
      const result = await apiFetch<SupplierAuthResponse>("/api/suppliers", {
        method: "POST",
        body: JSON.stringify({ ...company, password }),
      });

      saveAuth(result);
      setSupplierId(result.supplierId);
      setSupplierIdState(result.supplierId);
      setStep(2);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Registration failed.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleAddDocument(event: FormEvent) {
    event.preventDefault();
    if (!supplierId) {
      return;
    }

    setError("");
    setIsSubmitting(true);

    try {
      await apiFetch(`/api/suppliers/${supplierId}/documents`, {
        method: "POST",
        body: JSON.stringify({
          documentType: "RegistrationCertificate",
          fileName: documentFileName,
          fileUrl: documentUrl,
        }),
      });

      setStep(3);
      await loadSupplier(supplierId);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save document.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleAddBank(event: FormEvent) {
    event.preventDefault();
    if (!supplierId) {
      return;
    }

    setError("");
    setIsSubmitting(true);

    try {
      await apiFetch(`/api/suppliers/${supplierId}/bank-accounts`, {
        method: "POST",
        body: JSON.stringify(bank),
      });

      setStep(4);
      await loadSupplier(supplierId);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save bank account.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleSubmitApplication() {
    if (!supplierId) {
      return;
    }

    setError("");
    setIsSubmitting(true);

    try {
      await apiFetch(`/api/suppliers/${supplierId}/submit`, { method: "POST" });
      await loadSupplier(supplierId);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not submit application.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="supplier-card" style={{ maxWidth: 720 }}>
      <div style={{ display: "flex", gap: 8, marginBottom: 24 }}>
        {[1, 2, 3, 4].map((n) => (
          <div
            key={n}
            style={{
              flex: 1,
              height: 4,
              borderRadius: 4,
              background: step >= n ? "#1e3a5f" : "#e2e8f0",
            }}
          />
        ))}
      </div>

      {error ? (
        <div style={{ marginBottom: 16, color: "#b91c1c", fontSize: 14 }}>{error}</div>
      ) : null}

      {step === 1 ? (
        <form onSubmit={handleRegister}>
          <h2 style={{ margin: "0 0 8px", fontSize: 18 }}>Company information</h2>
          <p style={{ margin: "0 0 20px", color: "#64748b" }}>
            Register your business on BuyFromArmenia.
          </p>
          <div style={{ display: "grid", gap: 14 }}>
            {[
              ["legalName", "Legal name"],
              ["tradingName", "Trading name"],
              ["contactPerson", "Contact person"],
              ["email", "Email"],
              ["phone", "Phone"],
              ["taxNumber", "Tax number"],
              ["legalCity", "City"],
              ["legalLine1", "Legal address"],
            ].map(([key, label]) => (
              <label key={key} style={{ display: "grid", gap: 6 }}>
                <span style={{ fontSize: 13, fontWeight: 500 }}>{label}</span>
                <input
                  required={["legalName", "tradingName", "contactPerson", "email", "phone"].includes(key)}
                  value={company[key as keyof typeof company]}
                  onChange={(e) =>
                    setCompany((prev) => ({ ...prev, [key]: e.target.value }))
                  }
                  style={inputStyle}
                />
              </label>
            ))}
            <label style={{ display: "grid", gap: 6 }}>
              <span style={{ fontSize: 13, fontWeight: 500 }}>Password</span>
              <input
                required
                type="password"
                minLength={8}
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                style={inputStyle}
                autoComplete="new-password"
              />
            </label>
          </div>
          <button type="submit" className="button-primary" style={{ marginTop: 20 }} disabled={isSubmitting}>
            {isSubmitting ? "Saving..." : "Continue"}
          </button>
        </form>
      ) : null}

      {step === 2 ? (
        <form onSubmit={handleAddDocument}>
          <h2 style={{ margin: "0 0 8px", fontSize: 18 }}>Registration documents</h2>
          <p style={{ margin: "0 0 20px", color: "#64748b" }}>
            Add certificate metadata (file upload — later).
          </p>
          <div style={{ display: "grid", gap: 14 }}>
            <label style={{ display: "grid", gap: 6 }}>
              <span style={{ fontSize: 13, fontWeight: 500 }}>File name</span>
              <input
                required
                value={documentFileName}
                onChange={(e) => setDocumentFileName(e.target.value)}
                style={inputStyle}
              />
            </label>
            <label style={{ display: "grid", gap: 6 }}>
              <span style={{ fontSize: 13, fontWeight: 500 }}>File URL</span>
              <input
                required
                value={documentUrl}
                onChange={(e) => setDocumentUrl(e.target.value)}
                placeholder="https://..."
                style={inputStyle}
              />
            </label>
          </div>
          <button type="submit" className="button-primary" style={{ marginTop: 20 }} disabled={isSubmitting}>
            {isSubmitting ? "Saving..." : "Continue"}
          </button>
        </form>
      ) : null}

      {step === 3 ? (
        <form onSubmit={handleAddBank}>
          <h2 style={{ margin: "0 0 8px", fontSize: 18 }}>Bank account</h2>
          <p style={{ margin: "0 0 20px", color: "#64748b" }}>
            Required for supplier payouts.
          </p>
          <div style={{ display: "grid", gap: 14 }}>
            {[
              ["bankName", "Bank name"],
              ["accountHolder", "Account holder"],
              ["iban", "IBAN"],
              ["currency", "Currency"],
              ["swift", "SWIFT (optional)"],
            ].map(([key, label]) => (
              <label key={key} style={{ display: "grid", gap: 6 }}>
                <span style={{ fontSize: 13, fontWeight: 500 }}>{label}</span>
                <input
                  required={key !== "swift"}
                  value={bank[key as keyof typeof bank]}
                  onChange={(e) => setBank((prev) => ({ ...prev, [key]: e.target.value }))}
                  style={inputStyle}
                />
              </label>
            ))}
          </div>
          <button type="submit" className="button-primary" style={{ marginTop: 20 }} disabled={isSubmitting}>
            {isSubmitting ? "Saving..." : "Continue"}
          </button>
        </form>
      ) : null}

      {step === 4 ? (
        <div>
          <h2 style={{ margin: "0 0 8px", fontSize: 18 }}>Submit application</h2>
          <p style={{ margin: "0 0 20px", color: "#64748b" }}>
            Status: <strong>{supplier?.status ?? "Draft"}</strong>
          </p>

          {supplier?.status === "Draft" || supplier?.status === "ChangesRequested" ? (
            <button
              type="button"
              className="button-primary"
              onClick={handleSubmitApplication}
              disabled={isSubmitting}
            >
              {isSubmitting ? "Submitting..." : "Submit for review"}
            </button>
          ) : (
            <div>
              <p style={{ color: "#475569", marginBottom: 16 }}>
                Your application has been submitted. We will notify you after review.
              </p>
              <button type="button" className="button-secondary" onClick={() => router.push("/")}>
                Go to dashboard
              </button>
            </div>
          )}
        </div>
      ) : null}
    </div>
  );
}

const inputStyle: React.CSSProperties = {
  border: "1px solid #e2e8f0",
  borderRadius: 8,
  padding: "10px 12px",
};
