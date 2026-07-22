"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import {
  clearCustomerAuth,
  getCustomerTokenFromCookie,
  getStoredCustomer,
  saveCustomerAuth,
  type CustomerAuthResponse,
  type CustomerUser,
} from "@/lib/auth";

type AuthContextValue = {
  user: CustomerUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (
    email: string,
    password: string,
    fullName: string,
    phone?: string,
  ) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

type CurrentCustomerResponse = {
  userId: string;
  email: string;
  fullName: string;
  phone?: string | null;
};

export function AuthProvider({ children }: { children: ReactNode }) {
  const router = useRouter();
  const [user, setUser] = useState<CustomerUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function bootstrap() {
      const token = getCustomerTokenFromCookie();
      const storedUser = getStoredCustomer();

      if (!token || !storedUser) {
        clearCustomerAuth();
        setUser(null);
        setIsLoading(false);
        return;
      }

      try {
        const currentUser = await apiFetch<CurrentCustomerResponse>("/api/auth/me");
        setUser({
          userId: currentUser.userId,
          email: currentUser.email,
          fullName: currentUser.fullName,
          phone: currentUser.phone,
        });
      } catch {
        clearCustomerAuth();
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    }

    void bootstrap();
  }, []);

  const applyAuth = useCallback((response: CustomerAuthResponse) => {
    saveCustomerAuth(response);
    setUser({
      userId: response.userId,
      email: response.email,
      fullName: response.fullName,
      phone: response.phone,
    });
  }, []);

  const resolveReturnPath = useCallback(() => {
    const returnTo = new URLSearchParams(window.location.search).get("returnTo");
    return returnTo && returnTo.startsWith("/") ? returnTo : "/account";
  }, []);

  const login = useCallback(
    async (email: string, password: string) => {
      const response = await apiFetch<CustomerAuthResponse>("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });

      applyAuth(response);
      router.push(resolveReturnPath());
    },
    [applyAuth, router, resolveReturnPath],
  );

  const register = useCallback(
    async (email: string, password: string, fullName: string, phone?: string) => {
      const response = await apiFetch<CustomerAuthResponse>("/api/auth/register", {
        method: "POST",
        body: JSON.stringify({ email, password, fullName, phone: phone || null }),
      });

      applyAuth(response);
      router.push(resolveReturnPath());
    },
    [applyAuth, router, resolveReturnPath],
  );

  const logout = useCallback(() => {
    clearCustomerAuth();
    setUser(null);
    router.push("/");
  }, [router]);

  const value = useMemo(
    () => ({
      user,
      isLoading,
      isAuthenticated: Boolean(user),
      login,
      register,
      logout,
    }),
    [user, isLoading, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used within AuthProvider");
  }

  return context;
}

export function useAuthOptional() {
  return useContext(AuthContext);
}
