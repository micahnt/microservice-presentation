import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { deleteOrder, getMyOrders, getOrders, updateOrderStatus } from "../api/orderingApi";
import { useAuth } from "../auth/AuthContext";
import type { OrderStatus } from "../types";

// Orders are created asynchronously (Basket -> RabbitMQ -> Ordering), so right after a checkout
// the page polls until the new order shows up.
export function useMyOrders(poll: boolean) {
  const { user } = useAuth();
  return useQuery({
    queryKey: ["orders", "mine", user?.id],
    queryFn: getMyOrders,
    enabled: !!user,
    refetchInterval: poll ? 1500 : false,
  });
}

export const useAllOrders = (pageIndex: number, pageSize: number, name: string) =>
  useQuery({
    queryKey: ["orders", "all", pageIndex, pageSize, name],
    queryFn: () => getOrders(pageIndex, pageSize, name || undefined),
    placeholderData: keepPreviousData,
  });

export function useOrderMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["orders"] });
  return {
    setStatus: useMutation({
      mutationFn: ({ id, status }: { id: string; status: OrderStatus }) => updateOrderStatus(id, status),
      onSuccess: invalidate,
    }),
    remove: useMutation({ mutationFn: (id: string) => deleteOrder(id), onSuccess: invalidate }),
  };
}
