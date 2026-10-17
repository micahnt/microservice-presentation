import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { ApiError } from "../api/http";
import { register } from "../api/identityApi";
import { useAuth } from "../auth/AuthContext";
import { ErrorMessage } from "../components/common";

type Form = { firstName: string; lastName: string; email: string; password: string };

export function RegisterPage() {
  const auth = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState<Form>({ firstName: "", lastName: "", email: "", password: "" });
  const [error, setError] = useState<unknown>(null);
  const [busy, setBusy] = useState(false);
  const apiError = error instanceof ApiError ? error : null;

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setBusy(true);
    setError(null);
    try {
      const { token } = await register(form);
      auth.login(token);
      navigate("/", { replace: true });
    } catch (err) {
      setError(err);
    } finally {
      setBusy(false);
    }
  };

  const input = (name: keyof Form, label: string, type = "text", autoComplete?: string) => (
    <label>
      {label}
      <input
        type={type}
        required
        autoComplete={autoComplete}
        value={form[name]}
        onChange={(e) => setForm({ ...form, [name]: e.target.value })}
      />
      {apiError?.field(name) && <span className="field-error">{apiError.field(name)}</span>}
    </label>
  );

  return (
    <div className="auth-page">
      <form className="card form" onSubmit={submit}>
        <h1>Create an account</h1>
        <div className="form-grid">
          {input("firstName", "First name", "text", "given-name")}
          {input("lastName", "Last name", "text", "family-name")}
        </div>
        {input("email", "Email", "email", "email")}
        {input("password", "Password", "password", "new-password")}
        <p className="muted small">At least 6 characters, with an uppercase letter, a lowercase letter and a digit.</p>
        {apiError?.fieldErrors ? null : <ErrorMessage error={error} />}
        <button className="btn btn-primary btn-block" disabled={busy}>
          {busy ? "Creating account..." : "Register"}
        </button>
        <p className="muted">
          Already registered? <Link to="/login">Sign in</Link>
        </p>
      </form>
    </div>
  );
}
