"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { ApiError } from "@/lib/api";
import {
  supplierApiFetch,
  type SupplierRegisterResponse,
} from "@/lib/supplier-api";

type Step = 1 | 2 | 3 | 4;

const initialCompany = {
  legalName: "",
  tradingName: "",
  contactPerson: "",
  email: "",
  phone: "",
  taxNumber: "",
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

export default function SellPage() {
  const { translate } = useLanguage();
  const [step, setStep] = useState<Step>(1);
  const [company, setCompany] = useState(initialCompany);
  const [password, setPassword] = useState("");
  const [documentFileName, setDocumentFileName] = useState("");
  const [documentUrl, setDocumentUrl] = useState("");
  const [bank, setBank] = useState(initialBank);
  const [supplierId, setSupplierId] = useState("");
  const [accessToken, setAccessToken] = useState("");
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showThankYou, setShowThankYou] = useState(false);

  useEffect(() => {
    if (!showThankYou) return;
    document.body.classList.add("modal-open");
    return () => document.body.classList.remove("modal-open");
  }, [showThankYou]);

  async function handleRegister(event: FormEvent) {
    event.preventDefault();
    setError("");
    setIsSubmitting(true);

    try {
      const result = await supplierApiFetch<SupplierRegisterResponse>(
        "/api/suppliers",
        {
          method: "POST",
          body: JSON.stringify({ ...company, password }),
        },
      );
      setSupplierId(result.supplierId);
      setAccessToken(result.accessToken);
      setStep(2);
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : translate("sellerApplyFailed"),
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDocument(event: FormEvent) {
    event.preventDefault();
    if (!supplierId || !accessToken) return;

    setError("");
    setIsSubmitting(true);
    try {
      await supplierApiFetch(
        `/api/suppliers/${supplierId}/documents`,
        {
          method: "POST",
          body: JSON.stringify({
            documentType: "RegistrationCertificate",
            fileName: documentFileName,
            fileUrl: documentUrl,
          }),
        },
        accessToken,
      );
      setStep(3);
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : translate("sellerApplyFailed"),
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleBank(event: FormEvent) {
    event.preventDefault();
    if (!supplierId || !accessToken) return;

    setError("");
    setIsSubmitting(true);
    try {
      await supplierApiFetch(
        `/api/suppliers/${supplierId}/bank-accounts`,
        {
          method: "POST",
          body: JSON.stringify({ ...bank, isPrimary: true }),
        },
        accessToken,
      );
      setStep(4);
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : translate("sellerApplyFailed"),
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleSubmitApplication() {
    if (!supplierId || !accessToken) return;

    setError("");
    setIsSubmitting(true);
    try {
      await supplierApiFetch(
        `/api/suppliers/${supplierId}/submit`,
        { method: "POST" },
        accessToken,
      );
      setAccessToken("");
      setShowThankYou(true);
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : translate("sellerApplyFailed"),
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <PublicSiteLayout>
      <section className="section container catalog-page seller-apply">
        <div className="seller-apply-header">
          <p className="eyebrow">{translate("sellGlobally")}</p>
          <h1>{translate("sellerApplyTitle")}</h1>
          <p className="seller-apply-lead">{translate("sellerApplyLead")}</p>
        </div>

        <div className="seller-apply-steps" aria-hidden="true">
          {[1, 2, 3, 4].map((n) => (
            <span
              key={n}
              className={`seller-apply-step${step >= n || showThankYou ? " active" : ""}`}
            />
          ))}
        </div>

        {error ? <p className="catalog-message catalog-error">{error}</p> : null}

        {step === 1 && !showThankYou ? (
          <form className="seller-apply-card" onSubmit={(e) => void handleRegister(e)}>
            <h2>{translate("sellerApplyCompany")}</h2>
            <div className="seller-apply-grid">
              {(
                [
                  ["legalName", "sellerFieldLegalName", true],
                  ["tradingName", "sellerFieldTradingName", true],
                  ["contactPerson", "sellerFieldContactPerson", true],
                  ["email", "sellerFieldEmail", true],
                  ["phone", "sellerFieldPhone", true],
                  ["taxNumber", "sellerFieldTaxNumber", false],
                  ["legalCity", "sellerFieldCity", false],
                  ["legalLine1", "sellerFieldAddress", false],
                ] as const
              ).map(([key, labelKey, required]) => (
                <label key={key} className="seller-apply-field">
                  <span>{translate(labelKey)}</span>
                  <input
                    required={required}
                    type={key === "email" ? "email" : "text"}
                    value={company[key]}
                    onChange={(e) =>
                      setCompany((prev) => ({ ...prev, [key]: e.target.value }))
                    }
                  />
                </label>
              ))}
              <label className="seller-apply-field">
                <span>{translate("sellerFieldPassword")}</span>
                <input
                  required
                  type="password"
                  minLength={8}
                  autoComplete="new-password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                />
              </label>
            </div>
            <button
              type="submit"
              className="button button-primary"
              disabled={isSubmitting}
            >
              {isSubmitting ? translate("sellerApplySaving") : translate("continue")}
            </button>
          </form>
        ) : null}

        {step === 2 && !showThankYou ? (
          <form className="seller-apply-card" onSubmit={(e) => void handleDocument(e)}>
            <h2>{translate("sellerApplyDocuments")}</h2>
            <p className="seller-apply-note">{translate("sellerApplyDocumentsHint")}</p>
            <div className="seller-apply-grid">
              <label className="seller-apply-field">
                <span>{translate("sellerFieldFileName")}</span>
                <input
                  required
                  value={documentFileName}
                  onChange={(e) => setDocumentFileName(e.target.value)}
                />
              </label>
              <label className="seller-apply-field">
                <span>{translate("sellerFieldFileUrl")}</span>
                <input
                  required
                  value={documentUrl}
                  onChange={(e) => setDocumentUrl(e.target.value)}
                  placeholder="https://..."
                />
              </label>
            </div>
            <button
              type="submit"
              className="button button-primary"
              disabled={isSubmitting}
            >
              {isSubmitting ? translate("sellerApplySaving") : translate("continue")}
            </button>
          </form>
        ) : null}

        {step === 3 && !showThankYou ? (
          <form className="seller-apply-card" onSubmit={(e) => void handleBank(e)}>
            <h2>{translate("sellerApplyBank")}</h2>
            <div className="seller-apply-grid">
              {(
                [
                  ["bankName", "sellerFieldBankName", true],
                  ["accountHolder", "sellerFieldAccountHolder", true],
                  ["iban", "sellerFieldIban", true],
                  ["currency", "sellerFieldCurrency", true],
                  ["swift", "sellerFieldSwift", false],
                ] as const
              ).map(([key, labelKey, required]) => (
                <label key={key} className="seller-apply-field">
                  <span>{translate(labelKey)}</span>
                  <input
                    required={required}
                    value={bank[key]}
                    onChange={(e) =>
                      setBank((prev) => ({ ...prev, [key]: e.target.value }))
                    }
                  />
                </label>
              ))}
            </div>
            <button
              type="submit"
              className="button button-primary"
              disabled={isSubmitting}
            >
              {isSubmitting ? translate("sellerApplySaving") : translate("continue")}
            </button>
          </form>
        ) : null}

        {step === 4 && !showThankYou ? (
          <div className="seller-apply-card">
            <h2>{translate("sellerApplySubmitTitle")}</h2>
            <p className="seller-apply-note">{translate("sellerApplySubmitHint")}</p>
            <button
              type="button"
              className="button button-primary"
              disabled={isSubmitting}
              onClick={() => void handleSubmitApplication()}
            >
              {isSubmitting
                ? translate("sellerApplySaving")
                : translate("sellerApplySubmit")}
            </button>
          </div>
        ) : null}
      </section>

      {showThankYou ? (
        <div
          className="seller-thankyou-overlay"
          role="presentation"
          onClick={(event) => {
            if (event.target === event.currentTarget) {
              window.location.href = "/";
            }
          }}
        >
          <div
            className="seller-thankyou-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="seller-thankyou-title"
          >
            <h2 id="seller-thankyou-title">{translate("sellerApplySuccessTitle")}</h2>
            <p>{translate("sellerApplySuccessBody")}</p>
            <Link href="/" className="button button-primary">
              {translate("backToHome")}
            </Link>
          </div>
        </div>
      ) : null}
    </PublicSiteLayout>
  );
}
