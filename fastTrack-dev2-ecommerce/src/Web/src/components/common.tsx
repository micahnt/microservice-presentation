import { ApiError } from "../api/http";
import type { OrderStatus } from "../types";

export const money = (value: number) =>
  value.toLocaleString("en-US", { style: "currency", currency: "USD" });

export const imageUrl = (imageFile: string) => `/images/product/${imageFile}`;

// Falls back to a placeholder when an admin enters an image file that doesn't exist.
export function ProductImage({ file, alt, className }: { file: string; alt: string; className?: string }) {
  return (
    <img
      className={className}
      src={imageUrl(file)}
      alt={alt}
      onError={(e) => {
        e.currentTarget.onerror = null;
        e.currentTarget.src = "/images/placeholder.png";
      }}
    />
  );
}

export function Loader({ label = "Loading..." }: { label?: string }) {
  return (
    <div className="loader" role="status">
      <span className="spinner" aria-hidden /> {label}
    </div>
  );
}

export function ErrorMessage({ error }: { error: unknown }) {
  if (!error) return null;
  const message = error instanceof Error ? error.message : "Something went wrong.";
  const fields = error instanceof ApiError && error.fieldErrors ? Object.values(error.fieldErrors).flat() : [];
  return (
    <div className="alert alert-error" role="alert">
      <strong>{message}</strong>
      {fields.length > 0 && (
        <ul>
          {fields.map((f) => (
            <li key={f}>{f}</li>
          ))}
        </ul>
      )}
    </div>
  );
}

const statusLabels: Record<OrderStatus, string> = {
  Pending: "Pending",
  Paid: "Paid",
  PaymentFailed: "Payment failed",
  Shipped: "Shipped",
  Completed: "Completed",
  Cancelled: "Cancelled",
};

export function StatusBadge({ status }: { status: OrderStatus }) {
  return <span className={`badge badge-${status.toLowerCase()}`}>{statusLabels[status]}</span>;
}

export const paymentLabel = (method: string) => (method === "FakeCard" ? "Card (demo)" : "Cash on delivery");
