"use client";

import Link from "next/link";
import { useCallback, useEffect, useState, type FormEvent } from "react";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useAuth } from "@/components/providers/AuthProvider";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { RequireAuth } from "@/components/providers/RequireAuth";
import { ApiError, apiFetch } from "@/lib/api";
import type { CustomerDeliveryAddress } from "@/lib/auth";

type AddressForm = {
  label: string;
  countryCode: string;
  city: string;
  line1: string;
  line2: string;
  postalCode: string;
  region: string;
  isDefault: boolean;
};

const EMPTY_FORM: AddressForm = {
  label: "Home",
  countryCode: "AM",
  city: "",
  line1: "",
  line2: "",
  postalCode: "",
  region: "",
  isDefault: false,
};

function AccountContent() {
  const { translate } = useLanguage();
  const { user, logout } = useAuth();
  const [addresses, setAddresses] = useState<CustomerDeliveryAddress[]>([]);
  const [form, setForm] = useState<AddressForm>(EMPTY_FORM);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const loadAddresses = useCallback(async () => {
    setIsLoading(true);
    setError("");
    try {
      setAddresses(await apiFetch<CustomerDeliveryAddress[]>("/api/delivery-addresses"));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load addresses.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadAddresses();
  }, [loadAddresses]);

  function startEdit(address: CustomerDeliveryAddress) {
    setEditingId(address.id);
    setForm({
      label: address.label,
      countryCode: address.countryCode,
      city: address.city,
      line1: address.line1,
      line2: address.line2 ?? "",
      postalCode: address.postalCode ?? "",
      region: address.region ?? "",
      isDefault: address.isDefault,
    });
    setMessage("");
  }

  function resetForm() {
    setEditingId(null);
    setForm(EMPTY_FORM);
  }

  async function saveAddress(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSaving(true);
    setMessage("");
    setError("");

    const payload = {
      label: form.label,
      countryCode: form.countryCode,
      city: form.city,
      line1: form.line1,
      line2: form.line2 || null,
      postalCode: form.postalCode || null,
      region: form.region || null,
      isDefault: form.isDefault,
    };

    try {
      if (editingId) {
        await apiFetch(`/api/delivery-addresses/${editingId}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
        setMessage("Address updated.");
      } else {
        await apiFetch("/api/delivery-addresses", {
          method: "POST",
          body: JSON.stringify(payload),
        });
        setMessage("Address added.");
      }
      resetForm();
      await loadAddresses();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not save address.");
    } finally {
      setIsSaving(false);
    }
  }

  async function setDefault(addressId: string) {
    setError("");
    try {
      await apiFetch(`/api/delivery-addresses/${addressId}/default`, { method: "POST" });
      await loadAddresses();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not set default address.");
    }
  }

  async function removeAddress(addressId: string) {
    if (!window.confirm("Delete this delivery address?")) {
      return;
    }

    setError("");
    try {
      await apiFetch(`/api/delivery-addresses/${addressId}`, { method: "DELETE" });
      if (editingId === addressId) {
        resetForm();
      }
      await loadAddresses();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not delete address.");
    }
  }

  return (
    <PublicSiteLayout>
      <section className="section container catalog-page">
        <h1>{translate("accountTitle")}</h1>

        {user ? (
          <>
            <p className="catalog-message">
              {translate("signedInAs")} <strong>{user.fullName}</strong> ({user.email})
            </p>
            <button
              type="button"
              className="button button-secondary"
              onClick={logout}
              style={{ marginBottom: 24 }}
            >
              {translate("signOut")}
            </button>
          </>
        ) : null}

        <div className="auth-card" style={{ maxWidth: 720, marginBottom: 24 }}>
          <h2>Delivery addresses</h2>
          <p className="catalog-message">
            Save one or more delivery addresses. Choose a default for checkout.
          </p>

          {isLoading ? <p className="catalog-message">{translate("loading")}</p> : null}
          {error ? <p className="catalog-message catalog-error">{error}</p> : null}
          {message ? <p className="catalog-message">{message}</p> : null}

          {!isLoading && addresses.length === 0 ? (
            <p className="catalog-message">No addresses yet. Add your first delivery address below.</p>
          ) : null}

          <div className="address-list">
            {addresses.map((address) => (
              <div key={address.id} className="address-card">
                <div>
                  <strong>
                    {address.label}
                    {address.isDefault ? " · Default" : ""}
                  </strong>
                  <p>
                    {address.line1}
                    {address.line2 ? `, ${address.line2}` : ""}
                    <br />
                    {address.city}
                    {address.region ? `, ${address.region}` : ""} {address.postalCode}
                    <br />
                    {address.countryCode}
                  </p>
                </div>
                <div className="address-card-actions">
                  {!address.isDefault ? (
                    <button type="button" className="button-ghost" onClick={() => void setDefault(address.id)}>
                      Set default
                    </button>
                  ) : null}
                  <button type="button" className="button-ghost" onClick={() => startEdit(address)}>
                    Edit
                  </button>
                  <button
                    type="button"
                    className="button-ghost"
                    onClick={() => void removeAddress(address.id)}
                  >
                    Delete
                  </button>
                </div>
              </div>
            ))}
          </div>

          <h3 style={{ marginTop: 28 }}>{editingId ? "Edit address" : "Add address"}</h3>
          <form className="checkout-form" onSubmit={(event) => void saveAddress(event)}>
            <label>
              Label
              <input
                required
                value={form.label}
                onChange={(event) => setForm((current) => ({ ...current, label: event.target.value }))}
              />
            </label>
            <label>
              Country code
              <input
                required
                minLength={2}
                maxLength={2}
                value={form.countryCode}
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    countryCode: event.target.value.toUpperCase(),
                  }))
                }
              />
            </label>
            <label>
              City
              <input
                required
                value={form.city}
                onChange={(event) => setForm((current) => ({ ...current, city: event.target.value }))}
              />
            </label>
            <label>
              Address line 1
              <input
                required
                value={form.line1}
                onChange={(event) => setForm((current) => ({ ...current, line1: event.target.value }))}
              />
            </label>
            <label>
              Address line 2
              <input
                value={form.line2}
                onChange={(event) => setForm((current) => ({ ...current, line2: event.target.value }))}
              />
            </label>
            <label>
              Region
              <input
                value={form.region}
                onChange={(event) => setForm((current) => ({ ...current, region: event.target.value }))}
              />
            </label>
            <label>
              Postal code
              <input
                value={form.postalCode}
                onChange={(event) =>
                  setForm((current) => ({ ...current, postalCode: event.target.value }))
                }
              />
            </label>
            {!editingId ? (
              <label style={{ display: "flex", alignItems: "center", gap: 8 }}>
                <input
                  type="checkbox"
                  checked={form.isDefault}
                  onChange={(event) =>
                    setForm((current) => ({ ...current, isDefault: event.target.checked }))
                  }
                />
                Set as default address
              </label>
            ) : null}
            <div style={{ display: "flex", gap: 12 }}>
              <button type="submit" className="button button-primary" disabled={isSaving}>
                {isSaving ? "Saving…" : editingId ? "Save changes" : "Add address"}
              </button>
              {editingId ? (
                <button type="button" className="button button-secondary" onClick={resetForm}>
                  Cancel
                </button>
              ) : null}
            </div>
          </form>
        </div>

        <div className="catalog-grid">
          <Link href="/orders" className="catalog-card">
            <h2>{translate("myOrders")}</h2>
            <p>{translate("accountOrdersDesc")}</p>
          </Link>
          <Link href="/wishlist" className="catalog-card">
            <h2>{translate("wishlist")}</h2>
            <p>{translate("accountWishlistDesc")}</p>
          </Link>
          <Link href="/cart" className="catalog-card">
            <h2>{translate("cart")}</h2>
            <p>{translate("accountCartDesc")}</p>
          </Link>
        </div>
      </section>
    </PublicSiteLayout>
  );
}

export default function AccountPage() {
  return (
    <RequireAuth>
      <AccountContent />
    </RequireAuth>
  );
}
