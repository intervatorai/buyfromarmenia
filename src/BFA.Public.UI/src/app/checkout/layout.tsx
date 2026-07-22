import type { ReactNode } from "react";
import { RequireAuth } from "@/components/providers/RequireAuth";

export default function CheckoutLayout({ children }: { children: ReactNode }) {
  return <RequireAuth>{children}</RequireAuth>;
}
