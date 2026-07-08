# STLMS Web (React 19 + Vite)
# Build context MUST be the repo root, e.g.:
#   docker build -f deployment/docker/frontend.Dockerfile -t stlms-web \
#     --build-arg VITE_API_URL=https://api.example.com/api/v1 .

# ---------- Stage 1: build ----------
FROM node:20-alpine AS build
WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend .

# Vite bakes VITE_* env vars into the bundle at build time - it can't be swapped later at
# container-start the way a backend connection string can, so this has to be a build arg.
ARG VITE_API_URL
ENV VITE_API_URL=${VITE_API_URL}
RUN npm run build

# ---------- Stage 2: runtime ----------
FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY deployment/docker/nginx/default.conf.template /etc/nginx/templates/default.conf.template

ENV PORT=8080
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s CMD wget -qO- http://localhost:${PORT}/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
