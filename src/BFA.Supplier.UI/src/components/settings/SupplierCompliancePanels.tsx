"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { ApiError, apiFetch } from "@/lib/api";

type VendorDocument = {
  id: string;
  documentType: string;
  fileName: string;
  fileUrl: string;
  status: string;
};

type VendorBankAccount = {
  id: string;
  bankName: string;
  accountHolder: string;
  iban: string;
  swift?: string | null;
  currency: string;
  isPrimary: boolean;
};

type SupplierExtras = {
  documents: VendorDocument[];
  bankAccounts: VendorBankAccount[];
};

type DocumentForm = {
  id?: string;
  documentType: string;
  fileName: string;
  fileUrl: string;
};

type BankForm = {
  id?: string;
  bankName: string;
  accountHolder: string;
  iban: string;
  currency: string;
  swift: string;
  isPrimary: boolean;
};

const DOCUMENT_TYPES = [
  "RegistrationCertificate",
  "TaxCertificate",
  "BankStatement",
  "IdentityDocument",
  "Other",
] as const;

const EMPTY_DOCUMENT: DocumentForm = {
  documentType: "RegistrationCertificate",
  fileName: "",
  fileUrl: "",
};

const EMPTY_BANK: BankForm = {
  bankName: "",
  accountHolder: "",
  iban: "",
  currency: "USD",
  swift: "",
  isPrimary: true,
};

