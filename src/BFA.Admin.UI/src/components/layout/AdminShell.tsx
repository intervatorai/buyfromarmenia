"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth } from "@/components/providers/AuthProvider";
import { BrandLogo } from "@/components/layout/BrandLogo";

const navItems = [
  { href: "/dashboard", label: "Dashboard" },
  { href: "/products", label: "Products" },
  { href: "/categories", label: "Categories" },
  { href: "/vendors", label: "Vendors" },
  { href: "/orders", label: "Orders" },
  { href: "/warehouse", label: "Warehouse" },
  { href: "/logistics", label: "Logistics" },
  { href: "/finance", label: "Finance" },
  { href: "/returns", label: "Returns" },
  { href: "/settings/audit", label: "Audit" },
  { href: "/settings/compliance", label: "Compliance" },
  { href: "/settings/users", label: "Users" },
];

export function AdminShell({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const { user, isLoading, logout } = useAuth();

  if (isLoading) {
    return (
      <div className="login-page">
        <p>Loading...</p>
      </div>
    );
  }

  if (!user) {
    return null;
  }

  return (
    <div className="admin-shell">
      <aside className="admin-sidebar">
        <BrandLogo href="/dashboard" subtitle="Admin" />

        <nav className="admin-nav">
          {navItems.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className={`admin-nav-link${
                pathname === item.href ? " active" : ""
              }`}
            >
              {item.label}
            </Link>
          ))}
        </nav>
      </aside>

      <div className="admin-main">
        <header className="admin-topbar">
          <h1>{title}</h1>

          <div className="admin-user">
            <div className="admin-user-meta">
              <strong>{user.fullName}</strong>
              <span>
                {user.email} · {user.role}
              </span>
            </div>
            <button className="button-ghost" type="button" onClick={logout}>
              Logout
            </button>
          </div>
        </header>

        <div className="admin-content">{children}</div>
      </div>
    </div>
  );
}
