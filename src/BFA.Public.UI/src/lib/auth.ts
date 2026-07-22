export const AUTH_COOKIE_NAME = "bfa_customer_token";
export const AUTH_USER_KEY = "bfa_customer_user";

export type CustomerUser = {
  userId: string;
  email: string;
  fullName: string;
  phone?: string | null;
};

export type CustomerDeliveryAddress = {
  id: string;
  label: string;
  countryCode: string;
  city: string;
  line1: string;
  line2?: string | null;
  postalCode?: string | null;
  region?: string | null;
  isDefault: boolean;
};

export type CustomerAuthResponse = {
  accessToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  fullName: string;
  phone?: string | null;
};

export function saveCustomerAuth(response: CustomerAuthResponse) {
  const user: CustomerUser = {
    userId: response.userId,
    email: response.email,
    fullName: response.fullName,
    phone: response.phone,
  };

  localStorage.setItem(AUTH_USER_KEY, JSON.stringify(user));
  document.cookie = `${AUTH_COOKIE_NAME}=${response.accessToken}; path=/; max-age=${60 * 60 * 24 * 7}; SameSite=Lax`;
}

export function clearCustomerAuth() {
  localStorage.removeItem(AUTH_USER_KEY);
  document.cookie = `${AUTH_COOKIE_NAME}=; path=/; max-age=0; SameSite=Lax`;
}

export function getStoredCustomer(): CustomerUser | null {
  if (typeof window === "undefined") {
    return null;
  }

  const raw = localStorage.getItem(AUTH_USER_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as CustomerUser;
  } catch {
    return null;
  }
}

export function getCustomerTokenFromCookie(): string | null {
  if (typeof document === "undefined") {
    return null;
  }

  const match = document.cookie
    .split("; ")
    .find((item) => item.startsWith(`${AUTH_COOKIE_NAME}=`));

  return match ? match.split("=")[1] : null;
}
