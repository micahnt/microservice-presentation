import { useState, type FormEvent } from "react";
import { ApiError } from "../../api/http";
import { ErrorMessage, Loader, money, ProductImage } from "../../components/common";
import { useProductMutations, useProducts } from "../../hooks/useProducts";
import type { Product, ProductInput } from "../../types";

type FormState = { name: string; category: string; description: string; imageFile: string; price: string; stock: string };

const empty: FormState = { name: "", category: "", description: "", imageFile: "product-1.png", price: "", stock: "10" };

const toForm = (p: Product): FormState => ({
  name: p.name,
  category: p.category.join(", "),
  description: p.description,
  imageFile: p.imageFile,
  price: String(p.price),
  stock: String(p.stock),
});

const toInput = (f: FormState): ProductInput => ({
  name: f.name.trim(),
  category: f.category.split(",").map((c) => c.trim()).filter(Boolean),
  description: f.description.trim(),
  imageFile: f.imageFile.trim(),
  price: Number(f.price),
  stock: Number(f.stock),
});

export function AdminProductsPage() {
  const [pageIndex, setPageIndex] = useState(0);
  const pageSize = 20;
  const { data, isLoading, error } = useProducts({ pageIndex, pageSize });
  const { create, update, remove } = useProductMutations();
  const [editing, setEditing] = useState<Product | "new" | null>(null);
  const pages = data ? Math.max(1, Math.ceil(data.count / pageSize)) : 1;

  const onDelete = (p: Product) => {
    if (confirm(`Delete "${p.name}"? This can't be undone.`)) remove.mutate(p.id);
  };

  return (
    <>
      <div className="page-header">
        <h1>Manage products</h1>
        <button className="btn btn-primary" onClick={() => setEditing("new")}>
          New product
        </button>
      </div>

      {editing && (
        <ProductForm
          key={editing === "new" ? "new" : editing.id}
          initial={editing === "new" ? empty : toForm(editing)}
          title={editing === "new" ? "New product" : `Edit ${editing.name}`}
          saving={create.isPending || update.isPending}
          error={create.error ?? update.error}
          onCancel={() => {
            create.reset();
            update.reset();
            setEditing(null);
          }}
          onSave={(input) => {
            const done = { onSuccess: () => setEditing(null) };
            if (editing === "new") create.mutate(input, done);
            else update.mutate({ id: editing.id, input }, done);
          }}
        />
      )}

      <ErrorMessage error={error ?? remove.error} />
      {isLoading && <Loader />}
      {data && (
        <div className="card">
          <table className="table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Categories</th>
                <th className="right">Price</th>
                <th className="right">Stock</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {data.data.map((p) => (
                <tr key={p.id}>
                  <td>
                    <span className="line-product">
                      <ProductImage file={p.imageFile} alt="" className="thumb" />
                      {p.name}
                    </span>
                  </td>
                  <td>{p.category.join(", ")}</td>
                  <td className="right">{money(p.price)}</td>
                  <td className={p.stock === 0 ? "right stock out" : "right"}>{p.stock}</td>
                  <td className="right nowrap">
                    <button className="btn btn-sm" onClick={() => setEditing(p)}>
                      Edit
                    </button>{" "}
                    <button className="btn btn-sm btn-danger" disabled={remove.isPending} onClick={() => onDelete(p)}>
                      Delete
                    </button>
                  </td>
                </tr>
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

function ProductForm(props: {
  initial: FormState;
  title: string;
  saving: boolean;
  error: unknown;
  onSave: (input: ProductInput) => void;
  onCancel: () => void;
}) {
  const [form, setForm] = useState(props.initial);
  const apiError = props.error instanceof ApiError ? props.error : null;
  const set = (name: keyof FormState) => (e: { target: { value: string } }) => setForm({ ...form, [name]: e.target.value });

  const submit = (e: FormEvent) => {
    e.preventDefault();
    props.onSave(toInput(form));
  };

  const fieldError = (name: string) =>
    apiError?.field(name) && <span className="field-error">{apiError.field(name)}</span>;

  return (
    <form className="card form panel" onSubmit={submit}>
      <h2>{props.title}</h2>
      <div className="form-grid">
        <label>
          Name
          <input required value={form.name} onChange={set("name")} />
          {fieldError("name")}
        </label>
        <label>
          Categories (comma separated)
          <input required value={form.category} onChange={set("category")} placeholder="Smart Phone, Camera" />
          {fieldError("category")}
        </label>
        <label>
          Price
          <input required type="number" min="0.01" step="0.01" value={form.price} onChange={set("price")} />
          {fieldError("price")}
        </label>
        <label>
          Stock
          <input required type="number" min="0" step="1" value={form.stock} onChange={set("stock")} />
          {fieldError("stock")}
        </label>
        <label>
          Image file
          <input required value={form.imageFile} onChange={set("imageFile")} placeholder="product-1.png" />
          {fieldError("imageFile")}
        </label>
        <div className="image-preview">
          <ProductImage file={form.imageFile} alt="Preview" className="thumb large" />
        </div>
      </div>
      <label>
        Description
        <textarea rows={3} value={form.description} onChange={set("description")} />
      </label>
      {!apiError?.fieldErrors && <ErrorMessage error={props.error} />}
      <div className="row">
        <button className="btn btn-primary" disabled={props.saving}>
          {props.saving ? "Saving..." : "Save"}
        </button>
        <button type="button" className="btn btn-ghost" onClick={props.onCancel}>
          Cancel
        </button>
      </div>
    </form>
  );
}
