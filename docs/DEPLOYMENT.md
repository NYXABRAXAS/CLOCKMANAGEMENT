# Deployment Guide (partial - expanded in Milestone 16)

This covers just enough to get STLMS deploying on Render today. The full guide (Azure notes,
GitHub Actions CI, backup/restore, etc.) lands with the rest of the Milestone 16 documentation set.

## Option A - Render (`render.yaml`)

Render has no managed SQL Server offering, so **get an external Postgres database first**. The
easiest free option is [Supabase](https://supabase.com):
1. Create a project (free tier).
2. In **Connect** -> **Connection string**, use the **Session pooler** tab (not "Direct
   connection" - that route is IPv6-only on the free tier and Render may not route outbound IPv6).
3. Copy the connection string and substitute your project's database password for
   `[YOUR-PASSWORD]`.

Any other reachable Postgres instance (Neon, Render Postgres, self-hosted) works too - just supply
its connection string in `Host=...;Port=5432;Database=...;Username=...;Password=...;` form (or
the `postgresql://` URL form, which Npgsql also accepts).

1. Push this repo to GitHub (already done for this project).
2. In the Render dashboard: **New +** -> **Blueprint**, point it at the repo. Render reads
   `render.yaml` and creates:
   - `stlms-api` - the ASP.NET Core API (Docker, `deployment/docker/backend.Dockerfile`)
   - `stlms-web` - the React frontend (Docker, `deployment/docker/frontend.Dockerfile`)
3. Render prompts for the env vars marked `sync: false` in `render.yaml`. At minimum, set
   `ConnectionStrings__Postgres` on `stlms-api` to your Supabase (or other Postgres) connection
   string. Leave `WebUrl`/`Cors__AllowedOrigins__0`/SMTP/OAuth blank for now if you don't have
   those values yet.
4. **First-deploy URL wiring** (needed because `stlms-web` needs to know `stlms-api`'s URL, which
   doesn't exist until `stlms-api`'s first deploy finishes). The API URL is read at **container
   startup**, not baked into the JS bundle at build time, so this only needs a restart:
   - After the first deploy, copy `stlms-api`'s `*.onrender.com` URL.
   - On `stlms-web`: set the `API_URL` env var to `https://<stlms-api-url>/api/v1`, then trigger a
     restart (not a full redeploy - nginx re-reads the env var on boot and regenerates the small
     config file the frontend loads at runtime).
   - Copy `stlms-web`'s URL. On `stlms-api`, set `WebUrl` and `Cors__AllowedOrigins__0` to it, then
     redeploy `stlms-api`.
5. On boot, the API container automatically runs EF Core migrations against the configured
   Postgres database (see `DbSeeder.SeedAsync` in `STLMS.Infrastructure`), then seeds roles/
   permissions/religions if the database is empty. Watch the `stlms-api` logs for confirmation.
6. Redis is optional - leave `ConnectionStrings__Redis` blank and the API automatically falls back
   to an in-memory cache (logged, not silent - see `AddCaching` in
   `STLMS.Infrastructure/DependencyInjection.cs`).

## Option B - Docker Compose (local self-hosting)

```bash
cp .env.example .env
# edit .env: set POSTGRES_PASSWORD / JWT_SECRET / ENCRYPTION_KEY

docker compose up -d --build
```

This runs four containers: `postgres` (real Postgres 16, not a substitute), `redis`, `api`
(port 8080), `web` (port 3000, Nginx serving the built SPA). Data persists in named volumes.

SQL Server is still a supported provider (`Database__Provider=SqlServer` +
`ConnectionStrings__SqlServer`) if you'd rather point at your own SQL Server instance - it just
isn't what `docker-compose.yml` provisions by default.

## Option C - Manual Docker build

```bash
# from the repo root - build context matters, both Dockerfiles expect the monorepo root
docker build -f deployment/docker/backend.Dockerfile -t stlms-api .
docker build -f deployment/docker/frontend.Dockerfile -t stlms-web .
docker run -p 3000:8080 -e API_URL=https://api.example.com/api/v1 stlms-web
```
