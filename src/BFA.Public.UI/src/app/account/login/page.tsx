"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { useSearchParams } from "next/navigation";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useAuth } from "@/components/providers/AuthProvider";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { ApiError } from "@/lib/api";

export default function LoginPage() {
  const { translate } = useLanguage();
  const { login } = useAuth();
  const searchParams = useSearchParams();
  const returnTo = searchParams.get("returnTo");
  const registerHref =
    returnTo && returnTo.startsWith("/")
      ? `/account/register?returnTo=${encodeURIComponent(returnTo)}`
      : "/account/register";
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setIsSubmitting(true);

    try {
      await login(email, password);
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) {
        setError(translate("invalidCredentials"));
      } else {
        setError(translate("signInFailed"));
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <PublicSiteLayout>
      <section className="section container catalog-page">
        <div className="auth-card">
          <h1>{translate("signIn")}</h1>
          <p className="catalog-message">{translate("signInHint")}</p>

          <form className="checkout-form" onSubmit={(event) => void handleSubmit(event)}>
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
              {translate("password")}
              <input
                required
                type="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                autoComplete="current-password"
              />
            </label>

            {error ? <p className="catalog-message catalog-error">{error}</p> : null}

            <button
              type="submit"
              className="button button-primary"
              disabled={isSubmitting}
            >
              {isSubmitting ? translate("signingIn") : translate("signIn")}
            </button>
          </form>

          <p className="auth-switch">
            {translate("noAccountYet")}{" "}
            <Link href={registerHref}>{translate("createAccount")}</Link>
          </p>
        </div>
      </section>
    </PublicSiteLayout>
  );
}
