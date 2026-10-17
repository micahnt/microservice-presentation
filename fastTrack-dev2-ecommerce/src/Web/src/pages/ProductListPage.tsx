import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { ErrorMessage, Loader } from "../components/common";
import { ProductCard } from "../components/ProductCard";
import { useCategories, useProducts } from "../hooks/useProducts";

const pageSize = 8;

export function ProductListPage() {
  // Filters live in the URL so they survive reloads and the back button.
  const [params, setParams] = useSearchParams();
  const category = params.get("category") ?? "";
  const search = params.get("search") ?? "";
  const pageIndex = Number(params.get("page") ?? 0);
  const [searchInput, setSearchInput] = useState(search);

  useEffect(() => setSearchInput(search), [search]);

  const update = (changes: Record<string, string>) => {
    const next = new URLSearchParams(params);
    Object.entries(changes).forEach(([k, v]) => (v ? next.set(k, v) : next.delete(k)));
    if (!("page" in changes)) next.delete("page");
    setParams(next);
  };

  const { data, isLoading, error, isFetching } = useProducts({ pageIndex, pageSize, category, search });
  const { data: categories } = useCategories();
  const pages = data ? Math.max(1, Math.ceil(data.count / pageSize)) : 1;

  return (
    <>
      <div className="page-header">
        <h1>Products</h1>
        <form
          className="search"
          onSubmit={(e) => {
            e.preventDefault();
            update({ search: searchInput.trim() });
          }}
        >
          <input
            type="search"
            placeholder="Search by name"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            aria-label="Search products"
          />
          <button className="btn">Search</button>
        </form>
      </div>

      <div className="chips">
        <button className={!category ? "chip active" : "chip"} onClick={() => update({ category: "" })}>
          All
        </button>
        {categories?.map((c) => (
          <button key={c} className={c === category ? "chip active" : "chip"} onClick={() => update({ category: c })}>
            {c}
          </button>
        ))}
      </div>

      <ErrorMessage error={error} />
      {isLoading && <Loader />}

      {data && data.data.length === 0 && <p className="empty">No products match your filters.</p>}

      {data && data.data.length > 0 && (
        <>
          <div className={isFetching ? "grid fading" : "grid"}>
            {data.data.map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
          {pages > 1 && (
            <div className="pager">
              <button className="btn" disabled={pageIndex === 0} onClick={() => update({ page: String(pageIndex - 1) })}>
                Previous
              </button>
              <span>
                Page {pageIndex + 1} of {pages}
              </span>
              <button
                className="btn"
                disabled={pageIndex + 1 >= pages}
                onClick={() => update({ page: String(pageIndex + 1) })}
              >
                Next
              </button>
            </div>
          )}
        </>
      )}
    </>
  );
}
