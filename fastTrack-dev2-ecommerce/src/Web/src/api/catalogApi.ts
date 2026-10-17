import type { PaginatedResult, Product, ProductInput } from "../types";
import { http, json } from "./http";

const base = "/catalog-service/products";

export type ProductFilter = { pageIndex: number; pageSize: number; category?: string; search?: string };

export const getProducts = ({ pageIndex, pageSize, category, search }: ProductFilter) => {
  const params = new URLSearchParams({ pageIndex: String(pageIndex), pageSize: String(pageSize) });
  if (category) params.set("category", category);
  if (search) params.set("search", search);
  return http<PaginatedResult<Product>>(`${base}?${params}`);
};

export const getProduct = (id: string) => http<Product>(`${base}/${id}`);

export const getCategories = () => http<string[]>(`${base}/categories`);

export const createProduct = (input: ProductInput) => http<Product>(base, { method: "POST", ...json(input) });

export const updateProduct = (id: string, input: ProductInput) =>
  http<void>(`${base}/${id}`, { method: "PUT", ...json(input) });

export const deleteProduct = (id: string) => http<void>(`${base}/${id}`, { method: "DELETE" });
