import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  base: "/wwwroot/signalr/",
  build: {
    outDir: "../wwwroot/signalr",
    emptyOutDir: false,
    sourcemap: false
  }
});
