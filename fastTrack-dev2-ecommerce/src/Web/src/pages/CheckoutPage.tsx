import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { ApiError } from "../api/http";
import { useAuth } from "../auth/AuthContext";
import { ErrorMessage, Loader, money } from "../components/common";
import { useBasket, useBasketActions } from "../hooks/useBasket";
import type { CheckoutRequest } from "../types";

const fields: { name: keyof Omit<CheckoutRequest, "paymentMethod">; label: string }[] = [
  { name: "firstName", label: "First name" },
  { name: "lastName", label: "Last name" },
  { name: "addressLine", label: "Address" },
  { name: "country", label: "Country" },
  { name: "state", label: "State / city" },
  { name: "zipCode", label: "ZIP code" },
];

export function CheckoutPage() {
  const { user } = useAuth();
  const { data: basket, isLoading } = useBasket();
  const { placeOrder } = useBasketActions();
  const navigate = useNavigate();
  const [first, ...rest] = (user?.name ?? "").split(" ");
  const [form, setForm] = useState<CheckoutRequest>({
    firstName: first ?? "",
    lastName: rest.join(" "),
    addressLine: "",
    country: "",
    state: "",
    zipCode: "",
    paymentMethod: "CashOnDelivery",
  });

  if (isLoading) return <Loader />;
  if (!basket || basket.items.length === 0)
    return (
      <div className="empty-state">
        <h1>Nothing to check out</h1>
        <Link to="/" className="btn btn-primary">
          Browse products
        </Link>
      </div>
    );

  const error = placeOrder.error instanceof ApiError ? placeOrder.error : null;
  const submit = (e: FormEvent) => {
    e.preventDefault();
    placeOrder.mutate(form, {
      onSuccess: ({ checkoutId }) => navigate("/orders", { state: { checkoutId } }),
    });
  };

  return (
    <>
      <h1>Checkout</h1>
      <div className="basket">
        <form className="card form" onSubmit={submit}>
          <h2>Shipping address</h2>
          <div className="form-grid">
            {fields.map((f) => (
              <label key={f.name}>
                {f.label}
                <input
                  required
                  value={form[f.name]}
                  onChange={(e) => setForm({ ...form, [f.name]: e.target.value })}
                />
                {error?.field(f.name) && <span className="field-error">{error.field(f.name)}</span>}
              </label>
            ))}
          </div>

          <h2>Payment</h2>
          <div className="radio-group">
            <label className="radio">
              <input
                type="radio"
                checked={form.paymentMethod === "CashOnDelivery"}
                onChange={() => setForm({ ...form, paymentMethod: "CashOnDelivery" })}
              />
              Cash on delivery
            </label>
            <label className="radio">
              <input
                type="radio"
                checked={form.paymentMethod === "FakeCard"}
                onChange={() => setForm({ ...form, paymentMethod: "FakeCard" })}
              />
              Card (demo payment)
            </label>
          </div>
          {form.paymentMethod === "FakeCard" && (
            // For the look only: these inputs have no name and their values are never sent.
            <div className="fake-card">
              <p className="muted">
                Demo payment: card details are not sent anywhere. Orders over $5,000 are declined so you can see a
                failed payment.
              </p>
              <div className="form-grid">
                <label>
                  Card number
                  <input placeholder="4242 4242 4242 4242" autoComplete="off" />
                </label>
                <label>
                  Expiry
                  <input placeholder="12/30" autoComplete="off" />
                </label>
              </div>
            </div>
          )}

          <ErrorMessage error={placeOrder.error} />
          <button className="btn btn-primary btn-block" disabled={placeOrder.isPending}>
            {placeOrder.isPending ? "Placing order..." : `Place order · ${money(basket.totalPrice)}`}
          </button>
        </form>

        <aside className="card summary">
          <h2>Your order</h2>
          {basket.items.map((i) => (
            <div key={i.productId} className="summary-row">
              <span>
                {i.quantity} × {i.productName}
              </span>
              <span>{money(i.price * i.quantity)}</span>
            </div>
          ))}
          <div className="summary-row total">
            <span>Total</span>
            <span>{money(basket.totalPrice)}</span>
          </div>
        </aside>
      </div>
    </>
  );
}
