"use client";

import Link from "next/link";
import { FormEvent, Suspense, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useAuth } from "@/components/providers/AuthProvider";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { ApiError } from "@/lib/api";
import {
  DEFAULT_PHONE_COUNTRY_ISO,
  PHONE_COUNTRIES,
  buildE164Phone,
} from "@/lib/phone-countries";

function RegisterPageContent() {
  const { translate } = useLanguage();
  const { register } = useAuth();
  const searchParams = useSearchParams();
  const returnTo = searchParams.get("returnTo");
  const loginHref =
    returnTo && returnTo.startsWith("/")
      ? `/account/login?returnTo=${encodeURIComponent(returnTo)}`
      : "/account/login";
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneCountryIso, setPhoneCountryIso] = useState(DEFAULT_PHONE_COUNTRY_ISO);
  const [phoneLocal, setPhoneLocal] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const selectedCountry = useMemo(
    () =>
      PHONE_COUNTRIES.find((country) => country.iso === phoneCountryIso) ??
      PHONE_COUNTRIES[0],
    [phoneCountryIso],
  );

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setIsSubmitting(true);

    try {
      const phone = buildE164Phone(selectedCountry.dialCode, phoneLocal);
      await register(email, password, fullName, phone);
    } catch (err) {
      if (err instanceof ApiError && err.status === 409) {
        setError(translate("emailAlreadyRegistered"));
      } else {
        setError(translate("registrationFailed"));
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <PublicSiteLayout>
      <section className="section container catalog-page">
        <div className="auth-card">
          <h1>{translate("createAccount")}</h1>
          <p className="catalog-message">{translate("registerHint")}</p>

          <form className="checkout-form" onSubmit={(event) => void handleSubmit(event)}>
            <label>
              {translate("fullName")}
              <input
                required
                value={fullName}
                onChange={(event) => setFullName(event.target.value)}
                autoComplete="name"
              />
            </label>
            <label>
              {translate("email")}
              <input
                required
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                autoComplete="email"
              />
            </label>
            <label>
              {translate("phone")}
              <div className="phone-input-row">
                <select
                  aria-label={translate("phoneCountry")}
                  value={phoneCountryIso}
                  onChange={(event) => setPhoneCountryIso(event.target.value)}
                >
                  {PHONE_COUNTRIES.map((country) => (
                    <option key={country.iso} value={country.iso}>
                      {country.name} ({country.dialCode})
                    </option>
                  ))}
                </select>
                <input
                  type="tel"
                  inputMode="tel"
                  placeholder={translate("phoneNumberPlaceholder")}
                  value={phoneLocal}
                  onChange={(event) => setPhoneLocal(event.target.value)}
                  autoComplete="tel-national"
                />
              </div>
            </label>
            <label>
              {translate("password")}
              <input
                required
                type="password"
                minLength={8}
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                autoComplete="new-password"
              />
            </label>

            {error ? <p className="catalog-message catalog-error">{error}</p> : null}

            <button
              type="submit"
              className="button button-primary"
              disabled={isSubmitting}
            >
              {isSubmitting ? translate("creatingAccount") : translate("createAccount")}
            </button>
          </form>

          <p className="auth-switch">
            {translate("alreadyHaveAccount")}{" "}
            <Link href={loginHref}>{translate("signIn")}</Link>
          </p>
        </div>
      </section>
    </PublicSiteLayout>
  );
}

export default function RegisterPage() {
  return (
    <Suspense fallback={null}>
      <RegisterPageContent />
    </Suspense>
  );
}
