"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { saveCustomerAuth, type CustomerAuthResponse } from "@/lib/auth";

export default function ImpersonatePage() {
  const router = useRouter();
  const [error, setError] = useState("");

  useEffect(() => {
    try {
      const hash = window.location.hash.replace(/^#/, "");
      if (!hash) {
        setError("Missing impersonation payload.");
        return;
      }

      const parsed = JSON.parse(decodeURIComponent(hash)) as CustomerAuthResponse;
      if (!parsed.accessToken || !parsed.userId || !parsed.email) {
        setError("Invalid impersonation payload.");
        return;
      }

      saveCustomerAuth(parsed);
      window.history.replaceState(null, "", "/account/impersonate");
      router.replace("/orders");
    } catch {
      setError("Unable to apply impersonation session.");
    }
  }, [router]);

  return (
    <main style={{ padding: "3rem 1.5rem", maxWidth: 480, margin: "0 auto" }}>
      {error ? (
        <>
          <h1>Impersonation failed</h1>
          <p>{error}</p>
          <a href="/account/login">Go to login</a>
        </>
      ) : (
        <p>Signing you in as customer...</p>
      )}
    </main>
  );
}
