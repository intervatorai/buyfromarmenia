"use client";

import { FormEvent, useState } from "react";
import { BrandLogo } from "@/components/layout/BrandLogo";
import { ApiError } from "@/lib/api";
import { useAuth } from "@/components/providers/AuthProvider";

export default function LoginPage() {
  const { login } = useAuth();
  const [email, setEmail] = useState("admin");
  const [password, setPassword] = useState("admin");
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
        setError("Invalid login or password.");
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
        <BrandLogo href="/login" subtitle="Admin Panel" />

        <h1>Sign in</h1>
        <p>Access the BuyFromArmenia administration panel.</p>

        <form onSubmit={handleSubmit}>
          <div className="form-field">
            <label htmlFor="email">Login</label>
            <input
              id="email"
              type="text"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              autoComplete="username"
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

        <div className="login-hint">
          Development credentials:
          <br />
          admin / admin
        </div>
      </div>
    </div>
  );
}
