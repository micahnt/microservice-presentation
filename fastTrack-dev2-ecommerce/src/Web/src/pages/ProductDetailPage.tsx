import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import { AddToBasketButton } from "../components/AddToBasketButton";
import { ErrorMessage, Loader, money, ProductImage } from "../components/common";
import { useProduct } from "../hooks/useProducts";

export function ProductDetailPage() {
  const { id = "" } = useParams();
  const { data: product, isLoading, error } = useProduct(id);
  const [quantity, setQuantity] = useState(1);

  if (isLoading) return <Loader />;
  if (error || !product) return <ErrorMessage error={error ?? new Error("Product not found.")} />;

  return (
    <>
      <Link to="/" className="back">
        ← All products
      </Link>
      <div className="detail">
        <div className="detail-image card">
          <ProductImage file={product.imageFile} alt={product.name} />
        </div>
        <div className="detail-info">
          <div className="tags">
            {product.category.map((c) => (
              <Link key={c} to={`/?category=${encodeURIComponent(c)}`} className="tag">
                {c}
              </Link>
            ))}
          </div>
          <h1>{product.name}</h1>
          <p className="price large">{money(product.price)}</p>
          <p className="muted">Discounts, if any, are applied in your basket.</p>
          <p>{product.description}</p>
          <p className={product.stock > 0 ? "stock" : "stock out"}>
            {product.stock > 0 ? `${product.stock} in stock` : "Out of stock"}
          </p>
          {product.stock > 0 && (
            <div className="row">
              <label>
                Quantity{" "}
                <select value={quantity} onChange={(e) => setQuantity(Number(e.target.value))}>
                  {Array.from({ length: Math.min(10, product.stock) }, (_, i) => i + 1).map((n) => (
                    <option key={n}>{n}</option>
                  ))}
                </select>
              </label>
              <AddToBasketButton product={product} quantity={quantity} />
            </div>
          )}
        </div>
      </div>
    </>
  );
}
