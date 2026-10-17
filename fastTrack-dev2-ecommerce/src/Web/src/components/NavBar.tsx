import { useQueryClient } from "@tanstack/react-query";
import { Link, NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useBasket } from "../hooks/useBasket";

export function NavBar() {
  const { user, isAdmin, logout } = useAuth();
  const { data: basket } = useBasket();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const count = basket?.items.reduce((n, i) => n + i.quantity, 0) ?? 0;

  const signOut = () => {
    logout();
    queryClient.clear(); // drop the previous user's basket and orders
    navigate("/");
  };

  return (
    <header className="navbar">
      <div className="container navbar-inner">
        <Link to="/" className="brand">
          Ecommerce<span>Shop</span>
        </Link>
        <nav className="nav-links">
          <NavLink to="/" end>
            Products
          </NavLink>
          {user && <NavLink to="/orders">My orders</NavLink>}
          {isAdmin && <NavLink to="/admin/products">Manage products</NavLink>}
          {isAdmin && <NavLink to="/admin/orders">Manage orders</NavLink>}
        </nav>
        <div className="nav-actions">
          {user ? (
            <>
              <NavLink to="/basket" className="basket-link">
                Basket {count > 0 && <span className="count">{count}</span>}
              </NavLink>
              <span className="user" title={user.email}>
                {user.name}
              </span>
              <button className="btn btn-ghost" onClick={signOut}>
                Sign out
              </button>
            </>
          ) : (
            <>
              <NavLink to="/login">Sign in</NavLink>
              <Link to="/register" className="btn btn-primary btn-sm">
                Register
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
