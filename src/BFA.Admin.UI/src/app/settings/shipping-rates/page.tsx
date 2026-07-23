"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { AdminShell } from "@/components/layout/AdminShell";
import { Modal } from "@/components/ui/Modal";
import { ApiError, apiFetch } from "@/lib/api";

type ShippingCountry = {
  id: string;
  isoCode: string;
  nameEn: string;
  isEnabled: boolean;
};

type RateBracket = {
  id: string;
  countryIsoCode: string;
  weightFromKg: number;
  weightToKg: number;
  price: number;
  currency: string;
  isActive: boolean;
};

type PricingSettings = {
  errorMarginPercent: number;
  updatedAtUtc: string;
};

type BracketForm = {
  countryIsoCode: string;
  weightFromKg: string;
  weightToKg: string;
  price: string;
  currency: string;
  isActive: boolean;
};

const EMPTY_FORM: BracketForm = {
  countryIsoCode: "US",
  weightFromKg: "0",
  weightToKg: "1",
  price: "15",
  currency: "USD",
  isActive: true,
};

export default function ShippingRatesPage() {
  const [countries, setCountries] = useState<ShippingCountry[]>([]);
  const [brackets, setBrackets] = useState<RateBracket[]>([]);
  const [settings, setSettings] = useState<PricingSettings | null>(null);
  const [countryFilter, setCountryFilter] = useState("");
  const [marginInput, setMarginInput] = useState("10");
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<RateBracket | null>(null);
  const [form, setForm] = useState<BracketForm>(EMPTY_FORM);
  const [formError, setFormError] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  const filteredBrackets = useMemo(
    () =>
      countryFilter
        ? brackets.filter((bracket) => bracket.countryIsoCode === countryFilter)
        : brackets,
    [brackets, countryFilter],
  );

  async function load() {
    try {
      const [countryData, bracketData, settingsData] = await Promise.all([
        apiFetch<ShippingCountry[]>("/api/shipping-countries"),
        apiFetch<RateBracket[]>("/api/shipping-rates"),
        apiFetch<PricingSettings>("/api/shipping-rates/settings"),
      ]);
      setCountries(countryData);
      setBrackets(bracketData);
      setSettings(settingsData);
      setMarginInput(String(settingsData.errorMarginPercent));
      setError("");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load shipping rates.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  function openCreate() {
    setEditing(null);
    setForm({
      ...EMPTY_FORM,
      countryIsoCode: countryFilter || countries[0]?.isoCode || "US",
    });
    setFormError("");
    setModalOpen(true);
  }

  function openEdit(bracket: RateBracket) {
    setEditing(bracket);
    setForm({
      countryIsoCode: bracket.countryIsoCode,
      weightFromKg: String(bracket.weightFromKg),
      weightToKg: String(bracket.weightToKg),
      price: String(bracket.price),
      currency: bracket.currency,
      isActive: bracket.isActive,
    });
    setFormError("");
    setModalOpen(true);
  }

  async function saveMargin(event: FormEvent) {
    event.preventDefault();
    setMessage("");
    setError("");
    const value = Number(marginInput);
    if (Number.isNaN(value) || value < 0) {
      setError("Error margin must be a non-negative number.");
      return;
    }

    try {
      const updated = await apiFetch<PricingSettings>("/api/shipping-rates/settings", {
        method: "PUT",
        body: JSON.stringify({ errorMarginPercent: value }),
      });
      setSettings(updated);
      setMessage("Error margin saved.");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save margin.");
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSaving(true);
    setFormError("");

    const weightFromKg = Number(form.weightFromKg);
    const weightToKg = Number(form.weightToKg);
    const price = Number(form.price);
    if ([weightFromKg, weightToKg, price].some((value) => Number.isNaN(value))) {
      setFormError("Weight and price must be numbers.");
      setIsSaving(false);
      return;
    }

    try {
      if (editing) {
        await apiFetch(`/api/shipping-rates/${editing.id}`, {
          method: "PUT",
          body: JSON.stringify({
            weightFromKg,
            weightToKg,
            price,
            currency: form.currency,
            isActive: form.isActive,
          }),
        });
      } else {
        await apiFetch("/api/shipping-rates", {
          method: "POST",
          body: JSON.stringify({
            countryIsoCode: form.countryIsoCode,
            weightFromKg,
            weightToKg,
            price,
            currency: form.currency,
            isActive: form.isActive,
          }),
        });
      }
      setModalOpen(false);
      setMessage(editing ? "Rate bracket updated." : "Rate bracket created.");
      await load();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to save bracket.");
    } finally {
      setIsSaving(false);
    }
  }

  async function deleteBracket(id: string) {
    if (!window.confirm("Delete this shipping rate bracket?")) {
      return;
    }

    try {
      await apiFetch(`/api/shipping-rates/${id}`, { method: "DELETE" });
      setMessage("Rate bracket deleted.");
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete bracket.");
    }
  }

  return (
    <AdminShell title="Shipping rates">
      <p style={{ color: "var(--admin-muted)", marginBottom: 16 }}>
        Configure delivery price by destination country and weight. Checkout adds the
        error margin percent on top of the base rate.
      </p>

      {error ? <p className="form-error">{error}</p> : null}
      {message ? <p className="form-success">{message}</p> : null}
      {isLoading ? <p>Loading...</p> : null}

      <form
        onSubmit={(event) => void saveMargin(event)}
        className="admin-card"
        style={{ marginBottom: 24, display: "flex", gap: 12, alignItems: "end", flexWrap: "wrap" }}
      >
        <label>
          Error margin %
          <input
            value={marginInput}
            onChange={(event) => setMarginInput(event.target.value)}
            type="number"
            min={0}
            max={100}
            step="0.1"
          />
        </label>
        <button type="submit" className="button-primary">
          Save margin
        </button>
        {settings ? (
          <span style={{ color: "var(--admin-muted)", fontSize: 13 }}>
            Current: {settings.errorMarginPercent}%
          </span>
        ) : null}
      </form>

      <div style={{ display: "flex", gap: 12, marginBottom: 16, flexWrap: "wrap" }}>
        <select
          value={countryFilter}
          onChange={(event) => setCountryFilter(event.target.value)}
        >
          <option value="">All countries</option>
          {countries.map((country) => (
            <option key={country.id} value={country.isoCode}>
              {country.isoCode} — {country.nameEn}
            </option>
          ))}
        </select>
        <button type="button" className="button-primary" onClick={openCreate}>
          Add rate bracket
        </button>
      </div>

      <div className="admin-table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>Country</th>
              <th>Weight from (kg)</th>
              <th>Weight to (kg)</th>
              <th>Price</th>
              <th>Active</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {filteredBrackets.map((bracket) => (
              <tr key={bracket.id}>
                <td>{bracket.countryIsoCode}</td>
                <td>{bracket.weightFromKg}</td>
                <td>{bracket.weightToKg}</td>
                <td>
                  {bracket.price.toFixed(2)} {bracket.currency}
                </td>
                <td>{bracket.isActive ? "Yes" : "No"}</td>
                <td>
                  <button type="button" className="button-ghost" onClick={() => openEdit(bracket)}>
                    Edit
                  </button>{" "}
                  <button
                    type="button"
                    className="button-ghost"
                    onClick={() => void deleteBracket(bracket.id)}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <Modal
        open={modalOpen}
        title={editing ? "Edit rate bracket" : "Add rate bracket"}
        onClose={() => setModalOpen(false)}
      >
        <form className="admin-form" onSubmit={(event) => void handleSubmit(event)}>
          <label>
            Country
            <select
              required
              disabled={Boolean(editing)}
              value={form.countryIsoCode}
              onChange={(event) =>
                setForm((current) => ({ ...current, countryIsoCode: event.target.value }))
              }
            >
              {countries.map((country) => (
                <option key={country.id} value={country.isoCode}>
                  {country.isoCode} — {country.nameEn}
                </option>
              ))}
            </select>
          </label>
          <label>
            Weight from (kg)
            <input
              required
              type="number"
              min={0}
              step="0.001"
              value={form.weightFromKg}
              onChange={(event) =>
                setForm((current) => ({ ...current, weightFromKg: event.target.value }))
              }
            />
          </label>
          <label>
            Weight to (kg)
            <input
              required
              type="number"
              min={0}
              step="0.001"
              value={form.weightToKg}
              onChange={(event) =>
                setForm((current) => ({ ...current, weightToKg: event.target.value }))
              }
            />
          </label>
          <label>
            Price
            <input
              required
              type="number"
              min={0}
              step="0.01"
              value={form.price}
              onChange={(event) =>
                setForm((current) => ({ ...current, price: event.target.value }))
              }
            />
          </label>
          <label>
            Currency
            <input
              required
              maxLength={3}
              value={form.currency}
              onChange={(event) =>
                setForm((current) => ({ ...current, currency: event.target.value }))
              }
            />
          </label>
          <label className="admin-checkbox">
            <input
              type="checkbox"
              checked={form.isActive}
              onChange={(event) =>
                setForm((current) => ({ ...current, isActive: event.target.checked }))
              }
            />
            Active
          </label>
          {formError ? <p className="form-error">{formError}</p> : null}
          <button type="submit" className="button-primary" disabled={isSaving}>
            {isSaving ? "Saving…" : "Save"}
          </button>
        </form>
      </Modal>
    </AdminShell>
  );
}
