import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { checkout, clearBasket, getBasket, storeBasket, type BasketLine } from "../api/basketApi";
import { useAuth } from "../auth/AuthContext";
import type { Basket, CheckoutRequest } from "../types";

const basketKey = (userId?: string) => ["basket", userId];

export function useBasket() {
  const { user } = useAuth();
  return useQuery({ queryKey: basketKey(user?.id), queryFn: getBasket, enabled: !!user });
}

// The API stores the whole basket at once: build the new list of lines and send it.
export function useBasketActions() {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const key = basketKey(user?.id);

  const currentLines = async (): Promise<BasketLine[]> => {
    const basket: Basket =
      queryClient.getQueryData<Basket>(key) ?? (await queryClient.fetchQuery<Basket>({ queryKey: key, queryFn: getBasket }));
    return basket.items.map((i) => ({ productId: i.productId, quantity: i.quantity }));
  };

  const save = useMutation({
    mutationFn: storeBasket,
    onSuccess: (basket) => queryClient.setQueryData(key, basket),
  });

  const add = async (productId: string, quantity = 1) => {
    const lines = await currentLines();
    const existing = lines.find((l) => l.productId === productId);
    if (existing) existing.quantity += quantity;
    else lines.push({ productId, quantity });
    return save.mutateAsync(lines);
  };

  const setQuantity = async (productId: string, quantity: number) => {
    const lines = (await currentLines())
      .map((l) => (l.productId === productId ? { ...l, quantity } : l))
      .filter((l) => l.quantity > 0);
    return save.mutateAsync(lines);
  };

  const clear = useMutation({
    mutationFn: clearBasket,
    onSuccess: () => queryClient.setQueryData<Basket>(key, (b) => (b ? { ...b, items: [], totalPrice: 0 } : b)),
  });

  const placeOrder = useMutation({
    mutationFn: (request: CheckoutRequest) => checkout(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: key });
      queryClient.invalidateQueries({ queryKey: ["orders"] });
    },
  });

  return { add, setQuantity, saving: save.isPending, saveError: save.error, clear, placeOrder };
}
