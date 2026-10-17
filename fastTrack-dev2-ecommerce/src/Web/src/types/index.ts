// TS models mirroring the API DTOs (JSON is camelCase).

export type PaginatedResult<T> = {
  pageIndex: number;
  pageSize: number;
  count: number;
  data: T[];
};

export type Product = {
  id: string;
  name: string;
  category: string[];
  description: string;
  imageFile: string;
  price: number;
  stock: number;
};

export type ProductInput = Omit<Product, "id">;

export type BasketItem = {
  productId: string;
  productName: string;
  imageFile: string;
  quantity: number;
  unitPrice: number;
  discount: number;
  price: number;
};

export type Basket = {
  userId: string;
  items: BasketItem[];
  totalPrice: number;
};

export type PaymentMethod = "CashOnDelivery" | "FakeCard";

export type CheckoutRequest = {
  firstName: string;
  lastName: string;
  addressLine: string;
  country: string;
  state: string;
  zipCode: string;
  paymentMethod: PaymentMethod;
};

export type OrderStatus = "Pending" | "Paid" | "PaymentFailed" | "Shipped" | "Completed" | "Cancelled";

export type Address = {
  firstName: string;
  lastName: string;
  emailAddress: string;
  addressLine: string;
  country: string;
  state: string;
  zipCode: string;
};

export type Order = {
  id: string;
  customerId: string;
  orderName: string;
  shippingAddress: Address;
  billingAddress: Address;
  payment: { method: PaymentMethod; transactionId: string | null };
  status: OrderStatus;
  totalPrice: number;
  createdAt: string | null;
  checkoutId: string | null;
  orderItems: { productId: string; productName: string; quantity: number; price: number }[];
};
