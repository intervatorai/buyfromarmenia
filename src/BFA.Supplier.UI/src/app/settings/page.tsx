"use client";

import { FormEvent, useEffect, useState } from "react";
import { SupplierShell } from "@/components/layout/SupplierShell";
import { ApiError, apiFetch } from "@/lib/api";
import { getSupplierId } from "@/lib/supplier-session";

type SupplierDetail = {
  id: string;
  legalName: string;
  tradingName: string;
  taxNumber?: string | null;
  registrationNumber?: string | null;
  status: string;
  contactPerson: string;
  email: string;
  phone: string;
  legalAddress?: AddressFields | null;
  warehouseAddress?: AddressFields | null;
  preparationDays: number;
};

type AddressFields = {
  countryCode: string;
  city: string;
  line1: string;
  line2?: string | null;
  postalCode?: string | null;
  region?: string | null;
};

type SettingsForm = {
  legalName: string;
  tradingName: string;
  contactPerson: string;
  email: string;
  phone: string;
  taxNumber: string;
  registrationNumber: string;
  legalCountryCode: string;
  legalCity: string;
  legalLine1: string;
  legalLine2: string;
  legalPostalCode: string;
  legalRegion: string;
  warehouseCountryCode: string;
  warehouseCity: string;
  warehouseLine1: string;
  warehouseLine2: string;
  warehousePostalCode: string;
  warehouseRegion: string;
  preparationDays: number;
};

const emptyForm: SettingsForm = {
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
  legalLine2: "",
  legalPostalCode: "",
  legalRegion: "",
  warehouseCountryCode: "AM",
  warehouseCity: "",
  warehouseLine1: "",
  warehouseLine2: "",
  warehousePostalCode: "",
  warehouseRegion: "",
  preparationDays: 2,
};

function mapSupplierToForm(supplier: SupplierDetail): SettingsForm {
  return {
    legalName: supplier.legalName,
    tradingName: supplier.tradingName,
    contactPerson: supplier.contactPerson,
    email: supplier.email,
    phone: supplier.phone,
    taxNumber: supplier.taxNumber ?? "",
    registrationNumber: supplier.registrationNumber ?? "",
    legalCountryCode: supplier.legalAddress?.countryCode ?? "AM",
    legalCity: supplier.legalAddress?.city ?? "",
    legalLine1: supplier.legalAddress?.line1 ?? "",
    legalLine2: supplier.legalAddress?.line2 ?? "",
    legalPostalCode: supplier.legalAddress?.postalCode ?? "",
    legalRegion: supplier.legalAddress?.region ?? "",
    warehouseCountryCode: supplier.warehouseAddress?.countryCode ?? "AM",
    warehouseCity: supplier.warehouseAddress?.city ?? "",
    warehouseLine1: supplier.warehouseAddress?.line1 ?? "",
    warehouseLine2: supplier.warehouseAddress?.line2 ?? "",
    warehousePostalCode: supplier.warehouseAddress?.postalCode ?? "",
    warehouseRegion: supplier.warehouseAddress?.region ?? "",
    preparationDays: supplier.preparationDays,
  };
}

