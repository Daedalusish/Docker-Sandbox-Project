import { defineConfig } from "vite";

const apiPort = process.env.API_PORT ?? "3001";

export default defineConfig({
  server: {
    // Listen on all interfaces so the page is reachable from outside a container
    host: true,
    proxy: {
      "/api": `http://localhost:${apiPort}`,
    },
  },
});
