export const AUTH_COOKIE_NAME = "bfa_supplier_token";
export const AUTH_USER_KEY = "bfa_supplier_user";

export type SupplierUser = {
  userId: string;
  supplierId: string;
  email: string;
  fullName: string;
  tradingName: string;
  role: string;
};

export type SupplierAuthResponse = {
  accessToken: string;
  expiresAt: string;
  userId: string;
  supplierId: string;
  email: string;
  fullName: string;
  tradingName: string;
  role: string;
};

export function saveAuth(response: SupplierAuthResponse) {
  const user: SupplierUser = {
    userId: response.userId,
    supplierId: response.supplierId,
    email: response.email,
    fullName: response.fullName,
    tradingName: response.tradingName,
    role: response.role,
  };

  localStorage.setItem(AUTH_USER_KEY, JSON.stringify(user));
  localStorage.setItem("bfa_supplier_id", response.supplierId);
  document.cookie = `${AUTH_COOKIE_NAME}=${response.accessToken}; path=/; max-age=${60 * 60 * 24 * 7}; SameSite=Lax`;
}

export function clearAuth() {
  localStorage.removeItem(AUTH_USER_KEY);
  localStorage.removeItem("bfa_supplier_id");
  document.cookie = `${AUTH_COOKIE_NAME}=; path=/; max-age=0; SameSite=Lax`;
}

export function getStoredUser(): SupplierUser | null {
  if (typeof window === "undefined") {
    return null;
  }

  const raw = localStorage.getItem(AUTH_USER_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as SupplierUser;
  } catch {
    return null;
  }
}

export function getTokenFromCookie(): string | null {
  if (typeof document === "undefined") {
    return null;
  }

  const match = document.cookie
    .split("; ")
    .find((item) => item.startsWith(`${AUTH_COOKIE_NAME}=`));

  return match ? match.split("=")[1] : null;
}
