"use client";

import { FormEvent, useState } from "react";
import { BrandLogo } from "@/components/layout/BrandLogo";
import { ApiError } from "@/lib/api";
import { useAuth } from "@/components/providers/AuthProvider";

export default function LoginPage() {
  const { login } = useAuth();
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
        setError("Invalid email or password.");
      } else {
        setError("Unable to sign in. Please try again.");
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="login-page">
      <div className="login-card">
        <div style={{ marginBottom: 24 }}>
          <BrandLogo href="/login" subtitle="Supplier Portal" showSubdomain={false} />
        </div>

        <h1>Sign in</h1>
        <p>Access your products, orders and finance.</p>

        <form onSubmit={handleSubmit}>
          <div className="form-field">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              autoComplete="email"
              required
            />
          </div>

          <div className="form-field">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete="current-password"
              required
            />
          </div>

          {error ? <div className="form-error">{error}</div> : null}

          <button
            className="button-primary"
            type="submit"
            disabled={isSubmitting}
            style={{ width: "100%" }}
          >
            {isSubmitting ? "Signing in..." : "Sign in"}
          </button>
        </form>

        <p style={{ marginTop: 20, fontSize: 14, color: "#64748b" }}>
          New supplier?{" "}
          <a
            href={
              new URL(
                "/sell",
                process.env.NEXT_PUBLIC_PUBLIC_URL ?? "http://localhost:3200",
              ).toString()
            }
            style={{ color: "#c45c26" }}
          >
            Apply on the public site
          </a>
        </p>
      </div>
    </div>
  );
}
