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
  type AdminUser,
  type LoginResponse,
} from "@/lib/auth";

type AuthContextValue = {
  user: AdminUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const router = useRouter();
  const [user, setUser] = useState<AdminUser | null>(null);
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
        const currentUser = await apiFetch<AdminUser & { id: string }>(
          "/api/auth/me",
        );

        setUser({
          adminId: currentUser.id,
          email: currentUser.email,
          fullName: currentUser.fullName,
          role: currentUser.role,
        });
      } catch {
        clearAuth();
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    }

    bootstrap();
  }, []);

  const login = useCallback(
    async (email: string, password: string) => {
      const response = await apiFetch<LoginResponse>("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });

      saveAuth(response);
      setUser({
        adminId: response.adminId,
        email: response.email,
        fullName: response.fullName,
        role: response.role,
      });
      router.push("/dashboard");
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
