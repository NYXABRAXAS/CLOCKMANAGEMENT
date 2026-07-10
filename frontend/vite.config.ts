import path from "node:path";
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import { VitePWA } from "vite-plugin-pwa";

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    VitePWA({
      registerType: "autoUpdate",
      includeAssets: ["icons/apple-touch-icon.png"],
      manifest: {
        name: "STLMS - Smart Time & Lifestyle Management System",
        short_name: "STLMS",
        description: "Time management, habit tracking, prayer times, and productivity in one app.",
        start_url: "/dashboard",
        display: "standalone",
        background_color: "#ffffff",
        theme_color: "#4f46e5",
        icons: [
          { src: "/icons/icon-192.png", sizes: "192x192", type: "image/png" },
          { src: "/icons/icon-512.png", sizes: "512x512", type: "image/png" },
          { src: "/icons/icon-512-maskable.png", sizes: "512x512", type: "image/png", purpose: "maskable" },
        ],
      },
      workbox: {
        // Cache the app shell only - API responses are always fetched fresh (server state is
        // already owned by React Query, which has its own cache/staleness rules; a service worker
        // caching API GETs on top of that would just be a second, conflicting cache to reason about).
        navigateFallbackDenylist: [/^\/api\//],
      },
    }),
  ],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    port: 5173,
  },
});