export default function SettingsPage() {
  const [form, setForm] = useState<SettingsForm>(emptyForm);
  const [status, setStatus] = useState("");
  const [error, setError] = useState("");
  const [savedMessage, setSavedMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    async function loadSettings() {
      const supplierId = getSupplierId();
      if (!supplierId) {
        setError("Complete supplier onboarding first.");
        setIsLoading(false);
        return;
      }

      try {
        const supplier = await apiFetch<SupplierDetail>(`/api/suppliers/${supplierId}`);
        setForm(mapSupplierToForm(supplier));
        setStatus(supplier.status);
      } catch (err) {
        setError(err instanceof ApiError ? err.message : "Failed to load settings.");
      } finally {
        setIsLoading(false);
      }
    }

    void loadSettings();
  }, []);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const supplierId = getSupplierId();
    if (!supplierId) {
      setError("Complete supplier onboarding first.");
      return;
    }

    setIsSaving(true);
    setError("");
    setSavedMessage("");

    try {
      await apiFetch(`/api/suppliers/${supplierId}`, {
        method: "PUT",
        body: JSON.stringify({
          legalName: form.legalName,
          tradingName: form.tradingName,
          contactPerson: form.contactPerson,
          email: form.email,
          phone: form.phone,
          taxNumber: form.taxNumber || null,
          registrationNumber: form.registrationNumber || null,
          legalCountryCode: form.legalCountryCode || null,
          legalCity: form.legalCity || null,
          legalLine1: form.legalLine1 || null,
          legalLine2: form.legalLine2 || null,
          legalPostalCode: form.legalPostalCode || null,
          legalRegion: form.legalRegion || null,
          warehouseCountryCode: form.warehouseCountryCode || null,
          warehouseCity: form.warehouseCity || null,
          warehouseLine1: form.warehouseLine1 || null,
          warehouseLine2: form.warehouseLine2 || null,
          warehousePostalCode: form.warehousePostalCode || null,
          warehouseRegion: form.warehouseRegion || null,
          preparationDays: form.preparationDays,
        }),
      });
      setSavedMessage("Settings saved.");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save settings.");
    } finally {
      setIsSaving(false);
    }
  }

  function updateField<K extends keyof SettingsForm>(key: K, value: SettingsForm[K]) {
    setForm((current) => ({ ...current, [key]: value }));
  }

  return (
    <SupplierShell title="Store settings">
      <div className="supplier-card" style={{ maxWidth: 760 }}>
        {error ? <p className="form-error">{error}</p> : null}
        {isLoading ? <p>Loading settings...</p> : null}

        {!isLoading ? (
          <form onSubmit={(event) => void handleSubmit(event)} style={{ display: "grid", gap: 20 }}>
            {status ? (
              <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                <span style={{ fontSize: 13, color: "#64748b" }}>Application status</span>
                <span
                  style={{
                    fontSize: 12,
                    fontWeight: 600,
                    padding: "3px 10px",
                    borderRadius: 999,
                    background: status === "Active" ? "#dcfce7" : "#fef3c7",
                    color: status === "Active" ? "#166534" : "#92400e",
                  }}
                >
                  {status}
                </span>
              </div>
            ) : null}

            <fieldset className="supplier-fieldset">
              <legend>Shop profile</legend>
              <label>
                Legal name
                <input
                  required
                  value={form.legalName}
                  onChange={(event) => updateField("legalName", event.target.value)}
                />
              </label>
              <label>
                Trading name
                <input
                  required
                  value={form.tradingName}
                  onChange={(event) => updateField("tradingName", event.target.value)}
                />
              </label>
              <label>
                Contact person
                <input
                  required
                  value={form.contactPerson}
                  onChange={(event) => updateField("contactPerson", event.target.value)}
                />
              </label>
              <label>
                Email
                <input
                  required
                  type="email"
                  value={form.email}
                  onChange={(event) => updateField("email", event.target.value)}
                />
              </label>
              <label>
                Phone
                <input
                  required
                  value={form.phone}
                  onChange={(event) => updateField("phone", event.target.value)}
                />
              </label>
              <label>
                Default preparation time (days)
                <input
                  required
                  type="number"
                  min={1}
                  value={form.preparationDays}
                  onChange={(event) =>
                    updateField("preparationDays", Number(event.target.value) || 1)
                  }
                />
              </label>
            </fieldset>

            <fieldset className="supplier-fieldset">
              <legend>Legal information</legend>
              <label>
                Tax number
                <input
                  value={form.taxNumber}
                  onChange={(event) => updateField("taxNumber", event.target.value)}
                />
              </label>
              <label>
                Registration number
                <input
                  value={form.registrationNumber}
                  onChange={(event) => updateField("registrationNumber", event.target.value)}
                />
              </label>
              <label>
                Country
                <input
                  maxLength={2}
                  value={form.legalCountryCode}
                  onChange={(event) =>
                    updateField("legalCountryCode", event.target.value.toUpperCase())
                  }
                />
              </label>
              <label>
                City
                <input
                  value={form.legalCity}
                  onChange={(event) => updateField("legalCity", event.target.value)}
                />
              </label>
              <label style={{ gridColumn: "1 / -1" }}>
                Address line 1
                <input
                  value={form.legalLine1}
                  onChange={(event) => updateField("legalLine1", event.target.value)}
                />
              </label>
              <label style={{ gridColumn: "1 / -1" }}>
                Address line 2
                <input
                  value={form.legalLine2}
                  onChange={(event) => updateField("legalLine2", event.target.value)}
                />
              </label>
            </fieldset>

            <fieldset className="supplier-fieldset">
              <legend>Warehouse / pickup address</legend>
              <label>
                Country
                <input
                  maxLength={2}
                  value={form.warehouseCountryCode}
                  onChange={(event) =>
                    updateField("warehouseCountryCode", event.target.value.toUpperCase())
                  }
                />
              </label>
              <label>
                City
                <input
                  value={form.warehouseCity}
                  onChange={(event) => updateField("warehouseCity", event.target.value)}
                />
              </label>
              <label style={{ gridColumn: "1 / -1" }}>
                Address line 1
                <input
                  value={form.warehouseLine1}
                  onChange={(event) => updateField("warehouseLine1", event.target.value)}
                />
              </label>
              <label style={{ gridColumn: "1 / -1" }}>
                Address line 2
                <input
                  value={form.warehouseLine2}
                  onChange={(event) => updateField("warehouseLine2", event.target.value)}
                />
              </label>
            </fieldset>

            <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
              <button
                className="button-primary"
                type="submit"
                disabled={isSaving}
                style={{ padding: "10px 24px" }}
              >
                {isSaving ? "Saving..." : "Save settings"}
              </button>
              {savedMessage ? (
                <span style={{ fontSize: 13, color: "#16a34a" }}>{savedMessage}</span>
              ) : null}
            </div>
          </form>
        ) : null}
      </div>
    </SupplierShell>
  );
}
