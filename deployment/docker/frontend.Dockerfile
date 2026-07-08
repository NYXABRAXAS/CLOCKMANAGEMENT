# STLMS Web (React 19 + Vite)
# Build context MUST be the repo root, e.g.:
#   docker build -f deployment/docker/frontend.Dockerfile -t stlms-web .
#
# The API URL is NOT a build arg - it's injected at container startup (see
# deployment/docker/nginx/generate-runtime-config.sh), so the same built image works against any
# API URL and changing it only needs a container restart, not a rebuild.

# ---------- Stage 1: build ----------
FROM node:20-alpine AS build
WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend .
RUN npm run build

# ---------- Stage 2: runtime ----------
FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY deployment/docker/nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY deployment/docker/nginx/runtime-config.js.template /etc/nginx/templates/runtime-config.js.template
COPY deployment/docker/nginx/generate-runtime-config.sh /docker-entrypoint.d/15-generate-runtime-config.sh
RUN chmod +x /docker-entrypoint.d/15-generate-runtime-config.sh

ENV PORT=8080
ENV API_URL=""
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s CMD wget -qO- http://localhost:${PORT}/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
