// Placeholder for local dev / `vite build` output. In the production Docker image, nginx's
// entrypoint overwrites this file at container startup from the API_URL env var (see
// deployment/docker/nginx/generate-runtime-config.sh) - so the API URL can change without a
// frontend rebuild.
window.__RUNTIME_CONFIG__ = {};
