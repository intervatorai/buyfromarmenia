"use client";

import Link from "next/link";
import { useEffect, useRef, useState, useTransition } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useLanguage } from "@/components/providers/LanguageProvider";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";
import {
  ApiError,
  apiFetch,
  type PublicCart,
  type PublicCategory,
  type PublicProduct,
} from "@/lib/api";
import { getCartId } from "@/lib/cart-session";
import {
  catalogCacheKey,
  getCachedCatalogProducts,
  getLastCatalogProducts,
  setCachedCatalogProducts,
} from "@/lib/catalog-cache";

function formatPrice(price: number, currency: string) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: currency || "USD",
  }).format(price);
}

type ProductsCatalogProps = {
  initialCategorySlug?: string;
};

export function ProductsCatalog({ initialCategorySlug }: ProductsCatalogProps) {
  const { translate, language } = useLanguage();
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isPending, startTransition] = useTransition();
  const requestIdRef = useRef(0);

  const resolvedCategory =
    initialCategorySlug ?? searchParams.get("category") ?? "";
  const resolvedSearch = searchParams.get("search") ?? "";
  const initialKey = catalogCacheKey(resolvedCategory, resolvedSearch, language);
  const initialProducts =
    getCachedCatalogProducts(initialKey) ?? getLastCatalogProducts();

  const [products, setProducts] = useState<PublicProduct[]>(initialProducts);
  const [categories, setCategories] = useState<PublicCategory[]>([]);
  const [categorySlug, setCategorySlug] = useState(resolvedCategory);
  const [search, setSearch] = useState(resolvedSearch);
  const [isInitialLoading, setIsInitialLoading] = useState(
    initialProducts.length === 0,
  );
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [error, setError] = useState("");
  const [wishlistProductIds, setWishlistProductIds] = useState<string[]>([]);

  useEffect(() => {
    setCategorySlug(initialCategorySlug ?? searchParams.get("category") ?? "");
    setSearch(searchParams.get("search") ?? "");
  }, [initialCategorySlug, searchParams]);

  useEffect(() => {
    async function loadCategories() {
      try {
        const data = await apiFetch<PublicCategory[]>("/api/categories");
        setCategories(data.filter((category) => !category.parentCategoryId));
      } catch {
        // Categories are optional for the catalog list.
      }
    }

    void loadCategories();
    void apiFetch<PublicCart>(`/api/carts/${getCartId()}`)
      .then((cart) => setWishlistProductIds(cart.wishlistProductIds))
      .catch(() => undefined);
  }, []);

  useEffect(() => {
    const key = catalogCacheKey(categorySlug, search, language);
    const cached = getCachedCatalogProducts(key);
    if (cached) {
      setProducts(cached);
      setIsInitialLoading(false);
    }

    const requestId = ++requestIdRef.current;
    const hasVisibleProducts = (cached?.length ?? products.length) > 0;

    if (hasVisibleProducts) {
      setIsRefreshing(true);
    } else {
      setIsInitialLoading(true);
    }
    setError("");

    async function loadProducts() {
      try {
        const params = new URLSearchParams();
        if (categorySlug) params.set("category", categorySlug);
        if (search) params.set("search", search);
        params.set("lang", language);
        const data = await apiFetch<PublicProduct[]>(
          `/api/products?${params.toString()}`,
        );

        if (requestId !== requestIdRef.current) {
          return;
        }

        setCachedCatalogProducts(key, data);
        setProducts(data);
      } catch (err) {
        if (requestId !== requestIdRef.current) {
          return;
        }
        setError(err instanceof ApiError ? err.message : "Failed to load products.");
      } finally {
        if (requestId === requestIdRef.current) {
          setIsInitialLoading(false);
          setIsRefreshing(false);
        }
      }
    }

    void loadProducts();
    // products.length intentionally omitted — only react to filter changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [categorySlug, search, language]);

  function selectCategory(nextSlug: string) {
    // Update UI immediately so the grid does not wait for route remount.
    setCategorySlug(nextSlug);

    startTransition(() => {
      if (nextSlug) {
        router.replace(`/categories/${nextSlug}`, { scroll: false });
        return;
      }

      const params = new URLSearchParams();
      if (search) params.set("search", search);
      const query = params.toString();
      router.replace(query ? `/products?${query}` : "/products", { scroll: false });
    });
  }

  async function toggleFavorite(productId: string) {
    const isFavorite = wishlistProductIds.includes(productId);
    await apiFetch(`/api/carts/${getCartId()}/wishlist/${productId}`, {
      method: "PUT",
      body: JSON.stringify({ isFavorite: !isFavorite }),
    });
    setWishlistProductIds((current) =>
      isFavorite
        ? current.filter((id) => id !== productId)
        : [...current, productId],
    );
  }

  const hasActiveFilters = Boolean(categorySlug || search);
  const showRefreshing = isRefreshing || isPending;

  return (
    <PublicSiteLayout>
      <section className="section container catalog-page">
        <div className="catalog-layout">
          <aside className="catalog-sidebar">
            <h2 className="catalog-sidebar-title">{translate("filters")}</h2>

            {categories.length > 0 ? (
              <div className="catalog-filter-group">
                <p className="catalog-filter-label">{translate("categoriesTitle")}</p>
                <ul className="catalog-category-list">
                  <li>
                    <button
                      type="button"
                      className={
                        categorySlug === ""
                          ? "catalog-category-option active"
                          : "catalog-category-option"
                      }
                      onClick={() => selectCategory("")}
                    >
                      {translate("allCategories")}
                    </button>
                  </li>
                  {categories.map((category) => (
                    <li key={category.id}>
                      <button
                        type="button"
                        className={
                          categorySlug === category.slug
                            ? "catalog-category-option active"
                            : "catalog-category-option"
                        }
                        onClick={() => selectCategory(category.slug)}
                      >
                        {category.name}
                      </button>
                    </li>
                  ))}
                </ul>
              </div>
            ) : null}

            {hasActiveFilters ? (
              <button
                type="button"
                className="catalog-clear-filters"
                onClick={() => {
                  setCategorySlug("");
                  setSearch("");
                  startTransition(() => {
                    router.replace("/products", { scroll: false });
                  });
                }}
              >
                {translate("clearFilters")}
              </button>
            ) : null}
          </aside>

          <div className="catalog-results">
            <div className="catalog-results-header">
              <h1>{translate("productCatalog")}</h1>
              {!isInitialLoading && !error ? (
                <p className="catalog-results-count">
                  {products.length} {translate("products")}
                  {showRefreshing ? " · …" : ""}
                </p>
              ) : null}
            </div>

            {isInitialLoading ? (
              <p className="catalog-message">{translate("loadingProducts")}</p>
            ) : null}
            {error ? <p className="catalog-message catalog-error">{error}</p> : null}

            {!isInitialLoading && !error && products.length === 0 ? (
              <p className="catalog-message">{translate("noProductsYet")}</p>
            ) : null}

            {products.length > 0 ? (
              <div
                className="product-grid catalog-grid"
                style={{
                  opacity: showRefreshing ? 0.72 : 1,
                  transition: "opacity 0.18s ease",
                }}
                aria-busy={showRefreshing}
              >
                {products.map((product) => {
                  const isFavorite = wishlistProductIds.includes(product.id);
                  return (
                    <article
                      key={product.id}
                      className="product-card catalog-product-card"
                    >
                      <button
                        type="button"
                        className={`favorite-button${isFavorite ? " active" : ""}`}
                        aria-label={translate("favorites")}
                        onClick={() => void toggleFavorite(product.id)}
                      >
                        {isFavorite ? "♥" : "♡"}
                      </button>
                      <Link
                        href={`/products/${product.slug || product.id}`}
                        className="catalog-card-link"
                      >
                        <div className="product-image catalog-product-image">
                          {product.primaryImageUrl ? (
                            // eslint-disable-next-line @next/next/no-img-element
                            <img src={product.primaryImageUrl} alt={product.name} />
                          ) : (
                            <div className="catalog-image-placeholder" />
                          )}
                        </div>

                        <h3>{product.name}</h3>
                        {product.shortDescription ? (
                          <p className="catalog-short-description">
                            {product.shortDescription}
                          </p>
                        ) : null}
                        <div className="product-price">
                          {formatPrice(product.price, product.currency)}
                        </div>
                      </Link>
                    </article>
                  );
                })}
              </div>
            ) : null}
          </div>
        </div>
      </section>
    </PublicSiteLayout>
  );
}
