import type { Basket, CheckoutRequest } from "../types";
import { http, json } from "./http";

const base = "/basket-service/basket";

export type BasketLine = { productId: string; quantity: number };

export const getBasket = () => http<Basket>(base);

// Only ids and quantities are sent; Basket recomputes prices from Catalog + Discount.
export const storeBasket = (items: BasketLine[]) => http<Basket>(base, { method: "POST", ...json({ items }) });

export const clearBasket = () => http<void>(base, { method: "DELETE" });

export const checkout = (request: CheckoutRequest) =>
  http<{ checkoutId: string }>(`${base}/checkout`, { method: "POST", ...json(request) });
