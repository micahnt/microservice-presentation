import type { ReactNode } from "react";
import { NavBar } from "./NavBar";

export function Layout({ children }: { children: ReactNode }) {
  return (
    <>
      <NavBar />
      <main className="container page">{children}</main>
      <footer className="footer">
        <div className="container">fastTrack dev2 · Ecommerce microservices demo</div>
      </footer>
    </>
  );
}
