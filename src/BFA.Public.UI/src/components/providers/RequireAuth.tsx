"use client";

import { usePathname, useRouter } from "next/navigation";
import { useEffect, type ReactNode } from "react";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useAuth } from "@/components/providers/AuthProvider";
import { useLanguage } from "@/components/providers/LanguageProvider";

/**
 * Guards customer-only pages (account, orders, cart, wishlist, checkout).
 * Unauthenticated visitors are redirected to the login page and returned
 * to the original page after signing in.
 */
export function RequireAuth({ children }: { children: ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const { translate } = useLanguage();
  const { isAuthenticated, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace(`/account/login?returnTo=${encodeURIComponent(pathname)}`);
    }
  }, [isLoading, isAuthenticated, pathname, router]);

  if (isLoading || !isAuthenticated) {
    return (
      <PublicSiteLayout>
        <section className="section container catalog-page">
          <p className="catalog-message">{translate("loading")}</p>
        </section>
      </PublicSiteLayout>
    );
  }

  return <>{children}</>;
}
