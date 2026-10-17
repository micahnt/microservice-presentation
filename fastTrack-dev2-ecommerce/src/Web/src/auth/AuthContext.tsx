import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { jwtDecode } from "jwt-decode";
import { tokenStore } from "../api/http";

export type User = { id: string; email: string; name: string; roles: string[] };

type AuthState = {
  token: string | null;
  user: User | null;
  isAdmin: boolean;
  login: (token: string) => void;
  logout: () => void;
};

type Claims = { sub: string; email: string; name?: string; role?: string | string[]; exp: number };

// Decoding is for display only: every service still validates the token and the roles.
function toUser(token: string): User | null {
  try {
    const c = jwtDecode<Claims>(token);
    if (c.exp * 1000 < Date.now()) return null;
    const roles = Array.isArray(c.role) ? c.role : c.role ? [c.role] : [];
    return { id: c.sub, email: c.email, name: c.name || c.email, roles };
  } catch {
    return null;
  }
}

const AuthContext = createContext<AuthState>(null!);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => tokenStore.get());
  const user = useMemo(() => (token ? toUser(token) : null), [token]);

  const login = useCallback((t: string) => {
    tokenStore.set(t);
    setToken(t);
  }, []);

  const logout = useCallback(() => {
    tokenStore.clear();
    setToken(null);
  }, []);

  // http() fires this on a 401 (expired token): send the user back to login.
  useEffect(() => {
    window.addEventListener("auth:expired", logout);
    return () => window.removeEventListener("auth:expired", logout);
  }, [logout]);

  const value = useMemo(
    () => ({ token: user ? token : null, user, isAdmin: !!user?.roles.includes("Admin"), login, logout }),
    [token, user, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export const useAuth = () => useContext(AuthContext);
