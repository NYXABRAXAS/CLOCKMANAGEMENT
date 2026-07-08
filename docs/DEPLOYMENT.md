# Deployment Guide (partial - expanded in Milestone 16)

This covers just enough to get STLMS deploying on Render today. The full guide (Azure notes,
GitHub Actions CI, backup/restore, etc.) lands with the rest of the Milestone 16 documentation set.

## Option A - Render (`render.yaml`)

Render has no managed SQL Server offering, so **get an external SQL Server first**:
- [Azure SQL Database](https://azure.microsoft.com/products/azure-sql/database) has a free tier
  (32GB, serverless) - the easiest option if you don't already have a SQL Server somewhere.
- Any other reachable SQL Server instance works too.

Copy its connection string (`Server=...;Database=...;User Id=...;Password=...;
TrustServerCertificate=True;` or similar).

1. Push this repo to GitHub (already done for this project).
2. In the Render dashboard: **New +** -> **Blueprint**, point it at the repo. Render reads
   `render.yaml` and creates:
   - `stlms-api` - the ASP.NET Core API (Docker, `deployment/docker/backend.Dockerfile`)
   - `stlms-web` - the React frontend (Docker, `deployment/docker/frontend.Dockerfile`)
3. Render prompts for the env vars marked `sync: false` in `render.yaml`. At minimum, set
   `ConnectionStrings__SqlServer` on `stlms-api` to your SQL Server connection string. Leave
   `WebUrl`/`Cors__AllowedOrigins__0`/SMTP/OAuth blank for now if you don't have those values yet.
4. **First-deploy URL wiring** (needed because Vite bakes `VITE_API_URL` into the frontend bundle
   at build time - it can't be known before `stlms-api` exists):
   - After the first deploy, copy `stlms-api`'s `*.onrender.com` URL.
   - On `stlms-web`: set the `VITE_API_URL` build arg (in the service's Environment settings, or
     directly in `render.yaml`) to `https://<stlms-api-url>/api/v1`, then trigger **Manual Deploy**
     (a full rebuild, not a restart - the value is compiled into the JS bundle).
   - Copy `stlms-web`'s URL. On `stlms-api`, set `WebUrl` and `Cors__AllowedOrigins__0` to it, then
     redeploy `stlms-api`.
5. On boot, the API container automatically runs EF Core migrations against the configured SQL
   Server (see `DbSeeder.SeedAsync` in `STLMS.Infrastructure`), then seeds roles/permissions/
   religions if the database is empty. Watch the `stlms-api` logs for confirmation.
6. Redis is optional - leave `ConnectionStrings__Redis` blank and the API automatically falls back
   to an in-memory cache (logged, not silent - see `AddCaching` in
   `STLMS.Infrastructure/DependencyInjection.cs`).

## Option B - Docker Compose (local self-hosting)

```bash
cp .env.example .env
# edit .env: set MSSQL_SA_PASSWORD / JWT_SECRET / ENCRYPTION_KEY

docker compose up -d --build
```

This runs four containers: `sqlserver` (real SQL Server 2022, not a substitute), `redis`, `api`
(port 8080), `web` (port 3000, Nginx serving the built SPA). Data persists in named volumes.

## Option C - Manual Docker build

```bash
# from the repo root - build context matters, both Dockerfiles expect the monorepo root
docker build -f deployment/docker/backend.Dockerfile -t stlms-api .
docker build -f deployment/docker/frontend.Dockerfile -t stlms-web \
  --build-arg VITE_API_URL=https://api.example.com/api/v1 .
```
