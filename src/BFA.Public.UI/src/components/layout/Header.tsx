"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { apiFetch, type PublicCart, type PublicCategory } from "@/lib/api";
import { CART_UPDATED_EVENT, getCartId } from "@/lib/cart-session";
import { BrandLogo } from "./BrandLogo";
import { MobileDrawer } from "./MobileDrawer";

export function Header() {
  const router = useRouter();
  const { translate, toggleLanguage, languageLabel } = useLanguage();
  const [menuOpen, setMenuOpen] = useState(false);
  const [cartQuantity, setCartQuantity] = useState(0);
  const [searchQuery, setSearchQuery] = useState("");
  const [categories, setCategories] = useState<PublicCategory[]>([]);

  useEffect(() => {
    async function loadCartQuantity() {
      const cartId = getCartId();
      if (!cartId) return;

      try {
        const cart = await apiFetch<PublicCart>(`/api/carts/${cartId}`);
        setCartQuantity(cart.totalQuantity);
      } catch {
        setCartQuantity(0);
      }
    }

    void loadCartQuantity();
    window.addEventListener(CART_UPDATED_EVENT, loadCartQuantity);
    return () => window.removeEventListener(CART_UPDATED_EVENT, loadCartQuantity);
  }, []);

  useEffect(() => {
    void apiFetch<PublicCategory[]>("/api/categories")
      .then((data) =>
        setCategories(
          data.filter((category) => !category.parentCategoryId),
        ),
      )
      .catch(() => setCategories([]));
  }, []);

  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const q = searchQuery.trim();
    if (q) {
      router.push(`/products?search=${encodeURIComponent(q)}`);
    } else {
      router.push("/products");
    }
  }

  return (
    <>
      <header className="header">
        <div className="container header-main">
          <BrandLogo />

          <form className="search" onSubmit={handleSearch}>
            <input
              type="search"
              value={searchQuery}
              onChange={(event) => setSearchQuery(event.target.value)}
              placeholder={translate("searchPlaceholder")}
              aria-label={translate("search")}
            />
            <button type="submit" aria-label={translate("search")}>
              ⌕
            </button>
          </form>

          <div className="header-actions">
            <button
              className="lang-button"
              type="button"
              onClick={toggleLanguage}
            >
              <span>{languageLabel}</span>
            </button>

            <Link
              href="/account"
              className="icon-button desktop-heart"
              aria-label={translate("accountTitle")}
            >
              👤
            </Link>

            <Link
              href="/wishlist"
              className="icon-button desktop-heart"
              aria-label={translate("favorites")}
            >
              ♡
            </Link>

            <Link
              href="/cart"
              className="icon-button"
              aria-label={translate("shoppingCart")}
            >
              🛒
              {cartQuantity > 0 ? (
                <span className="cart-count">{cartQuantity}</span>
              ) : null}
            </Link>

            <button
              className="icon-button mobile-menu-button"
              type="button"
              aria-label={translate("openMenu")}
              onClick={() => setMenuOpen(true)}
            >
              ☰
            </button>
          </div>
        </div>

        <div className="container nav-row">
          <Link href="/products" className="catalog-button">
            <span>☰</span>
            <span>{translate("productCatalog")}</span>
          </Link>

          <nav className="main-nav" aria-label={translate("categoriesTitle")}>
            {categories.map((category) => (
              <Link
                key={category.id}
                href={`/categories/${category.slug}`}
              >
                {category.name}
              </Link>
            ))}
          </nav>
        </div>
      </header>

      <MobileDrawer
        open={menuOpen}
        onClose={() => setMenuOpen(false)}
        categories={categories}
      />
    </>
  );
}
