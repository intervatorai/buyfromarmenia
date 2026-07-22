"use client";

import { FormEvent, useEffect, useState } from "react";
import { ApiError, apiFetch } from "@/lib/api";
import { Modal } from "./Modal";

type VendorSeed = {
  id: string;
  legalName: string;
  tradingName: string;
  contactPerson: string;
  email: string;
  phone: string;
  taxNumber?: string | null;
  registrationNumber?: string | null;
};

type VendorFormModalProps = {
  open: boolean;
  vendorId?: string | null;
  onClose: () => void;
  onSaved: (vendorId: string) => void;
};

export function VendorFormModal({ open, vendorId, onClose, onSaved }: VendorFormModalProps) {
  const isEdit = Boolean(vendorId);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const [legalName, setLegalName] = useState("");
  const [tradingName, setTradingName] = useState("");
  const [contactPerson, setContactPerson] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [taxNumber, setTaxNumber] = useState("");
  const [registrationNumber, setRegistrationNumber] = useState("");
  const [activateImmediately, setActivateImmediately] = useState(true);

  useEffect(() => {
    if (!open) {
      return;
    }

    async function load() {
      setError("");
      if (!vendorId) {
        setLegalName("");
        setTradingName("");
        setContactPerson("");
        setEmail("");
        setPhone("");
        setTaxNumber("");
        setRegistrationNumber("");
        setActivateImmediately(true);
        setIsLoading(false);
        return;
      }

      setIsLoading(true);
      try {
        const vendor = await apiFetch<VendorSeed>(`/api/suppliers/${vendorId}`);
        setLegalName(vendor.legalName);
        setTradingName(vendor.tradingName);
        setContactPerson(vendor.contactPerson);
        setEmail(vendor.email);
        setPhone(vendor.phone);
        setTaxNumber(vendor.taxNumber ?? "");
        setRegistrationNumber(vendor.registrationNumber ?? "");
      } catch (err) {
        setError(err instanceof ApiError ? err.message : "Failed to load vendor.");
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [open, vendorId]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setIsSaving(true);
    setError("");

    try {
      if (isEdit && vendorId) {
        await apiFetch(`/api/suppliers/${vendorId}`, {
          method: "PUT",
          body: JSON.stringify({
            legalName,
            tradingName,
            contactPerson,
            email,
            phone,
            taxNumber: taxNumber || null,
            registrationNumber: registrationNumber || null,
          }),
        });
        onSaved(vendorId);
      } else {
        const result = await apiFetch<{ id: string }>("/api/suppliers", {
          method: "POST",
          body: JSON.stringify({
            legalName,
            tradingName,
            contactPerson,
            email,
            phone,
            taxNumber: taxNumber || null,
            activateImmediately,
          }),
        });
        onSaved(result.id);
      }
      onClose();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save vendor.");
      setIsSaving(false);
    }
  }

  return (
    <Modal
      open={open}
      title={isEdit ? "Edit vendor" : "Add vendor"}
      onClose={onClose}
      footer={
        <>
          <button type="button" className="button-ghost" onClick={onClose} disabled={isSaving}>
            Cancel
          </button>
          <button
            type="submit"
            form="vendor-form-modal"
            className="button-primary"
            disabled={isSaving || isLoading}
          >
            {isSaving ? "Saving..." : isEdit ? "Save changes" : "Create vendor"}
          </button>
        </>
      }
    >
      {isLoading ? <p>Loading...</p> : null}
      {error ? <p className="form-error">{error}</p> : null}

      {!isLoading ? (
        <form id="vendor-form-modal" onSubmit={(event) => void handleSubmit(event)}>
          <div className="form-field">
            <label htmlFor="vendor-legalName">Legal name</label>
            <input
              id="vendor-legalName"
              required
              value={legalName}
              onChange={(e) => setLegalName(e.target.value)}
            />
          </div>
          <div className="form-field">
            <label htmlFor="vendor-tradingName">Trading name</label>
            <input
              id="vendor-tradingName"
              required
              value={tradingName}
              onChange={(e) => setTradingName(e.target.value)}
            />
          </div>
          <div className="form-field">
            <label htmlFor="vendor-contactPerson">Contact person</label>
            <input
              id="vendor-contactPerson"
              required
              value={contactPerson}
              onChange={(e) => setContactPerson(e.target.value)}
            />
          </div>
          <div className="form-row-2">
            <div className="form-field">
              <label htmlFor="vendor-email">Email</label>
              <input
                id="vendor-email"
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>
            <div className="form-field">
              <label htmlFor="vendor-phone">Phone</label>
              <input
                id="vendor-phone"
                required
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
              />
            </div>
          </div>
          <div className="form-field">
            <label htmlFor="vendor-taxNumber">Tax number</label>
            <input
              id="vendor-taxNumber"
              value={taxNumber}
              onChange={(e) => setTaxNumber(e.target.value)}
            />
          </div>
          {isEdit ? (
            <div className="form-field">
              <label htmlFor="vendor-registrationNumber">Registration number</label>
              <input
                id="vendor-registrationNumber"
                value={registrationNumber}
                onChange={(e) => setRegistrationNumber(e.target.value)}
              />
            </div>
          ) : (
            <label className="form-checkbox">
              <input
                type="checkbox"
                checked={activateImmediately}
                onChange={(e) => setActivateImmediately(e.target.checked)}
              />
              Activate immediately (skip onboarding queue)
            </label>
          )}
        </form>
      ) : null}
    </Modal>
  );
}
