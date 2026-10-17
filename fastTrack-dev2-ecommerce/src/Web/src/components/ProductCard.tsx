import { Link } from "react-router-dom";
import type { Product } from "../types";
import { AddToBasketButton } from "./AddToBasketButton";
import { money, ProductImage } from "./common";

export function ProductCard({ product }: { product: Product }) {
  return (
    <article className="card product-card">
      <Link to={`/products/${product.id}`} className="product-image-link">
        <ProductImage file={product.imageFile} alt={product.name} className="product-image" />
      </Link>
      <div className="card-body">
        <div className="tags">
          {product.category.map((c) => (
            <span key={c} className="tag">
              {c}
            </span>
          ))}
        </div>
        <h3>
          <Link to={`/products/${product.id}`}>{product.name}</Link>
        </h3>
        <div className="product-meta">
          <span className="price">{money(product.price)}</span>
          <span className={product.stock > 0 ? "stock" : "stock out"}>
            {product.stock > 0 ? `${product.stock} in stock` : "Out of stock"}
          </span>
        </div>
        <AddToBasketButton product={product} />
      </div>
    </article>
  );
}
