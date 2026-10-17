import { Link } from "react-router-dom";
import { ErrorMessage, Loader, money, ProductImage } from "../components/common";
import { useBasket, useBasketActions } from "../hooks/useBasket";

export function BasketPage() {
  const { data: basket, isLoading, error } = useBasket();
  const { setQuantity, saving, saveError, clear } = useBasketActions();

  if (isLoading) return <Loader />;
  if (error) return <ErrorMessage error={error} />;
  if (!basket || basket.items.length === 0)
    return (
      <div className="empty-state">
        <h1>Your basket is empty</h1>
        <Link to="/" className="btn btn-primary">
          Browse products
        </Link>
      </div>
    );

  const discount = basket.items.reduce((sum, i) => sum + i.discount * i.quantity, 0);

  return (
    <>
      <h1>Basket</h1>
      <ErrorMessage error={saveError ?? clear.error} />
      <div className="basket">
        <div className="card">
          <table className="table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Unit price</th>
                <th>Discount</th>
                <th>Quantity</th>
                <th className="right">Line total</th>
              </tr>
            </thead>
            <tbody>
              {basket.items.map((item) => (
                <tr key={item.productId}>
                  <td>
                    <Link to={`/products/${item.productId}`} className="line-product">
                      <ProductImage file={item.imageFile} alt="" className="thumb" />
                      {item.productName}
                    </Link>
                  </td>
                  <td>{money(item.unitPrice)}</td>
                  <td>{item.discount > 0 ? <span className="discount">−{money(item.discount)}</span> : "—"}</td>
                  <td>
                    <div className="qty">
                      <button
                        className="btn btn-sm"
                        disabled={saving}
                        onClick={() => setQuantity(item.productId, item.quantity - 1)}
                        aria-label={`One less ${item.productName}`}
                      >
                        −
                      </button>
                      <span>{item.quantity}</span>
                      <button
                        className="btn btn-sm"
                        disabled={saving}
                        onClick={() => setQuantity(item.productId, item.quantity + 1)}
                        aria-label={`One more ${item.productName}`}
                      >
                        +
                      </button>
                      <button
                        className="btn btn-ghost btn-sm"
                        disabled={saving}
                        onClick={() => setQuantity(item.productId, 0)}
                      >
                        Remove
                      </button>
                    </div>
                  </td>
                  <td className="right">{money(item.price * item.quantity)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <aside className="card summary">
          <h2>Summary</h2>
          {discount > 0 && (
            <div className="summary-row">
              <span>You save</span>
              <span className="discount">{money(discount)}</span>
            </div>
          )}
          <div className="summary-row total">
            <span>Total</span>
            <span>{money(basket.totalPrice)}</span>
          </div>
          <Link to="/checkout" className="btn btn-primary btn-block">
            Checkout
          </Link>
          <button className="btn btn-ghost btn-block" disabled={clear.isPending} onClick={() => clear.mutate()}>
            Empty basket
          </button>
        </aside>
      </div>
    </>
  );
}
