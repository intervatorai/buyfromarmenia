import type { ReactNode } from "react";
import { RequireAuth } from "@/components/providers/RequireAuth";

export default function CartLayout({ children }: { children: ReactNode }) {
  return <RequireAuth>{children}</RequireAuth>;
}
