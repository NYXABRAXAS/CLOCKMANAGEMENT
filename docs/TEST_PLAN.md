# Test Plan

## Strategy

Three layers, each testing what it's actually good at testing - no layer tries to cover what
another layer already covers better:

1. **Unit tests** (`STLMS.Domain.Tests`, `STLMS.Application.Tests`) - pure logic and command/query
   handlers in isolation, using Moq for external-boundary interfaces (`IAuditService`,
   `IEmailSender`, `IHostEnvironment`) and a hand-written in-memory `FakeUnitOfWork`/`FakeRepository<T>`
   for data access (Moq can't usefully mock `IQueryable` `Query()` chains - a real in-memory list is
   more honest than trying to stub every possible LINQ expression).
2. **Integration tests** (`STLMS.API.IntegrationTests`) - `WebApplicationFactory<Program>` boots the
   *real* DI graph, middleware pipeline, and DbSeeder-seeded roles/permissions against a throwaway
   SQLite file, then drives actual HTTP requests through it. This is the only layer that proves
   registration → verification → login → a permission-gated endpoint actually works wired together.
3. **End-to-end tests** (`frontend/e2e`, Playwright) - a real browser against the real Vite dev
   server *and* the real .NET API (not mocked), covering the same register/login/dashboard flow a
   real user drives.

## Running

```bash
dotnet test STLMS.slnx                    # 90 backend tests (Domain + Application + Integration)
cd frontend && npm run build               # typecheck + production build
cd frontend && npm run test:e2e            # 4 Playwright E2E tests (starts both servers itself)
```

CI (`.github/workflows/ci.yml`) runs all three on every push/PR to `main`.

## What's covered

- **Auth**: registration (default role assignment, duplicate-email conflict, dev-only verification
  token exposure rules), login (lockout after 5 failed attempts, disabled-account rejection,
  2FA challenge flow, timing-safe unknown-email handling), password hashing, JWT issuance, TOTP.
- **Pure-logic calculators** - deliberately prioritized, since this is exactly the kind of
  untested hand-rolled math that caused a real bug this session (`ProductivityScoreCalculator`
  scoring every unadopted day as `0` instead of excluding it - caught during live verification,
  fixed, and now has a dedicated regression test):
  `ProductivityScoreCalculator`, `HabitStreakCalculator`, `CalendarRecurrenceExpander` (including a
  real `DateTime.AddMonths` drift quirk found while writing its tests), `ChristianFeastCalculator`
  (checked against real historical Easter dates, not values the algorithm itself produced),
  `QiblaCalculator`, `PanchangCalculator`.
- **Admin action guards**: can't deactivate/reassign-role on your own account, Super Admin's
  permissions can't be edited, unknown role codes rejected.
- **Full auth HTTP flow**: register → verify → login → cookie-authenticated request to a
  permission-gated endpoint; 401 without auth; wrong password; duplicate email; health check.
- **E2E**: register redirects to login; login (after API-driven register+verify, since a browser
  test has no way to receive a real verification email) lands on the dashboard; wrong password
  keeps you on the login page; an unauthenticated visit to a protected route redirects to login.

## What's explicitly not covered, and why

- **Every handler in the app.** With ~25 controllers and correspondingly many command/query
  handlers, "test every single one" was not pursued as a goal in itself - coverage was prioritized
  toward the highest-risk code (untested hand-rolled math, new admin guards) rather than padding
  the count with handlers that are thin passthroughs to already-tested repository operations.
- **Google/Microsoft OAuth, Firebase push, real SQL Server/Redis** - see
  [ARCHITECTURE.md](ARCHITECTURE.md#written-but-unverified). These need credentials/infrastructure
  this environment doesn't have; the code exists and fails soft when unconfigured, but has no
  automated test exercising the real third-party handshake.
- **Load/performance testing** - not attempted. This is a personal-productivity app at
  individual-user scale; nothing in the design (per-user data, no shared hot paths beyond the
  seeded reference tables) suggests this is a near-term concern.
- **Cross-browser E2E** - Playwright is configured for Chromium only. Adding Firefox/WebKit
  projects to `playwright.config.ts` is a one-line change if cross-browser coverage becomes a
  priority; it wasn't exercised here to keep CI runtime reasonable.
