import { ApiError } from "./api";

const SUPPLIER_API_URL =
  process.env.NEXT_PUBLIC_SUPPLIER_API_URL ?? "http://localhost:5102";

export async function supplierApiFetch<T>(
  path: string,
  options: RequestInit = {},
  accessToken?: string,
): Promise<T> {
  const headers = new Headers(options.headers);

  if (!headers.has("Content-Type") && options.body) {
    headers.set("Content-Type", "application/json");
  }

  if (accessToken) {
    headers.set("Authorization", `Bearer ${accessToken}`);
  }

  const response = await fetch(`${SUPPLIER_API_URL}${path}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    let message = response.statusText;
    try {
      const payload = (await response.json()) as { message?: string };
      if (payload.message) {
        message = payload.message;
      }
    } catch {
      const text = await response.text().catch(() => "");
      if (text) {
        message = text;
      }
    }
    throw new ApiError(message || response.statusText, response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export type SupplierRegisterResponse = {
  supplierId: string;
  accessToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  fullName: string;
  tradingName: string;
  role: string;
};
