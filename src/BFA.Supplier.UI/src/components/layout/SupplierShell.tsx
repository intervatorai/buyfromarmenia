"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth } from "@/components/providers/AuthProvider";
import { BrandLogo } from "@/components/layout/BrandLogo";

const navItems = [
  { href: "/", label: "Dashboard", icon: "◫" },
  { href: "/products", label: "Products", icon: "▣" },
  { href: "/inventory", label: "Inventory", icon: "▤" },
  { href: "/orders", label: "Orders", icon: "☰" },
  { href: "/finance", label: "Finance", icon: "◎" },
  { href: "/settings", label: "Settings", icon: "⚙" },
];

export function SupplierShell({
  title,
  children,
  action,
}: {
  title: string;
  children: React.ReactNode;
  action?: React.ReactNode;
}) {
  const pathname = usePathname();
  const { user, isLoading, logout } = useAuth();

  if (isLoading) {
    return (
      <div style={{ minHeight: "100vh", display: "grid", placeItems: "center" }}>
        <p>Loading...</p>
      </div>
    );
  }

  if (!user) {
    return null;
  }

  function isActive(href: string) {
    if (href === "/") {
      return pathname === "/";
    }

    return pathname.startsWith(href);
  }

  return (
    <div className="supplier-shell">
      <aside className="supplier-sidebar">
        <BrandLogo href="/" subtitle="Supplier Portal" />

        <nav className="supplier-nav">
          {navItems.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className={`supplier-nav-link${isActive(item.href) ? " active" : ""}`}
            >
              <span aria-hidden>{item.icon}</span>
              {item.label}
            </Link>
          ))}
        </nav>

        <div className="supplier-sidebar-footer">
          <div style={{ marginBottom: 8 }}>{user.tradingName}</div>
          <button
            type="button"
            onClick={logout}
            style={{
              background: "transparent",
              border: "1px solid rgba(255,255,255,0.2)",
              color: "#e2e8f0",
              borderRadius: 6,
              padding: "6px 10px",
              width: "100%",
            }}
          >
            Sign out
          </button>
        </div>
      </aside>

      <div className="supplier-main">
        <header className="supplier-topbar">
          <h1>{title}</h1>
          {action ? (
            <div className="supplier-topbar-actions">{action}</div>
          ) : null}
        </header>

        <div className="supplier-content">{children}</div>
      </div>
    </div>
  );
}
