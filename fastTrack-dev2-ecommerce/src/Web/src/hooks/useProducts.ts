import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createProduct,
  deleteProduct,
  getCategories,
  getProduct,
  getProducts,
  updateProduct,
  type ProductFilter,
} from "../api/catalogApi";
import type { ProductInput } from "../types";

export const useProducts = (filter: ProductFilter) =>
  useQuery({ queryKey: ["products", filter], queryFn: () => getProducts(filter), placeholderData: keepPreviousData });

export const useProduct = (id: string) => useQuery({ queryKey: ["product", id], queryFn: () => getProduct(id) });

export const useCategories = () =>
  useQuery({ queryKey: ["categories"], queryFn: getCategories, staleTime: 5 * 60_000 });

// Admin: any write refreshes every product list, product and category query.
export function useProductMutations() {
  const queryClient = useQueryClient();
  const invalidate = () =>
    Promise.all(["products", "product", "categories"].map((key) => queryClient.invalidateQueries({ queryKey: [key] })));

  return {
    create: useMutation({ mutationFn: (input: ProductInput) => createProduct(input), onSuccess: invalidate }),
    update: useMutation({
      mutationFn: ({ id, input }: { id: string; input: ProductInput }) => updateProduct(id, input),
      onSuccess: invalidate,
    }),
    remove: useMutation({ mutationFn: (id: string) => deleteProduct(id), onSuccess: invalidate }),
  };
}
