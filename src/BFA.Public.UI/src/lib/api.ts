import { getCustomerTokenFromCookie } from "./auth";

const API_URL = (process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5100").replace(
  /\/$/,
  "",
);

export class ApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

function readErrorMessage(text: string, statusText: string): string {
  if (!text) {
    return statusText || "Request failed";
  }

  try {
    const parsed = JSON.parse(text) as {
      error?: string;
      message?: string;
      title?: string;
      detail?: string;
    };
    return (
      parsed.error ||
      parsed.message ||
      parsed.detail ||
      parsed.title ||
      statusText ||
      "Request failed"
    );
  } catch {
    // Never dump HTML (e.g. Next.js 404 page) into the UI.
    if (/^\s*</.test(text) || text.includes("<!DOCTYPE")) {
      return statusText || "API request failed";
    }

    return text.length > 280 ? `${text.slice(0, 280)}…` : text;
  }
}

export async function apiFetch<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  if (!API_URL) {
    throw new ApiError(
      "NEXT_PUBLIC_API_URL is not configured. Set it to the Public API URL at build time.",
      0,
    );
  }

  const token = getCustomerTokenFromCookie();
  const headers = new Headers(options.headers);

  if (!headers.has("Content-Type") && options.body) {
    headers.set("Content-Type", "application/json");
  }

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    const text = await response.text().catch(() => "");
    throw new ApiError(readErrorMessage(text, response.statusText), response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const contentType = response.headers.get("content-type") ?? "";
  if (!contentType.includes("application/json")) {
    throw new ApiError(
      `Expected JSON from ${API_URL}${path}, got ${contentType || "unknown content type"}. Check NEXT_PUBLIC_API_URL.`,
      response.status,
    );
  }

  return response.json() as Promise<T>;
}

export type PublicProduct = {
  id: string;
  slug: string;
  name: string;
  shortDescription: string;
  price: number;
  currency: string;
  primaryImageUrl?: string | null;
  categoryId?: string | null;
  tag?: string | null;
};

export type PublicProductDetail = PublicProduct & {
  categorySlug?: string | null;
  description: string;
  ingredients: string;
  usageInstructions: string;
  images: Array<{
    url: string;
    altText?: string | null;
    isPrimary: boolean;
  }>;
  variants: Array<{
    id: string;
    supplierSku: string;
    size?: string | null;
    color?: string | null;
    weight: number;
    countryOfOrigin: string;
    available: number;
  }>;
  shipping?: {
    netWeight: number;
    grossWeight: number;
    packageLength: number;
    packageWidth: number;
    packageHeight: number;
    isFragile: boolean;
    isPerishable: boolean;
  } | null;
};

export type PublicCategory = {
  id: string;
  name: string;
  slug: string;
  description: string;
  parentCategoryId?: string | null;
};

export type PublicCart = {
  id: string;
  items: Array<{
    id: string;
    productId: string;
    productVariantId: string;
    supplierId: string;
    productName: string;
    imageUrl?: string | null;
    unitPrice: number;
    currency: string;
    quantity: number;
    lineTotal: number;
  }>;
  wishlistProductIds: string[];
  totalQuantity: number;
  subtotal: number;
  currency: string;
  removedUnavailableItems?: number;
};

export type PublicOrderSummary = {
  id: string;
  orderNumber: string;
  status: string;
  paymentStatus: string;
  paymentReference?: string | null;
  fulfillmentStatus: string;
  subtotal: number;
  currency: string;
  itemsCount: number;
  createdAtUtc: string;
};

export type PublicSupplierFulfillment = {
  status: string;
  itemsCount: number;
  productNames: string[];
};

export type PublicOrderDetail = {
  id: string;
  orderNumber: string;
  customerEmail: string;
  customerFullName: string;
  status: string;
  paymentStatus: string;
  paymentReference?: string | null;
  fulfillmentStatus: string;
  trackingStage: string;
  subtotal: number;
  currency: string;
  shippingAddress: {
    countryCode: string;
    city: string;
    line1: string;
    line2?: string | null;
    postalCode?: string | null;
    region?: string | null;
  };
  items: Array<{
    id: string;
    productName: string;
    supplierSku: string;
    imageUrl?: string | null;
    unitPrice: number;
    currency: string;
    quantity: number;
    lineTotal: number;
  }>;
  supplierFulfillments: PublicSupplierFulfillment[];
  createdAtUtc: string;
};
