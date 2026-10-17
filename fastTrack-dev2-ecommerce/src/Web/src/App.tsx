import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { ApiError } from "./api/http";
import { AuthProvider } from "./auth/AuthContext";
import { RequireRole } from "./auth/RequireRole";
import { Layout } from "./components/Layout";
import { AdminOrdersPage } from "./pages/admin/AdminOrdersPage";
import { AdminProductsPage } from "./pages/admin/AdminProductsPage";
import { BasketPage } from "./pages/BasketPage";
import { CheckoutPage } from "./pages/CheckoutPage";
import { LoginPage } from "./pages/LoginPage";
import { MyOrdersPage } from "./pages/MyOrdersPage";
import { ProductDetailPage } from "./pages/ProductDetailPage";
import { ProductListPage } from "./pages/ProductListPage";
import { RegisterPage } from "./pages/RegisterPage";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Don't retry client errors (401/403/404/...): they won't fix themselves.
      retry: (count, error) => !(error instanceof ApiError && error.status < 500) && count < 2,
      refetchOnWindowFocus: false,
    },
  },
});

// Never start a route with "*-service": those paths belong to the gateway's API routes.
export default function App() {
  return (
    <AuthProvider>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <Layout>
            <Routes>
              <Route path="/" element={<ProductListPage />} />
              <Route path="/products/:id" element={<ProductDetailPage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/basket" element={<RequireRole><BasketPage /></RequireRole>} />
              <Route path="/checkout" element={<RequireRole><CheckoutPage /></RequireRole>} />
              <Route path="/orders" element={<RequireRole><MyOrdersPage /></RequireRole>} />
              <Route path="/admin/products" element={<RequireRole role="Admin"><AdminProductsPage /></RequireRole>} />
              <Route path="/admin/orders" element={<RequireRole role="Admin"><AdminOrdersPage /></RequireRole>} />
              <Route path="*" element={<p className="empty">Page not found.</p>} />
            </Routes>
          </Layout>
        </BrowserRouter>
      </QueryClientProvider>
    </AuthProvider>
  );
}
