"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { ApiError, apiFetch } from "@/lib/api";
import { Modal } from "./Modal";

export type VendorSubpanelTab = "documents" | "bank";

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

type VendorExtras = {
  id: string;
  documents: VendorDocument[];
  bankAccounts: VendorBankAccount[];
};

type DocumentFormState = {
  id?: string;
  documentType: string;
  fileName: string;
  fileUrl: string;
};

type BankFormState = {
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

const EMPTY_DOCUMENT_FORM: DocumentFormState = {
  documentType: "RegistrationCertificate",
  fileName: "",
  fileUrl: "",
};

const EMPTY_BANK_FORM: BankFormState = {
  bankName: "",
  accountHolder: "",
  iban: "",
  currency: "USD",
  swift: "",
  isPrimary: true,
};

const TABS: Array<{ id: VendorSubpanelTab; label: string }> = [
  { id: "documents", label: "Documents" },
  { id: "bank", label: "Bank accounts" },
];

type VendorSubpanelsProps = {
  vendorId: string;
  collapsible?: boolean;
  defaultOpen?: boolean;
  defaultTab?: VendorSubpanelTab;
  onChanged?: () => void;
};

export function VendorSubpanels({
  vendorId,
  collapsible = true,
  defaultOpen = false,
  defaultTab = "documents",
  onChanged,
}: VendorSubpanelsProps) {
  const [open, setOpen] = useState(defaultOpen || !collapsible);
  const [tab, setTab] = useState<VendorSubpanelTab>(defaultTab);
  const [data, setData] = useState<VendorExtras | null>(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [busyAction, setBusyAction] = useState("");

  const [documentFormOpen, setDocumentFormOpen] = useState(false);
  const [documentForm, setDocumentForm] = useState<DocumentFormState>(EMPTY_DOCUMENT_FORM);
  const [documentSaving, setDocumentSaving] = useState(false);
  const [documentError, setDocumentError] = useState("");

  const [bankFormOpen, setBankFormOpen] = useState(false);
  const [bankForm, setBankForm] = useState<BankFormState>(EMPTY_BANK_FORM);
  const [bankSaving, setBankSaving] = useState(false);
  const [bankError, setBankError] = useState("");

  const load = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      const vendor = await apiFetch<VendorExtras>(`/api/suppliers/${vendorId}`);
      setData(vendor);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load vendor details.");
      setData(null);
    } finally {
      setIsLoading(false);
    }
  }, [vendorId]);

  useEffect(() => {
    if (open) {
      void load();
    }
  }, [open, load]);

  function notifyChanged() {
    onChanged?.();
  }

  function openAddDocument() {
    setDocumentForm(EMPTY_DOCUMENT_FORM);
    setDocumentError("");
    setDocumentFormOpen(true);
  }

  function openEditDocument(document: VendorDocument) {
    setDocumentForm({
      id: document.id,
      documentType: document.documentType,
      fileName: document.fileName,
      fileUrl: document.fileUrl,
    });
    setDocumentError("");
    setDocumentFormOpen(true);
  }

  async function saveDocument(event: FormEvent) {
    event.preventDefault();
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
        await apiFetch(`/api/suppliers/${vendorId}/documents/${documentForm.id}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
      } else {
        await apiFetch(`/api/suppliers/${vendorId}/documents`, {
          method: "POST",
          body: JSON.stringify(payload),
        });
      }
      setDocumentFormOpen(false);
      await load();
      notifyChanged();
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

    setBusyAction(document.id);
    setError("");
    try {
      await apiFetch(`/api/suppliers/${vendorId}/documents/${document.id}`, {
        method: "DELETE",
      });
      await load();
      notifyChanged();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete document.");
    } finally {
      setBusyAction("");
    }
  }

  function openAddBank() {
    setBankForm({
      ...EMPTY_BANK_FORM,
      isPrimary: !(data?.bankAccounts.some((account) => account.isPrimary) ?? false),
    });
    setBankError("");
    setBankFormOpen(true);
  }

  function openEditBank(account: VendorBankAccount) {
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
    setBankFormOpen(true);
  }

  async function saveBank(event: FormEvent) {
    event.preventDefault();
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
        await apiFetch(`/api/suppliers/${vendorId}/bank-accounts/${bankForm.id}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
      } else {
        await apiFetch(`/api/suppliers/${vendorId}/bank-accounts`, {
          method: "POST",
          body: JSON.stringify(payload),
        });
      }
      setBankFormOpen(false);
      await load();
      notifyChanged();
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

    setBusyAction(account.id);
    setError("");
    try {
      await apiFetch(`/api/suppliers/${vendorId}/bank-accounts/${account.id}`, {
        method: "DELETE",
      });
      await load();
      notifyChanged();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete bank account.");
    } finally {
      setBusyAction("");
    }
  }

  return (
    <div className="product-subpanels">
      {collapsible ? (
        <button
          type="button"
          className="product-subpanels-toggle"
          onClick={() => setOpen((current) => !current)}
        >
          <span className="product-subpanels-chevron">{open ? "▾" : "▸"}</span>
          Documents &amp; bank
          <span className="product-subpanels-meta">
            {data
              ? `${data.documents.length} docs · ${data.bankAccounts.length} bank`
              : "…"}
          </span>
        </button>
      ) : null}

      {open ? (
        <div className="product-subpanels-body">
          <div className="product-subpanels-toolbar">
            <div className="detail-tabs" role="tablist" aria-label="Vendor sections">
              {TABS.map((item) => (
                <button
                  key={item.id}
                  type="button"
                  role="tab"
                  aria-selected={tab === item.id}
                  className={`detail-tab${tab === item.id ? " active" : ""}`}
                  onClick={() => setTab(item.id)}
                >
                  {item.label}
                  {data
                    ? ` (${item.id === "documents" ? data.documents.length : data.bankAccounts.length})`
                    : ""}
                </button>
              ))}
            </div>
            {tab === "documents" ? (
              <button type="button" className="button-primary" onClick={openAddDocument}>
                Add document
              </button>
            ) : (
              <button type="button" className="button-primary" onClick={openAddBank}>
                Add bank account
              </button>
            )}
          </div>

          {error ? <p className="form-error">{error}</p> : null}
          {isLoading ? <p>Loading…</p> : null}

          {!isLoading && tab === "documents" ? (
            <div className="admin-table-wrap">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Type</th>
                    <th>File</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {(data?.documents.length ?? 0) === 0 ? (
                    <tr>
                      <td colSpan={4}>No documents yet.</td>
                    </tr>
                  ) : (
                    data?.documents.map((document) => (
                      <tr key={document.id}>
                        <td>{document.documentType}</td>
                        <td>
                          <a href={document.fileUrl} target="_blank" rel="noreferrer">
                            {document.fileName}
                          </a>
                        </td>
                        <td>{document.status}</td>
                        <td style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                          <button
                            type="button"
                            className="button-ghost"
                            onClick={() => openEditDocument(document)}
                          >
                            Edit
                          </button>
                          <button
                            type="button"
                            className="button-ghost"
                            disabled={busyAction === document.id}
                            onClick={() => void deleteDocument(document)}
                          >
                            Delete
                          </button>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          ) : null}

          {!isLoading && tab === "bank" ? (
            <div className="admin-table-wrap">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Bank</th>
                    <th>Holder</th>
                    <th>IBAN</th>
                    <th>Currency</th>
                    <th>Primary</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {(data?.bankAccounts.length ?? 0) === 0 ? (
                    <tr>
                      <td colSpan={6}>No bank accounts yet.</td>
                    </tr>
                  ) : (
                    data?.bankAccounts.map((account) => (
                      <tr key={account.id}>
                        <td>{account.bankName}</td>
                        <td>{account.accountHolder}</td>
                        <td>
                          {account.iban}
                          {account.swift ? (
                            <>
                              <br />
                              <span style={{ color: "var(--admin-muted)", fontSize: 12 }}>
                                SWIFT {account.swift}
                              </span>
                            </>
                          ) : null}
                        </td>
                        <td>{account.currency}</td>
                        <td>{account.isPrimary ? "Yes" : "No"}</td>
                        <td style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                          <button
                            type="button"
                            className="button-ghost"
                            onClick={() => openEditBank(account)}
                          >
                            Edit
                          </button>
                          <button
                            type="button"
                            className="button-ghost"
                            disabled={busyAction === account.id}
                            onClick={() => void deleteBank(account)}
                          >
                            Delete
                          </button>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          ) : null}
        </div>
      ) : null}

      <Modal
        open={documentFormOpen}
        title={documentForm.id ? "Edit document" : "Add document"}
        onClose={() => setDocumentFormOpen(false)}
        footer={
          <>
            <button
              type="button"
              className="button-ghost"
              onClick={() => setDocumentFormOpen(false)}
            >
              Cancel
            </button>
            <button
              type="submit"
              form="vendor-document-form"
              className="button-primary"
              disabled={documentSaving}
            >
              {documentSaving ? "Saving…" : "Save"}
            </button>
          </>
        }
      >
        <form id="vendor-document-form" className="form-field" onSubmit={(e) => void saveDocument(e)}>
          <label>
            Type
            <select
              value={documentForm.documentType}
              onChange={(event) =>
                setDocumentForm((current) => ({
                  ...current,
                  documentType: event.target.value,
                }))
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
                setDocumentForm((current) => ({ ...current, fileName: event.target.value }))
              }
            />
          </label>
          <label>
            File URL
            <input
              required
              value={documentForm.fileUrl}
              onChange={(event) =>
                setDocumentForm((current) => ({ ...current, fileUrl: event.target.value }))
              }
            />
          </label>
          {documentError ? <p className="form-error">{documentError}</p> : null}
        </form>
      </Modal>

      <Modal
        open={bankFormOpen}
        title={bankForm.id ? "Edit bank account" : "Add bank account"}
        onClose={() => setBankFormOpen(false)}
        footer={
          <>
            <button type="button" className="button-ghost" onClick={() => setBankFormOpen(false)}>
              Cancel
            </button>
            <button
              type="submit"
              form="vendor-bank-form"
              className="button-primary"
              disabled={bankSaving}
            >
              {bankSaving ? "Saving…" : "Save"}
            </button>
          </>
        }
      >
        <form id="vendor-bank-form" className="form-field" onSubmit={(e) => void saveBank(e)}>
          <label>
            Bank name
            <input
              required
              value={bankForm.bankName}
              onChange={(event) =>
                setBankForm((current) => ({ ...current, bankName: event.target.value }))
              }
            />
          </label>
          <label>
            Account holder
            <input
              required
              value={bankForm.accountHolder}
              onChange={(event) =>
                setBankForm((current) => ({ ...current, accountHolder: event.target.value }))
              }
            />
          </label>
          <label>
            IBAN
            <input
              required
              value={bankForm.iban}
              onChange={(event) =>
                setBankForm((current) => ({ ...current, iban: event.target.value }))
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
                setBankForm((current) => ({
                  ...current,
                  currency: event.target.value.toUpperCase(),
                }))
              }
            />
          </label>
          <label>
            SWIFT (optional)
            <input
              value={bankForm.swift}
              onChange={(event) =>
                setBankForm((current) => ({ ...current, swift: event.target.value }))
              }
            />
          </label>
          <label style={{ display: "flex", gap: 8, alignItems: "center" }}>
            <input
              type="checkbox"
              checked={bankForm.isPrimary}
              onChange={(event) =>
                setBankForm((current) => ({ ...current, isPrimary: event.target.checked }))
              }
            />
            Primary account
          </label>
          {bankError ? <p className="form-error">{bankError}</p> : null}
        </form>
      </Modal>
    </div>
  );
}