export function SupplierCompliancePanels({ supplierId }: { supplierId: string }) {
  const [tab, setTab] = useState<"documents" | "bank">("documents");
  const [data, setData] = useState<SupplierExtras | null>(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [busyId, setBusyId] = useState("");

  const [documentForm, setDocumentForm] = useState<DocumentForm | null>(null);
  const [documentError, setDocumentError] = useState("");
  const [documentSaving, setDocumentSaving] = useState(false);

  const [bankForm, setBankForm] = useState<BankForm | null>(null);
  const [bankError, setBankError] = useState("");
  const [bankSaving, setBankSaving] = useState(false);

  const load = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      const supplier = await apiFetch<SupplierExtras>(`/api/suppliers/${supplierId}`);
      setData({
        documents: supplier.documents ?? [],
        bankAccounts: supplier.bankAccounts ?? [],
      });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load documents and bank accounts.");
      setData(null);
    } finally {
      setIsLoading(false);
    }
  }, [supplierId]);

  useEffect(() => {
    void load();
  }, [load]);

  async function saveDocument(event: FormEvent) {
    event.preventDefault();
    if (!documentForm) {
      return;
    }

    if (!documentForm.fileName.trim() || !documentForm.fileUrl.trim()) {
      setDocumentError("File name and URL are required.");
      return;
    }

    setDocumentSaving(true);
    setDocumentError("");
    const payload = {
      documentType: documentForm.documentType,
      fileName: documentForm.fileName.trim(),
      fileUrl: documentForm.fileUrl.trim(),
    };

    try {
      if (documentForm.id) {
        await apiFetch(`/api/suppliers/${supplierId}/documents/${documentForm.id}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
      } else {
        await apiFetch(`/api/suppliers/${supplierId}/documents`, {
          method: "POST",
          body: JSON.stringify(payload),
        });
      }
      setDocumentForm(null);
      await load();
    } catch (err) {
      setDocumentError(err instanceof ApiError ? err.message : "Failed to save document.");
    } finally {
      setDocumentSaving(false);
    }
  }

  async function deleteDocument(document: VendorDocument) {
    if (!window.confirm(`Delete document “${document.fileName}”?`)) {
      return;
    }

    setBusyId(document.id);
    setError("");
    try {
      await apiFetch(`/api/suppliers/${supplierId}/documents/${document.id}`, {
        method: "DELETE",
      });
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete document.");
    } finally {
      setBusyId("");
    }
  }

  async function saveBank(event: FormEvent) {
    event.preventDefault();
    if (!bankForm) {
      return;
    }

    if (
      !bankForm.bankName.trim()
      || !bankForm.accountHolder.trim()
      || !bankForm.iban.trim()
      || !bankForm.currency.trim()
    ) {
      setBankError("Bank name, holder, IBAN and currency are required.");
      return;
    }

    setBankSaving(true);
    setBankError("");
    const payload = {
      bankName: bankForm.bankName.trim(),
      accountHolder: bankForm.accountHolder.trim(),
      iban: bankForm.iban.trim(),
      currency: bankForm.currency.trim().toUpperCase(),
      swift: bankForm.swift.trim() || null,
      isPrimary: bankForm.isPrimary,
    };

    try {
      if (bankForm.id) {
        await apiFetch(`/api/suppliers/${supplierId}/bank-accounts/${bankForm.id}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
      } else {
        await apiFetch(`/api/suppliers/${supplierId}/bank-accounts`, {
          method: "POST",
          body: JSON.stringify(payload),
        });
      }
      setBankForm(null);
      await load();
    } catch (err) {
      setBankError(err instanceof ApiError ? err.message : "Failed to save bank account.");
    } finally {
      setBankSaving(false);
    }
  }

  async function deleteBank(account: VendorBankAccount) {
    if (!window.confirm(`Delete bank account “${account.iban}”?`)) {
      return;
    }

    setBusyId(account.id);
    setError("");
    try {
      await apiFetch(`/api/suppliers/${supplierId}/bank-accounts/${account.id}`, {
        method: "DELETE",
      });
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete bank account.");
    } finally {
      setBusyId("");
    }
  }

  return (
    <div className="supplier-card">
      <div style={{ display: "flex", gap: 8, flexWrap: "wrap", marginBottom: 16 }}>
        <button
          type="button"
          className={tab === "documents" ? "button-primary" : "button-ghost"}
          onClick={() => setTab("documents")}
        >
          Documents ({data?.documents.length ?? 0})
        </button>
        <button
          type="button"
          className={tab === "bank" ? "button-primary" : "button-ghost"}
          onClick={() => setTab("bank")}
        >
          Bank accounts ({data?.bankAccounts.length ?? 0})
        </button>
      </div>

      {error ? <p className="form-error">{error}</p> : null}
      {isLoading ? <p>Loading…</p> : null}

      {!isLoading && tab === "documents" ? (
        <div style={{ display: "grid", gap: 16 }}>
          <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "center" }}>
            <p style={{ margin: 0, color: "#64748b", fontSize: 14 }}>
              Upload registration and compliance documents for review.
            </p>
            <button
              type="button"
              className="button-primary"
              onClick={() => {
                setDocumentForm(EMPTY_DOCUMENT);
                setDocumentError("");
              }}
            >
              Add document
            </button>
          </div>

          {(data?.documents.length ?? 0) === 0 ? (
            <p style={{ margin: 0, color: "#64748b" }}>No documents yet.</p>
          ) : (
            <div style={{ display: "grid", gap: 10 }}>
              {data?.documents.map((document) => (
                <div
                  key={document.id}
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 12,
                    flexWrap: "wrap",
                    padding: "12px 14px",
                    border: "1px solid #e2e8f0",
                    borderRadius: 12,
                  }}
                >
                  <div>
                    <strong>{document.documentType}</strong>
                    <div style={{ fontSize: 13, marginTop: 4 }}>
                      <a href={document.fileUrl} target="_blank" rel="noreferrer">
                        {document.fileName}
                      </a>
                    </div>
                    <div style={{ fontSize: 12, color: "#64748b", marginTop: 4 }}>
                      Status: {document.status}
                    </div>
                  </div>
                  <div style={{ display: "flex", gap: 8 }}>
                    <button
                      type="button"
                      className="button-ghost"
                      onClick={() => {
                        setDocumentForm({
                          id: document.id,
                          documentType: document.documentType,
                          fileName: document.fileName,
                          fileUrl: document.fileUrl,
                        });
                        setDocumentError("");
                      }}
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      className="button-ghost"
                      disabled={busyId === document.id}
                      onClick={() => void deleteDocument(document)}
                    >
                      Delete
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}

          {documentForm ? (
            <form
              onSubmit={(event) => void saveDocument(event)}
              className="supplier-fieldset"
              style={{ display: "grid", gap: 12, margin: 0 }}
            >
              <legend style={{ fontWeight: 600 }}>
                {documentForm.id ? "Edit document" : "Add document"}
              </legend>
              <label>
                Type
                <select
                  value={documentForm.documentType}
                  onChange={(event) =>
                    setDocumentForm((current) =>
                      current
                        ? { ...current, documentType: event.target.value }
                        : current,
                    )
                  }
                >
                  {DOCUMENT_TYPES.map((type) => (
                    <option key={type} value={type}>
                      {type}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                File name
                <input
                  required
                  value={documentForm.fileName}
                  onChange={(event) =>
                    setDocumentForm((current) =>
                      current ? { ...current, fileName: event.target.value } : current,
                    )
                  }
                />
              </label>
              <label>
                File URL
                <input
                  required
                  value={documentForm.fileUrl}
                  onChange={(event) =>
                    setDocumentForm((current) =>
                      current ? { ...current, fileUrl: event.target.value } : current,
                    )
                  }
                />
              </label>
              {documentError ? <p className="form-error">{documentError}</p> : null}
              <div style={{ display: "flex", gap: 8 }}>
                <button className="button-primary" type="submit" disabled={documentSaving}>
                  {documentSaving ? "Saving…" : "Save document"}
                </button>
                <button
                  type="button"
                  className="button-ghost"
                  onClick={() => setDocumentForm(null)}
                >
                  Cancel
                </button>
              </div>
            </form>
          ) : null}
        </div>
      ) : null}

      {!isLoading && tab === "bank" ? (
        <div style={{ display: "grid", gap: 16 }}>
          <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "center" }}>
            <p style={{ margin: 0, color: "#64748b", fontSize: 14 }}>
              Bank accounts used for payouts.
            </p>
            <button
              type="button"
              className="button-primary"
              onClick={() => {
                setBankForm({
                  ...EMPTY_BANK,
                  isPrimary: !(data?.bankAccounts.some((account) => account.isPrimary) ?? false),
                });
                setBankError("");
              }}
            >
              Add bank account
            </button>
          </div>

          {(data?.bankAccounts.length ?? 0) === 0 ? (
            <p style={{ margin: 0, color: "#64748b" }}>No bank accounts yet.</p>
          ) : (
            <div style={{ display: "grid", gap: 10 }}>
              {data?.bankAccounts.map((account) => (
                <div
                  key={account.id}
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 12,
                    flexWrap: "wrap",
                    padding: "12px 14px",
                    border: "1px solid #e2e8f0",
                    borderRadius: 12,
                  }}
                >
                  <div>
                    <strong>{account.bankName}</strong>
                    {account.isPrimary ? (
                      <span style={{ marginLeft: 8, fontSize: 12, color: "#166534" }}>
                        Primary
                      </span>
                    ) : null}
                    <div style={{ fontSize: 13, marginTop: 4 }}>{account.accountHolder}</div>
                    <div style={{ fontSize: 13, color: "#64748b", marginTop: 2 }}>
                      {account.iban} · {account.currency}
                      {account.swift ? ` · SWIFT ${account.swift}` : ""}
                    </div>
                  </div>
                  <div style={{ display: "flex", gap: 8 }}>
                    <button
                      type="button"
                      className="button-ghost"
                      onClick={() => {
                        setBankForm({
                          id: account.id,
                          bankName: account.bankName,
                          accountHolder: account.accountHolder,
                          iban: account.iban,
                          currency: account.currency,
                          swift: account.swift ?? "",
                          isPrimary: account.isPrimary,
                        });
                        setBankError("");
                      }}
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      className="button-ghost"
                      disabled={busyId === account.id}
                      onClick={() => void deleteBank(account)}
                    >
                      Delete
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}

          {bankForm ? (
            <form
              onSubmit={(event) => void saveBank(event)}
              className="supplier-fieldset"
              style={{ display: "grid", gap: 12, margin: 0 }}
            >
              <legend style={{ fontWeight: 600 }}>
                {bankForm.id ? "Edit bank account" : "Add bank account"}
              </legend>
              <label>
                Bank name
                <input
                  required
                  value={bankForm.bankName}
                  onChange={(event) =>
                    setBankForm((current) =>
                      current ? { ...current, bankName: event.target.value } : current,
                    )
                  }
                />
              </label>
              <label>
                Account holder
                <input
                  required
                  value={bankForm.accountHolder}
                  onChange={(event) =>
                    setBankForm((current) =>
                      current ? { ...current, accountHolder: event.target.value } : current,
                    )
                  }
                />
              </label>
              <label>
                IBAN
                <input
                  required
                  value={bankForm.iban}
                  onChange={(event) =>
                    setBankForm((current) =>
                      current ? { ...current, iban: event.target.value } : current,
                    )
                  }
                />
              </label>
              <label>
                Currency
                <input
                  required
                  maxLength={3}
                  value={bankForm.currency}
                  onChange={(event) =>
                    setBankForm((current) =>
                      current
                        ? { ...current, currency: event.target.value.toUpperCase() }
                        : current,
                    )
                  }
                />
              </label>
              <label>
                SWIFT (optional)
                <input
                  value={bankForm.swift}
                  onChange={(event) =>
                    setBankForm((current) =>
                      current ? { ...current, swift: event.target.value } : current,
                    )
                  }
                />
              </label>
              <label style={{ display: "flex", gap: 8, alignItems: "center" }}>
                <input
                  type="checkbox"
                  checked={bankForm.isPrimary}
                  onChange={(event) =>
                    setBankForm((current) =>
                      current ? { ...current, isPrimary: event.target.checked } : current,
                    )
                  }
                />
                Primary account
              </label>
              {bankError ? <p className="form-error">{bankError}</p> : null}
              <div style={{ display: "flex", gap: 8 }}>
                <button className="button-primary" type="submit" disabled={bankSaving}>
                  {bankSaving ? "Saving…" : "Save bank account"}
                </button>
                <button type="button" className="button-ghost" onClick={() => setBankForm(null)}>
                  Cancel
                </button>
              </div>
            </form>
          ) : null}
        </div>
      ) : null}
    </div>
  );
}
