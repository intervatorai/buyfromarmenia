"use client";

import Link from "next/link";
import type { ReactNode } from "react";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import { useAuth } from "@/components/providers/AuthProvider";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { RequireAuth } from "@/components/providers/RequireAuth";

type AccountLink = {
  href: string;
  titleKey:
    | "myOrders"
    | "shippingAddresses"
    | "wishlist"
    | "cart"
    | "changePassword";
  descKey:
    | "accountOrdersDesc"
    | "accountAddressesDesc"
    | "accountWishlistDesc"
    | "accountCartDesc"
    | "accountPasswordDesc";
  icon: ReactNode;
};

const links: AccountLink[] = [
  {
    href: "/orders",
    titleKey: "myOrders",
    descKey: "accountOrdersDesc",
    icon: (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path
          d="M7 4h10l1.2 3.2A2 2 0 0 1 16.3 10H7.7A2 2 0 0 1 5.8 7.2L7 4Z"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.7"
          strokeLinejoin="round"
        />
        <path
          d="M7 10v9a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1v-9"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.7"
          strokeLinecap="round"
        />
        <path
          d="M10 14h4"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.7"
          strokeLinecap="round"
        />
      </svg>
    ),
  },
  {
    href: "/account/addresses",
    titleKey: "shippingAddresses",
    descKey: "accountAddressesDesc",
    icon: (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path
          d="M12 21s-6-5.2-6-10a6 6 0 1 1 12 0c0 4.8-6 10-6 10Z"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.7"
          strokeLinejoin="round"
        />
        <circle cx="12" cy="11" r="2.2" fill="none" stroke="currentColor" strokeWidth="1.7" />
      </svg>
    ),
  },
  {
    href: "/wishlist",
    titleKey: "wishlist",
    descKey: "accountWishlistDesc",
    icon: (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path
          d="M12 20s-7-4.4-7-9.2A4.2 4.2 0 0 1 12 7.2a4.2 4.2 0 0 1 7 3.6C19 15.6 12 20 12 20Z"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.7"
          strokeLinejoin="round"
        />
      </svg>
    ),
  },
  {
    href: "/cart",
    titleKey: "cart",
    descKey: "accountCartDesc",
    icon: (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path
          d="M4 5h2l2.2 11h9.4L20 8H8"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.7"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <circle cx="10" cy="19" r="1.3" fill="currentColor" />
        <circle cx="17" cy="19" r="1.3" fill="currentColor" />
      </svg>
    ),
  },
  {
    href: "/account/password",
    titleKey: "changePassword",
    descKey: "accountPasswordDesc",
    icon: (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <rect
          x="5"
          y="11"
          width="14"
          height="9"
          rx="2"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.7"
        />
        <path
          d="M8 11V8a4 4 0 0 1 8 0v3"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.7"
          strokeLinecap="round"
        />
      </svg>
    ),
  },
];

function AccountContent() {
  const { translate } = useLanguage();
  const { user, logout } = useAuth();
  const firstName = user?.fullName?.trim().split(/\s+/)[0] ?? "";

  return (
    <PublicSiteLayout>
      <section className="section container account-page">
        <div className="account-hero">
          <div className="account-hero-glow" aria-hidden="true" />
          <div className="account-hero-content">
            <p className="account-eyebrow">{translate("accountWelcome")}</p>
            <h1>{firstName || translate("accountTitle")}</h1>
            {user ? (
              <p className="account-hero-meta">
                <span>{user.fullName}</span>
                <span aria-hidden="true">·</span>
                <span>{user.email}</span>
              </p>
            ) : null}
            <p className="account-hero-hint">{translate("accountNavHint")}</p>
          </div>
          <button type="button" className="button button-secondary account-signout" onClick={logout}>
            {translate("signOut")}
          </button>
        </div>

        <div className="account-nav-grid">
          {links.map((link, index) => (
            <Link
              key={link.href}
              href={link.href}
              className="account-nav-tile"
              style={{ animationDelay: `${80 + index * 70}ms` }}
            >
              <span className="account-nav-icon">{link.icon}</span>
              <span className="account-nav-copy">
                <strong>{translate(link.titleKey)}</strong>
                <span>{translate(link.descKey)}</span>
              </span>
              <span className="account-nav-arrow" aria-hidden="true">
                →
              </span>
            </Link>
          ))}
        </div>
      </section>
    </PublicSiteLayout>
  );
}

export default function AccountPage() {
  return (
    <RequireAuth>
      <AccountContent />
    </RequireAuth>
  );
}
