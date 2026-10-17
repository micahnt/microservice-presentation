import { http, json } from "./http";

type AuthResponse = { token: string };

export const login = (email: string, password: string) =>
  http<AuthResponse>("/identity-service/auth/login", { method: "POST", ...json({ email, password }) });

export const register = (input: { email: string; password: string; firstName: string; lastName: string }) =>
  http<AuthResponse>("/identity-service/auth/register", { method: "POST", ...json(input) });
