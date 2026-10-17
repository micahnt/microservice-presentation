import type { ReactElement } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";

// Signed-in users only; with role="Admin", admins only. The server enforces the same rules.
export function RequireRole({ role, children }: { role?: "Admin"; children: ReactElement }) {
  const { user } = useAuth();
  const location = useLocation();

  if (!user) return <Navigate to="/login" state={{ from: location }} replace />;
  if (role && !user.roles.includes(role)) return <Navigate to="/" replace />;
  return children;
}
