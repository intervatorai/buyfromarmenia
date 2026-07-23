"use client";

import Link from "next/link";
import { useCallback, useEffect, useMemo, useState, type FormEvent } from "react";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { RequireAuth } from "@/components/providers/RequireAuth";
import { AddressAutocomplete } from "@/components/address/AddressAutocomplete";
import type { AddressSuggestion } from "@/lib/address-autocomplete";
import { ApiError, apiFetch } from "@/lib/api";
import type { CustomerDeliveryAddress } from "@/lib/auth";

type ShippingCountryOption = {
  isoCode: string;
  nameEn: string;
  nameHy: string;
  sortOrder: number;
};

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
  countryCode: "",
  city: "",
  line1: "",
  line2: "",
  postalCode: "",
  region: "",
  isDefault: false,
};

function ShippingAddressesContent() {
  const { language, translate } = useLanguage();
  const [addresses, setAddresses] = useState<CustomerDeliveryAddress[]>([]);
  const [countries, setCountries] = useState<ShippingCountryOption[]>([]);
  const [form, setForm] = useState<AddressForm>(EMPTY_FORM);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [countriesLoaded, setCountriesLoaded] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const countryName = useCallback(
    (isoCode: string) => {
      const match = countries.find((country) => country.isoCode === isoCode);
      if (!match) {
        return isoCode;
      }
      return language === "hy" ? match.nameHy : match.nameEn;
    },
    [countries, language],
  );

  const selectOptions = useMemo(() => {
    const options = [...countries];
    if (
      form.countryCode &&
      !options.some((country) => country.isoCode === form.countryCode)
    ) {
      options.unshift({
        isoCode: form.countryCode,
        nameEn: form.countryCode,
        nameHy: form.countryCode,
        sortOrder: -1,
      });
    }
    return options;
  }, [countries, form.countryCode]);

  const loadPage = useCallback(async () => {
    setIsLoading(true);
    setError("");
    setCountriesLoaded(false);

    const errors: string[] = [];

    try {
      const addressList = await apiFetch<CustomerDeliveryAddress[]>("/api/delivery-addresses");
      setAddresses(addressList);
    } catch (err) {
      errors.push(err instanceof ApiError ? err.message : translate("addressesLoadFailed"));
      setAddresses([]);
    }

    try {
      const countryList = await apiFetch<ShippingCountryOption[]>("/api/shipping-countries");
      setCountries(countryList);
      setForm((current) => {
        if (current.countryCode) {
          return current;
        }
        return {
          ...current,
          countryCode: countryList[0]?.isoCode ?? "",
        };
      });
    } catch (err) {
      setCountries([]);
      errors.push(err instanceof ApiError ? err.message : translate("countriesLoadFailed"));
    } finally {
      setCountriesLoaded(true);
    }

    setError(errors.join(" "));
    setIsLoading(false);
  }, [translate]);

  useEffect(() => {
    void loadPage();
  }, [loadPage]);

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
    setForm({
      ...EMPTY_FORM,
      countryCode: countries[0]?.isoCode ?? "",
    });
  }

  function applyAddressSuggestion(suggestion: AddressSuggestion) {
    setForm((current) => {
      const matchedCountry = countries.find(
        (country) => country.isoCode === suggestion.countryCode,
      );
      // Always overwrite dependent fields on select (Zentbow approach).
      return {
        ...current,
        line1: suggestion.line1 || suggestion.displayName,
        city: suggestion.city,
        region: suggestion.region,
        postalCode: suggestion.postalCode,
        countryCode: matchedCountry?.isoCode || current.countryCode,
      };
    });
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
        setMessage(translate("addressUpdated"));
      } else {
        await apiFetch("/api/delivery-addresses", {
          method: "POST",
          body: JSON.stringify(payload),
        });
        setMessage(translate("addressAdded"));
      }
      resetForm();
      await loadPage();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : translate("addressSaveFailed"));
    } finally {
      setIsSaving(false);
    }
  }

  async function setDefault(addressId: string) {
    setError("");
    try {
      await apiFetch(`/api/delivery-addresses/${addressId}/default`, { method: "POST" });
      await loadPage();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : translate("addressDefaultFailed"));
    }
  }

  async function removeAddress(addressId: string) {
    if (!window.confirm(translate("addressDeleteConfirm"))) {
      return;
    }

    setError("");
    try {
      await apiFetch(`/api/delivery-addresses/${addressId}`, { method: "DELETE" });
      if (editingId === addressId) {
        resetForm();
      }
      await loadPage();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : translate("addressDeleteFailed"));
    }
  }

  return (
    <PublicSiteLayout>
      <section className="section container account-page">
        <Link href="/account" className="account-back-link">
          {translate("backToAccount")}
        </Link>

        <div className="account-panel-header">
          <div>
            <p className="account-eyebrow">{translate("accountTitle")}</p>
            <h1>{translate("shippingAddresses")}</h1>
            <p className="account-hero-hint">{translate("shippingAddressesIntro")}</p>
          </div>
        </div>

        <div className="account-panel">
          {isLoading ? <p className="catalog-message">{translate("loading")}</p> : null}
          {error ? <p className="catalog-message catalog-error">{error}</p> : null}
          {message ? <p className="catalog-message account-success">{message}</p> : null}

          {!isLoading && addresses.length === 0 ? (
            <div className="account-empty">
              <strong>{translate("shippingAddresses")}</strong>
              <p>{translate("addressesEmpty")}</p>
            </div>
          ) : null}

          <div className="address-list">
            {addresses.map((address) => (
              <div
                key={address.id}
                className={`address-card${address.isDefault ? " address-card-default" : ""}`}
              >
                <div>
                  <strong>
                    {address.label}
                    {address.isDefault ? (
                      <span className="address-default-badge">{translate("defaultAddress")}</span>
                    ) : null}
                  </strong>
                  <p>
                    {address.line1}
                    {address.line2 ? `, ${address.line2}` : ""}
                    <br />
                    {address.city}
                    {address.region ? `, ${address.region}` : ""} {address.postalCode}
                    <br />
                    {countryName(address.countryCode)} ({address.countryCode})
                  </p>
                </div>
                <div className="address-card-actions">
                  {!address.isDefault ? (
                    <button type="button" className="button-ghost" onClick={() => void setDefault(address.id)}>
                      {translate("setDefaultAddress")}
                    </button>
                  ) : null}
                  <button type="button" className="button-ghost" onClick={() => startEdit(address)}>
                    {translate("editAddress")}
                  </button>
                  <button
                    type="button"
                    className="button-ghost"
                    onClick={() => void removeAddress(address.id)}
                  >
                    {translate("deleteAddress")}
                  </button>
                </div>
              </div>
            ))}
          </div>

          <div className="account-form-block">
            <h2>{editingId ? translate("editAddress") : translate("addAddress")}</h2>
            <form className="checkout-form" onSubmit={(event) => void saveAddress(event)}>
              <div className="checkout-form-row">
                <label>
                  {translate("addressLabel")}
                  <input
                    required
                    value={form.label}
                    onChange={(event) => setForm((current) => ({ ...current, label: event.target.value }))}
                  />
                </label>
                <label>
                  {translate("addressCountry")}
                  <select
                    required
                    value={form.countryCode}
                    disabled={!countriesLoaded || (countries.length === 0 && !form.countryCode)}
                    onChange={(event) =>
                      setForm((current) => ({
                        ...current,
                        countryCode: event.target.value,
                      }))
                    }
                  >
                    {!countriesLoaded ? (
                      <option value="">{translate("loading")}</option>
                    ) : selectOptions.length === 0 ? (
                      <option value="">{translate("countriesEmpty")}</option>
                    ) : null}
                    {selectOptions.map((country) => {
                      const enabled = countries.some((item) => item.isoCode === country.isoCode);
                      const label = language === "hy" ? country.nameHy : country.nameEn;
                      return (
                        <option
                          key={country.isoCode}
                          value={country.isoCode}
                          disabled={!enabled && form.countryCode !== country.isoCode}
                        >
                          {label} ({country.isoCode})
                          {!enabled ? " — unavailable" : ""}
                        </option>
                      );
                    })}
                  </select>
                </label>
              </div>
              <label>
                {translate("addressLine1")}
                <AddressAutocomplete
                  required
                  value={form.line1}
                  countryCodes={countries.map((country) => country.isoCode)}
                  preferredCountryCode={form.countryCode || undefined}
                  placeholder={translate("addressAutocompleteHint")}
                  onChange={(line1) => setForm((current) => ({ ...current, line1 }))}
                  onSelect={applyAddressSuggestion}
                />
              </label>
              <label>
                {translate("addressLine2")}
                <input
                  value={form.line2}
                  onChange={(event) => setForm((current) => ({ ...current, line2: event.target.value }))}
                />
              </label>
              <label>
                {translate("addressCity")}
                <input
                  required
                  value={form.city}
                  onChange={(event) => setForm((current) => ({ ...current, city: event.target.value }))}
                />
              </label>
              <div className="checkout-form-row">
                <label>
                  {translate("addressRegion")}
                  <input
                    value={form.region}
                    onChange={(event) => setForm((current) => ({ ...current, region: event.target.value }))}
                  />
                </label>
                <label>
                  {translate("addressPostalCode")}
                  <input
                    value={form.postalCode}
                    onChange={(event) =>
                      setForm((current) => ({ ...current, postalCode: event.target.value }))
                    }
                  />
                </label>
              </div>
              {!editingId ? (
                <label className="account-checkbox">
                  <input
                    type="checkbox"
                    checked={form.isDefault}
                    onChange={(event) =>
                      setForm((current) => ({ ...current, isDefault: event.target.checked }))
                    }
                  />
                  {translate("setAsDefaultAddress")}
                </label>
              ) : null}
              <div className="account-form-actions">
                <button
                  type="submit"
                  className="button button-primary"
                  disabled={isSaving || !form.countryCode}
                >
                  {isSaving
                    ? translate("savingAddress")
                    : editingId
                      ? translate("saveAddressChanges")
                      : translate("addAddress")}
                </button>
                {editingId ? (
                  <button type="button" className="button button-secondary" onClick={resetForm}>
                    {translate("cancel")}
                  </button>
                ) : null}
              </div>
            </form>
          </div>
        </div>
      </section>
    </PublicSiteLayout>
  );
}

export default function ShippingAddressesPage() {
  return (
    <RequireAuth>
      <ShippingAddressesContent />
    </RequireAuth>
  );
}
