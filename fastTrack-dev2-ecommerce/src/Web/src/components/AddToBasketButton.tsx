import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useBasketActions } from "../hooks/useBasket";
import type { Product } from "../types";

export function AddToBasketButton({ product, quantity = 1 }: { product: Product; quantity?: number }) {
  const { user } = useAuth();
  const { add } = useBasketActions();
  const navigate = useNavigate();
  const location = useLocation();
  const [state, setState] = useState<"idle" | "adding" | "added">("idle");
  const [error, setError] = useState<string | null>(null);

  const onClick = async () => {
    if (!user) {
      navigate("/login", { state: { from: location } });
      return;
    }
    setError(null);
    setState("adding");
    try {
      await add(product.id, quantity);
      setState("added");
      setTimeout(() => setState("idle"), 1500);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Could not add to basket.");
      setState("idle");
    }
  };

  return (
    <div className="add-to-basket">
      <button className="btn btn-primary" disabled={product.stock === 0 || state === "adding"} onClick={onClick}>
        {state === "adding" ? "Adding..." : state === "added" ? "Added ✓" : "Add to basket"}
      </button>
      {error && <p className="field-error">{error}</p>}
    </div>
  );
}
