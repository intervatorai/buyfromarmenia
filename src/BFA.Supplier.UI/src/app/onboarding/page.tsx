"use client";

import { useEffect, useState } from "react";
import { SupplierShell } from "@/components/layout/SupplierShell";
import { OnboardingWizard } from "@/components/onboarding/OnboardingWizard";
import { getSupplierId } from "@/lib/supplier-session";

const PUBLIC_SELL_URL = new URL(
  "/sell",
  process.env.NEXT_PUBLIC_PUBLIC_URL ?? "http://localhost:3200",
).toString();

export default function OnboardingPage() {
  const [canContinue, setCanContinue] = useState(false);

  useEffect(() => {
    if (!getSupplierId()) {
      window.location.replace(PUBLIC_SELL_URL);
      return;
    }
    setCanContinue(true);
  }, []);

  if (!canContinue) {
    return (
      <div style={{ padding: 48, textAlign: "center", color: "#64748b" }}>
        Redirecting to seller application…
      </div>
    );
  }

  return (
    <SupplierShell title="Complete application">
      <OnboardingWizard />
    </SupplierShell>
  );
}
