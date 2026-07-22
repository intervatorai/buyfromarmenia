"use client";

import Link from "next/link";
import { useEffect } from "react";
import { useLanguage } from "@/components/providers/LanguageProvider";
import type { PublicCategory } from "@/lib/api";
import { BrandLogo } from "./BrandLogo";

type MobileDrawerProps = {
  open: boolean;
  onClose: () => void;
  categories: PublicCategory[];
};

export function MobileDrawer({ open, onClose, categories }: MobileDrawerProps) {
  const { translate } = useLanguage();

  useEffect(() => {
    document.body.classList.toggle("menu-open", open);
    return () => document.body.classList.remove("menu-open");
  }, [open]);

  return (
    <div
      className={`mobile-drawer${open ? " open" : ""}`}
      onClick={(event) => {
        if (event.target === event.currentTarget) {
          onClose();
        }
      }}
    >
      <div className="mobile-drawer-panel">
        <div className="mobile-drawer-header">
          <BrandLogo compact />
          <button className="close-menu" type="button" onClick={onClose}>
            ×
          </button>
        </div>

        <nav className="mobile-links">
          <Link href="/products" onClick={onClose}>
            {translate("productCatalog")}
          </Link>
          <Link href="/categories" onClick={onClose}>
            {translate("allCategories")}
          </Link>
          {categories.map((category) => (
            <Link
              key={category.id}
              href={`/categories/${category.slug}`}
              onClick={onClose}
            >
              {category.name}
            </Link>
          ))}
          <Link href="/account" onClick={onClose}>
            {translate("accountTitle")}
          </Link>
          <Link href="/sell" onClick={onClose}>
            {translate("forSellersMobile")}
          </Link>
        </nav>
      </div>
    </div>
  );
}
