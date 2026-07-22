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
  clearAuth,
  getStoredUser,
  getTokenFromCookie,
  saveAuth,
  type SupplierAuthResponse,
  type SupplierUser,
} from "@/lib/auth";

type AuthContextValue = {
  user: SupplierUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

type CurrentSupplierResponse = {
  userId: string;
  supplierId: string;
  email: string;
  fullName: string;
  tradingName: string;
  role: string;
};

export function AuthProvider({ children }: { children: ReactNode }) {
  const router = useRouter();
  const [user, setUser] = useState<SupplierUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function bootstrap() {
      const token = getTokenFromCookie();
      const storedUser = getStoredUser();

      if (!token || !storedUser) {
        clearAuth();
        setUser(null);
        setIsLoading(false);
        return;
      }

      try {
        const currentUser = await apiFetch<CurrentSupplierResponse>("/api/auth/me");
        setUser({
          userId: currentUser.userId,
          supplierId: currentUser.supplierId,
          email: currentUser.email,
          fullName: currentUser.fullName,
          tradingName: currentUser.tradingName,
          role: currentUser.role,
        });
      } catch {
        clearAuth();
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    }

    void bootstrap();
  }, []);

  const login = useCallback(
    async (email: string, password: string) => {
      const response = await apiFetch<SupplierAuthResponse>("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });

      saveAuth(response);
      setUser({
        userId: response.userId,
        supplierId: response.supplierId,
        email: response.email,
        fullName: response.fullName,
        tradingName: response.tradingName,
        role: response.role,
      });
      router.push("/");
    },
    [router],
  );

  const logout = useCallback(() => {
    clearAuth();
    setUser(null);
    router.push("/login");
  }, [router]);

  const value = useMemo(
    () => ({
      user,
      isLoading,
      isAuthenticated: Boolean(user),
      login,
      logout,
    }),
    [user, isLoading, login, logout],
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
