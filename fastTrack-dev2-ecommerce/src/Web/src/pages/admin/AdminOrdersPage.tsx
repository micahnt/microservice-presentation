import { Fragment, useState } from "react";
import { ErrorMessage, Loader, money, paymentLabel, StatusBadge } from "../../components/common";
import { useAllOrders, useOrderMutations } from "../../hooks/useOrders";
import type { Order, OrderStatus } from "../../types";

const pageSize = 10;

// Mirrors the domain rules in Order.Ship/Complete/Cancel; the server still has the final say.
const canShip = (o: Order) =>
  o.status === "Paid" || (o.status === "Pending" && o.payment.method === "CashOnDelivery");
const canComplete = (o: Order) => o.status === "Shipped";
const canCancel = (o: Order) => !["Shipped", "Completed", "Cancelled"].includes(o.status);

export function AdminOrdersPage() {
  const [pageIndex, setPageIndex] = useState(0);
  const [nameInput, setNameInput] = useState("");
  const [name, setName] = useState("");
  const [open, setOpen] = useState<string | null>(null);
  const { data, isLoading, error } = useAllOrders(pageIndex, pageSize, name);
  const { setStatus, remove } = useOrderMutations();
  const pages = data ? Math.max(1, Math.ceil(data.count / pageSize)) : 1;
  const busy = setStatus.isPending || remove.isPending;

  const change = (id: string, status: OrderStatus) => setStatus.mutate({ id, status });
  const onDelete = (o: Order) => {
    if (confirm(`Delete order ${o.orderName}?`)) remove.mutate(o.id);
  };

  return (
    <>
      <div className="page-header">
        <h1>Manage orders</h1>
        <form
          className="search"
          onSubmit={(e) => {
            e.preventDefault();
            setPageIndex(0);
            setName(nameInput.trim());
          }}
        >
          <input
            type="search"
            placeholder="Order number"
            value={nameInput}
            onChange={(e) => setNameInput(e.target.value)}
            aria-label="Filter by order number"
          />
          <button className="btn">Filter</button>
        </form>
      </div>

      <ErrorMessage error={error ?? setStatus.error ?? remove.error} />
      {isLoading && <Loader />}
      {data && data.data.length === 0 && <p className="empty">No orders found.</p>}
      {data && data.data.length > 0 && (
        <div className="card">
          <table className="table">
            <thead>
              <tr>
                <th>Order</th>
                <th>Customer</th>
                <th>Placed</th>
                <th>Payment</th>
                <th>Status</th>
                <th className="right">Total</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {data.data.map((o) => (
                <Fragment key={o.id}>
                  <tr>
                    <td>
                      <button className="link" onClick={() => setOpen(open === o.id ? null : o.id)}>
                        {open === o.id ? "▾" : "▸"} {o.orderName}
                      </button>
                    </td>
                    <td>
                      {o.shippingAddress.firstName} {o.shippingAddress.lastName}
                      <div className="muted small">{o.shippingAddress.emailAddress}</div>
                    </td>
                    <td>{o.createdAt ? new Date(o.createdAt).toLocaleString() : "—"}</td>
                    <td>
                      {paymentLabel(o.payment.method)}
                      {o.payment.transactionId && <div className="muted small">{o.payment.transactionId}</div>}
                    </td>
                    <td>
                      <StatusBadge status={o.status} />
                    </td>
                    <td className="right">{money(o.totalPrice)}</td>
                    <td className="right nowrap">
                      <button className="btn btn-sm" disabled={busy || !canShip(o)} onClick={() => change(o.id, "Shipped")}>
                        Ship
                      </button>{" "}
                      <button
                        className="btn btn-sm"
                        disabled={busy || !canComplete(o)}
                        onClick={() => change(o.id, "Completed")}
                      >
                        Complete
                      </button>{" "}
                      <button
                        className="btn btn-sm"
                        disabled={busy || !canCancel(o)}
                        onClick={() => change(o.id, "Cancelled")}
                      >
                        Cancel
                      </button>{" "}
                      <button className="btn btn-sm btn-danger" disabled={busy} onClick={() => onDelete(o)}>
                        Delete
                      </button>
                    </td>
                  </tr>
                  {open === o.id && (
                    <tr className="details">
                      <td colSpan={7}>
                        <ul>
                          {o.orderItems.map((i) => (
                            <li key={i.productId}>
                              {i.quantity} × {i.productName} — {money(i.price)} each
                            </li>
                          ))}
                        </ul>
                        <span className="muted">
                          Ship to {o.shippingAddress.addressLine}, {o.shippingAddress.state} {o.shippingAddress.zipCode},{" "}
                          {o.shippingAddress.country}
                        </span>
                      </td>
                    </tr>
                  )}
                </Fragment>
              ))}
            </tbody>
          </table>
        </div>
      )}
      {pages > 1 && (
        <div className="pager">
          <button className="btn" disabled={pageIndex === 0} onClick={() => setPageIndex(pageIndex - 1)}>
            Previous
          </button>
          <span>
            Page {pageIndex + 1} of {pages}
          </span>
          <button className="btn" disabled={pageIndex + 1 >= pages} onClick={() => setPageIndex(pageIndex + 1)}>
            Next
          </button>
        </div>
      )}
    </>
  );
}
