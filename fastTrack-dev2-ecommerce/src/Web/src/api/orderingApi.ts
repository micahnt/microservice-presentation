import type { Order, OrderStatus, PaginatedResult } from "../types";
import { http, json } from "./http";

const base = "/ordering-service/orders";

export const getMyOrders = () => http<Order[]>(`${base}/mine`);

export const getOrders = (pageIndex: number, pageSize: number, name?: string) => {
  const params = new URLSearchParams({ pageIndex: String(pageIndex), pageSize: String(pageSize) });
  if (name) params.set("name", name);
  return http<PaginatedResult<Order>>(`${base}?${params}`);
};

export const updateOrderStatus = (id: string, status: OrderStatus) =>
  http<void>(`${base}/${id}/status`, { method: "PUT", ...json({ status }) });

export const deleteOrder = (id: string) => http<void>(`${base}/${id}`, { method: "DELETE" });
