import { getTokenFromCookie } from "./auth";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5102";

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
  const token = getTokenFromCookie();
  const headers = new Headers(options.headers);

  if (!headers.has("Content-Type") && options.body && !(options.body instanceof FormData)) {
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

export type MediaUploadResult = {
  mediaAssetId?: string | null;
  storageKey: string;
  url: string;
  productMediaId?: string | null;
};

export async function uploadMedia(
  file: File,
  fields: Record<string, string> = {},
): Promise<MediaUploadResult> {
  const body = new FormData();
  body.append("file", file);
  for (const [key, value] of Object.entries(fields)) {
    body.append(key, value);
  }

  return apiFetch<MediaUploadResult>("/api/media/upload", {
    method: "POST",
    body,
  });
}
