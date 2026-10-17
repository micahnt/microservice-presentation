export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
    public fieldErrors?: Record<string, string[]>,
  ) {
    super(message);
  }

  // Validation errors come back keyed by property name ("Email"); look them up case-insensitively.
  field(name: string): string | undefined {
    const key = Object.keys(this.fieldErrors ?? {}).find((k) => k.toLowerCase() === name.toLowerCase());
    return key ? this.fieldErrors![key][0] : undefined;
  }
}

const TOKEN_KEY = "ecommerce.token";
export const tokenStore = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
};

// Every API call goes through the gateway prefix (/catalog-service/...), same origin as the UI.
export async function http<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = tokenStore.get();
  const res = await fetch(path, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init.headers,
    },
  });

  if (res.status === 401 && token) window.dispatchEvent(new Event("auth:expired"));
  if (!res.ok) {
    const problem = await res.json().catch(() => ({})); // ProblemDetails / ValidationProblemDetails
    const message =
      res.status === 429 ? "Too many requests, please wait a moment." : (problem.title ?? res.statusText);
    throw new ApiError(res.status, message, problem.errors);
  }
  if (res.status === 204 || res.headers.get("content-length") === "0") return undefined as T;
  return res.json();
}

export const json = (body: unknown): RequestInit => ({ body: JSON.stringify(body) });
