# STLMS - Smart Time & Lifestyle Management System

A full-stack personal productivity platform: world clock, alarms, timers, a calendar with
recurring events, medicine/habit/sleep tracking, a multi-religion prayer and festival center,
a rolling productivity score, in-app/email/push notifications, live weather, and an admin panel
for user/role/permission management - built on React 19 and ASP.NET Core 10.

## Tech stack

**Backend** - .NET 10 / ASP.NET Core, Clean Architecture (`Domain` → `Application` → `Infrastructure`/`API`),
a hand-rolled CQRS mediator (no MediatR dependency), EF Core with three interchangeable providers
(SQLite for local dev, SQL Server, PostgreSQL), JWT + rotating refresh tokens in HTTP-only cookies,
TOTP two-factor auth, role/permission-based authorization seeded entirely from data (no hardcoded
role checks).

**Frontend** - React 19, TypeScript, Vite, Redux Toolkit (client/ephemeral state), TanStack Query
(server state), React Router 7, Tailwind CSS 4 + shadcn/ui, Recharts.

**Real external integrations** (all free, no fake data):
[Aladhan](https://aladhan.com/prayer-times-api) for Islamic prayer times, [Hebcal](https://www.hebcal.com/home/developer-apis)
for the Hebrew calendar, [Open-Meteo](https://open-meteo.com/) for weather - all three chosen
specifically because they need no API key, so they work out of the box with zero configuration.

**Exports** - CsvHelper (CSV), ClosedXML (Excel), QuestPDF (PDF, Community license).

## Project layout

```
backend/
  src/
    STLMS.Domain/          entities, enums, IRepository<T>/IUnitOfWork - zero dependencies
    STLMS.Application/     CQRS handlers, validators, DTOs, external-service interfaces
    STLMS.Infrastructure/  EF Core, JWT/2FA, external API clients, background services, exports
    STLMS.API/             composition root - controllers, middleware, Program.cs
  tests/
    STLMS.Domain.Tests/            entity behavior
    STLMS.Application.Tests/       handler + pure-logic-calculator unit tests (Moq + hand-written fakes)
    STLMS.API.IntegrationTests/    WebApplicationFactory-driven HTTP tests against a real SQLite DB
frontend/
  src/
    app/          Redux store, typed hooks
    routes/       React Router tree, auth/permission route guards
    shared/       UI primitives, API client, cross-cutting utilities
    features/     one folder per module (auth, dashboard, worldClock, alarms, timers, calendar,
                  health, religionCenter, productivity, notifications, weather, settings, profile, admin)
  e2e/            Playwright end-to-end tests
deployment/       Dockerfiles, nginx config
docs/             architecture, ER diagram, API reference, user/admin guides, test plan, release notes
render.yaml       Render Blueprint (free-tier deploy against an external Postgres database)
docker-compose.yml
```

## Prerequisites

- .NET 10 SDK
- Node.js 20+
- No SQL Server/Redis/Docker required for local development - SQLite and an in-memory cache are
  used automatically when those aren't configured (see [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)).

## Running locally

**Backend** (from repo root):
```bash
dotnet run --project backend/src/STLMS.API/STLMS.API.csproj --urls http://localhost:5080
```
On first run this creates `backend/src/STLMS.API/App_Data/stlms.dev.db`, applies all migrations,
and seeds roles/permissions/religions/cities/achievements/festivals/quotes automatically. Swagger UI
is available at `http://localhost:5080/swagger` in Development.

**Frontend** (from `frontend/`):
```bash
npm install
npm run dev
```
Opens at `http://localhost:5173`, pointed at the API via `VITE_API_URL` (defaults to
`http://localhost:5080/api/v1`).

## Running tests

```bash
# Backend: 90 tests across Domain/Application/Integration projects
dotnet test STLMS.slnx

# Frontend build + typecheck
cd frontend && npm run build

# Frontend E2E (starts both the dev server and the API automatically)
cd frontend && npm run test:e2e
```

See [docs/TEST_PLAN.md](docs/TEST_PLAN.md) for what's covered and what's explicitly out of scope.

## Deployment

See [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) for Docker Compose and Render instructions.
`render.yaml` deploys both services as free-tier Render web services against an external Postgres
database (e.g. a free Supabase project).

## Documentation

- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) - system design, key architectural decisions, honesty-flagged approximations
- [docs/ER_DIAGRAM.md](docs/ER_DIAGRAM.md) - entity relationship diagram
- [docs/API.md](docs/API.md) - API reference and permission model
- [docs/USER_GUIDE.md](docs/USER_GUIDE.md) - end-user manual
- [docs/ADMIN_GUIDE.md](docs/ADMIN_GUIDE.md) - admin panel manual, including how to grant the first Super Admin
- [docs/TEST_PLAN.md](docs/TEST_PLAN.md) - test strategy and current coverage
- [docs/RELEASE_NOTES.md](docs/RELEASE_NOTES.md) - milestone-by-milestone changelog
- [docs/SRS.md](docs/SRS.md) - software requirements specification

## Known limitations

A handful of features are real, wired-up code that cannot be verified end-to-end without
credentials this project doesn't have: Google/Microsoft OAuth login, Firebase Cloud Messaging push
notifications, and real SQL Server/Redis behavior (both work identically to their tested SQLite/
in-memory-cache fallbacks by design, but haven't been run against the real thing). See
[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md#written-but-unverified) for the full list and why.
