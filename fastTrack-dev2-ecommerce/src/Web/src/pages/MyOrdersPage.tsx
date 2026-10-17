import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { ErrorMessage, Loader, money, paymentLabel, StatusBadge } from "../components/common";
import { useMyOrders } from "../hooks/useOrders";
import type { Order } from "../types";

export function MyOrdersPage() {
  const checkoutId = (useLocation().state as { checkoutId?: string } | null)?.checkoutId;
  const [timedOut, setTimedOut] = useState(false);
  const [polling, setPolling] = useState(!!checkoutId);
  const { data: orders, isLoading, error } = useMyOrders(polling);
  const placed = orders?.find((o) => o.checkoutId === checkoutId);

  // After a checkout, poll until the order created from it arrives (or give up after ~15 s).
  useEffect(() => {
    if (placed) setPolling(false);
  }, [placed]);

  useEffect(() => {
    if (!polling) return;
    const t = setTimeout(() => {
      setPolling(false);
      setTimedOut(true);
    }, 15_000);
    return () => clearTimeout(t);
  }, [polling]);

  if (isLoading) return <Loader />;

  return (
    <>
      <h1>My orders</h1>
      {checkoutId && (
        <div className={placed ? "alert alert-success" : "alert alert-info"}>
          {placed
            ? `Thanks! Order ${placed.orderName} has been placed.`
            : timedOut
              ? "Your order is still being processed. Refresh this page in a moment."
              : "Thanks! Your order is being created..."}
        </div>
      )}
      <ErrorMessage error={error} />
      {orders && orders.length === 0 && !polling && (
        <p className="empty">
          You have no orders yet. <Link to="/">Start shopping</Link>
        </p>
      )}
      <div className="orders">
        {orders?.map((o) => <OrderCard key={o.id} order={o} />)}
      </div>
    </>
  );
}

function OrderCard({ order }: { order: Order }) {
  return (
    <article className="card order">
      <header className="order-header">
        <div>
          <h3>{order.orderName}</h3>
          <span className="muted">{order.createdAt ? new Date(order.createdAt).toLocaleString() : ""}</span>
        </div>
        <StatusBadge status={order.status} />
      </header>
      <table className="table compact">
        <tbody>
          {order.orderItems.map((i) => (
            <tr key={i.productId}>
              <td>
                {i.quantity} × {i.productName}
              </td>
              <td className="right">{money(i.price * i.quantity)}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <footer className="order-footer">
        <span className="muted">
          {paymentLabel(order.payment.method)} · Ship to {order.shippingAddress.addressLine},{" "}
          {order.shippingAddress.state}
        </span>
        <strong>{money(order.totalPrice)}</strong>
      </footer>
    </article>
  );
}
