import { defineConfig } from "vite";

export default defineConfig({
  build: { outDir: "../wwwroot", emptyOutDir: true },
  server: {
    proxy: {
      "/api": "http://localhost:5158",
      "/stateflows": "http://localhost:5158",
    },
  },
});
