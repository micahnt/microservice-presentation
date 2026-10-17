import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// The gateway serves the API (and, in Docker, the built UI). In dev, Vite proxies the API
// prefixes to it, so the browser always talks to one origin: no CORS anywhere.
const gateway = "http://localhost:6004";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/identity-service": gateway,
      "/catalog-service": gateway,
      "/basket-service": gateway,
      "/ordering-service": gateway,
    },
  },
});
