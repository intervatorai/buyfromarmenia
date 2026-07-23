import { getCustomerTokenFromCookie } from "./auth";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5100";

export class ApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

export async function apiFetch<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
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
    const text = await response.text();
    let message = text || response.statusText;
    try {
      const parsed = JSON.parse(text) as { error?: string; message?: string; title?: string };
      message = parsed.error || parsed.message || parsed.title || message;
    } catch {
      // keep raw text
    }
    throw new ApiError(message, response.status);
  }

  if (response.status === 204) {
    return undefined as T;
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
