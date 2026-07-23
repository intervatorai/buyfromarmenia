import type { PublicProduct } from "@/lib/api";

const productCache = new Map<string, PublicProduct[]>();
let lastProducts: PublicProduct[] = [];

export function catalogCacheKey(
  categorySlug: string,
  search: string,
  language: string,
) {
  return `${language}|${categorySlug}|${search}`.toLowerCase();
}

export function getCachedCatalogProducts(key: string): PublicProduct[] | undefined {
  return productCache.get(key);
}

export function getLastCatalogProducts(): PublicProduct[] {
  return lastProducts;
}

export function setCachedCatalogProducts(key: string, products: PublicProduct[]) {
  productCache.set(key, products);
  lastProducts = products;
}
