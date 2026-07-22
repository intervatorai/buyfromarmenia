export const AUTH_COOKIE_NAME = "bfa_admin_token";
export const AUTH_USER_KEY = "bfa_admin_user";

export type AdminUser = {
  adminId: string;
  email: string;
  fullName: string;
  role: string;
};

export type LoginResponse = {
  accessToken: string;
  expiresAt: string;
  adminId: string;
  email: string;
  fullName: string;
  role: string;
};

export function saveAuth(response: LoginResponse) {
  const user: AdminUser = {
    adminId: response.adminId,
    email: response.email,
    fullName: response.fullName,
    role: response.role,
  };

  localStorage.setItem(AUTH_USER_KEY, JSON.stringify(user));
  document.cookie = `${AUTH_COOKIE_NAME}=${response.accessToken}; path=/; max-age=${60 * 60 * 8}; SameSite=Lax`;
}

export function clearAuth() {
  localStorage.removeItem(AUTH_USER_KEY);
  document.cookie = `${AUTH_COOKIE_NAME}=; path=/; max-age=0; SameSite=Lax`;
}

export function getStoredUser(): AdminUser | null {
  if (typeof window === "undefined") {
    return null;
  }

  const raw = localStorage.getItem(AUTH_USER_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as AdminUser;
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
