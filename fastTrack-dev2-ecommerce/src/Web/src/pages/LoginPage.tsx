import { useState, type FormEvent } from "react";
import { Link, useLocation, useNavigate, type Location } from "react-router-dom";
import { login } from "../api/identityApi";
import { useAuth } from "../auth/AuthContext";
import { ErrorMessage } from "../components/common";

export function LoginPage() {
  const auth = useAuth();
  const navigate = useNavigate();
  const from = (useLocation().state as { from?: Location } | null)?.from?.pathname ?? "/";
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<unknown>(null);
  const [busy, setBusy] = useState(false);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setBusy(true);
    setError(null);
    try {
      const { token } = await login(email, password);
      auth.login(token);
      navigate(from, { replace: true });
    } catch (err) {
      setError(err);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="auth-page">
      <form className="card form" onSubmit={submit}>
        <h1>Sign in</h1>
        <label>
          Email
          <input type="email" required autoComplete="email" value={email} onChange={(e) => setEmail(e.target.value)} />
        </label>
        <label>
          Password
          <input
            type="password"
            required
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </label>
        <ErrorMessage error={error} />
        <button className="btn btn-primary btn-block" disabled={busy}>
          {busy ? "Signing in..." : "Sign in"}
        </button>
        <p className="muted">
          No account? <Link to="/register">Register</Link>
        </p>
        <p className="muted small">
          Demo accounts: <code>admin@shop.local / Admin123</code>, <code>customer@shop.local / Customer123</code>
        </p>
      </form>
    </div>
  );
}
