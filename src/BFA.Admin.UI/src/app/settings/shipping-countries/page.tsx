"use client";

import { FormEvent, useEffect, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { Modal } from "@/components/ui/Modal";
import { ApiError, apiFetch } from "@/lib/api";

type ShippingCountry = {
  id: string;
  isoCode: string;
  nameEn: string;
  nameHy: string;
  isEnabled: boolean;
  sortOrder: number;
  createdAtUtc: string;
  updatedAtUtc: string;
};

type CountryForm = {
  isoCode: string;
  nameEn: string;
  nameHy: string;
  sortOrder: string;
  isEnabled: boolean;
};

type SeedResult = {
  added: number;
  skipped: number;
};

const EMPTY_FORM: CountryForm = {
  isoCode: "",
  nameEn: "",
  nameHy: "",
  sortOrder: "0",
  isEnabled: true,
};

export default function ShippingCountriesPage() {
  const [countries, setCountries] = useState<ShippingCountry[]>([]);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<ShippingCountry | null>(null);
  const [form, setForm] = useState<CountryForm>(EMPTY_FORM);
  const [formError, setFormError] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [isSeeding, setIsSeeding] = useState(false);
  const [togglingId, setTogglingId] = useState<string | null>(null);

  async function loadCountries() {
    try {
      const data = await apiFetch<ShippingCountry[]>("/api/shipping-countries");
      setCountries(data);
      setError("");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load shipping countries.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadCountries();
  }, []);

  function openCreate() {
    setEditing(null);
    setForm(EMPTY_FORM);
    setFormError("");
    setModalOpen(true);
  }

  function openEdit(country: ShippingCountry) {
    setEditing(country);
    setForm({
      isoCode: country.isoCode,
      nameEn: country.nameEn,
      nameHy: country.nameHy,
      sortOrder: String(country.sortOrder),
      isEnabled: country.isEnabled,
    });
    setFormError("");
    setModalOpen(true);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSaving(true);
    setFormError("");

    const sortOrder = Number.parseInt(form.sortOrder, 10);
    if (Number.isNaN(sortOrder)) {
      setFormError("Sort order must be a number.");
      setIsSaving(false);
      return;
    }

    try {
      if (editing) {
        await apiFetch(`/api/shipping-countries/${editing.id}`, {
          method: "PUT",
          body: JSON.stringify({
            nameEn: form.nameEn,
            nameHy: form.nameHy,
            sortOrder,
          }),
        });
      } else {
        await apiFetch("/api/shipping-countries", {
          method: "POST",
          body: JSON.stringify({
            isoCode: form.isoCode,
            nameEn: form.nameEn,
            nameHy: form.nameHy,
            sortOrder,
            isEnabled: form.isEnabled,
          }),
        });
      }

      setModalOpen(false);
      setEditing(null);
      setForm(EMPTY_FORM);
      setMessage(editing ? "Country updated." : "Country added.");
      await loadCountries();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to save shipping country.");
    } finally {
      setIsSaving(false);
    }
  }

  async function toggleEnabled(country: ShippingCountry) {
    setError("");
    setMessage("");
    setTogglingId(country.id);
    try {
      await apiFetch(
        `/api/shipping-countries/${country.id}/${country.isEnabled ? "disable" : "enable"}`,
        { method: "POST" },
      );
      setMessage(
        country.isEnabled
          ? `${country.nameEn} disabled for customer addresses.`
          : `${country.nameEn} enabled for customer addresses.`,
      );
      await loadCountries();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update country status.");
    } finally {
      setTogglingId(null);
    }
  }

  async function seedDefaults() {
    setError("");
    setMessage("");
    setIsSeeding(true);
    try {
      const result = await apiFetch<SeedResult>("/api/shipping-countries/seed-defaults", {
        method: "POST",
      });
      setMessage(
        result.added === 0
          ? `All default countries already exist (${result.skipped} skipped).`
          : `Added ${result.added} countries, skipped ${result.skipped} duplicates. New countries (except Armenia) are disabled — enable the ones you ship to.`,
      );
      await loadCountries();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to add default countries.");
    } finally {
      setIsSeeding(false);
    }
  }

  const enabledCount = countries.filter((country) => country.isEnabled).length;

  return (
    <AdminShell title="Shipping countries">
      <p style={{ margin: "0 0 16px", color: "#64748b", maxWidth: 720, lineHeight: 1.5 }}>
        Manage which countries customers can choose for delivery addresses. Use
        &quot;Add default list&quot; to import a starter catalog without duplicates, then
        enable only the destinations you support.
      </p>

      <div
        style={{
          marginBottom: 16,
          display: "flex",
          flexWrap: "wrap",
          gap: 12,
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <div style={{ color: "#64748b", fontSize: 14 }}>
          {isLoading
            ? null
            : `${countries.length} countries · ${enabledCount} enabled for checkout`}
        </div>
        <div style={{ display: "flex", flexWrap: "wrap", gap: 10 }}>
          <button
            type="button"
            className="button-secondary"
            disabled={isSeeding || isLoading}
            onClick={() => void seedDefaults()}
          >
            {isSeeding ? "Adding defaults..." : "Add default list"}
          </button>
          <button type="button" className="button-primary" onClick={openCreate}>
            Add country
          </button>
        </div>
      </div>

      {error ? <p className="form-error">{error}</p> : null}
      {message ? (
        <p style={{ marginBottom: 12, color: "#166534", fontSize: 14 }}>{message}</p>
      ) : null}
      {isLoading ? <p>Loading shipping countries...</p> : null}

      {!isLoading ? (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Code</th>
                <th>Name (EN)</th>
                <th>Name (HY)</th>
                <th>Sort</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {countries.length === 0 ? (
                <tr>
                  <td colSpan={6} style={{ textAlign: "center", color: "#94a3b8" }}>
                    No shipping countries yet. Click &quot;Add default list&quot; or add one manually.
                  </td>
                </tr>
              ) : (
                countries.map((country) => (
                  <tr key={country.id}>
                    <td>
                      <strong>{country.isoCode}</strong>
                    </td>
                    <td>{country.nameEn}</td>
                    <td>{country.nameHy}</td>
                    <td>{country.sortOrder}</td>
                    <td>
                      <span
                        style={{
                          display: "inline-block",
                          padding: "2px 8px",
                          borderRadius: 999,
                          fontSize: 12,
                          fontWeight: 600,
                          background: country.isEnabled ? "#dcfce7" : "#f1f5f9",
                          color: country.isEnabled ? "#166534" : "#64748b",
                        }}
                      >
                        {country.isEnabled ? "Enabled" : "Disabled"}
                      </span>
                    </td>
                    <td style={{ whiteSpace: "nowrap" }}>
                      <button type="button" className="button-ghost" onClick={() => openEdit(country)}>
                        Edit
                      </button>
                      <button
                        type="button"
                        className="button-ghost"
                        disabled={togglingId === country.id}
                        onClick={() => void toggleEnabled(country)}
                      >
                        {togglingId === country.id
                          ? "Updating..."
                          : country.isEnabled
                            ? "Disable"
                            : "Enable"}
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      ) : null}

      <Modal
        open={modalOpen}
        title={editing ? "Edit shipping country" : "Add shipping country"}
        onClose={() => setModalOpen(false)}
      >
        <form onSubmit={(event) => void handleSubmit(event)}>
          <label>
            ISO code
            <input
              required
              minLength={2}
              maxLength={2}
              value={form.isoCode}
              disabled={Boolean(editing)}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  isoCode: event.target.value.toUpperCase(),
                }))
              }
            />
          </label>
          <label>
            Name (English)
            <input
              required
              value={form.nameEn}
              onChange={(event) => setForm((current) => ({ ...current, nameEn: event.target.value }))}
            />
          </label>
          <label>
            Name (Armenian)
            <input
              required
              value={form.nameHy}
              onChange={(event) => setForm((current) => ({ ...current, nameHy: event.target.value }))}
            />
          </label>
          <label>
            Sort order
            <input
              required
              type="number"
              value={form.sortOrder}
              onChange={(event) =>
                setForm((current) => ({ ...current, sortOrder: event.target.value }))
              }
            />
          </label>
          {!editing ? (
            <label style={{ display: "flex", alignItems: "center", gap: 8 }}>
              <input
                type="checkbox"
                checked={form.isEnabled}
                onChange={(event) =>
                  setForm((current) => ({ ...current, isEnabled: event.target.checked }))
                }
              />
              Enabled for customer addresses
            </label>
          ) : null}

          {formError ? <p className="form-error">{formError}</p> : null}

          <div style={{ display: "flex", gap: 12, marginTop: 16 }}>
            <button type="submit" className="button-primary" disabled={isSaving}>
              {isSaving ? "Saving..." : editing ? "Save changes" : "Add country"}
            </button>
            <button type="button" className="button-ghost" onClick={() => setModalOpen(false)}>
              Cancel
            </button>
          </div>
        </form>
      </Modal>
    </AdminShell>
  );
}
