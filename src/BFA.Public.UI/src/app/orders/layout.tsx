import type { ReactNode } from "react";
import { RequireAuth } from "@/components/providers/RequireAuth";

export default function OrdersLayout({ children }: { children: ReactNode }) {
  return <RequireAuth>{children}</RequireAuth>;
}
