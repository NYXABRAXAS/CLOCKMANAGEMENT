# Architecture

## Layering

Clean Architecture, dependency direction `API → Application → Domain`, `API → Infrastructure →
Application/Domain`. Infrastructure is never referenced by Application directly - only through
interfaces Application defines (`ICacheService`, `IEmailSender`, `IPushSender`, `IWeatherProvider`,
`IPrayerTimeProvider`, `IHebrewCalendarProvider`, `IExportService`, etc.), so the concrete
implementation (real HTTP client vs. in-memory fallback, real SMTP vs. logged no-op) is swappable
without touching a single handler.

- **STLMS.Domain** - entities, enums, `IRepository<T>`/`IUnitOfWork`. Zero external dependencies.
- **STLMS.Application** - a hand-rolled CQRS mediator (`IRequest<T>`, `IRequestHandler<TRequest,
  TResponse>`, `IAppMediator`) instead of MediatR - handlers and FluentValidation validators are
  auto-registered via a reflection scan (`STLMS.Application/DependencyInjection.cs`), so adding a
  new query/command handler needs no manual DI registration. Also holds the pure-logic calculators
  (`ProductivityScoreCalculator`, `HabitStreakCalculator`, `CalendarRecurrenceExpander`,
  `ChristianFeastCalculator`, `QiblaCalculator`, `PanchangCalculator`) that do no I/O at all.
- **STLMS.Infrastructure** - EF Core, external API clients, background services, JWT/2FA/password
  hashing, exports.
- **STLMS.API** - composition root: versioned controllers, middleware, `Program.cs`.

## Persistence

One `AppDbContext` base class, three provider-specific subclasses (`SqliteAppDbContext`,
`SqlServerAppDbContext`, `PostgresAppDbContext`) with **independent migration sets** under
`Persistence/Migrations/{Sqlite,SqlServer,Postgres}/`. This exists because EF Core filters which
migrations apply to a context by the `[DbContext(typeof(...))]` attribute each migration carries -
sharing one context type across providers in the same assembly caused EF to try applying
SQL-Server-flavored migration operations against SQLite (confirmed by reproducing the exact
mismatch this design avoids). **Every new entity needs three migrations generated**, one per
provider, via `dotnet ef migrations add <Name> --context {Sqlite,SqlServer,Postgres}AppDbContext
--output-dir Persistence/Migrations/{Provider} --startup-project ../STLMS.API/STLMS.API.csproj`.

`Database:Provider` config (`Sqlite` | `SqlServer` | `Postgres`) selects which subclass is
registered at startup. Local development and CI both use SQLite by default - no SQL Server/Postgres
instance is required to build, test, or run the app locally.

Every entity soft-deletes (a global EF Core query filter on `ISoftDelete.IsDeleted` excludes
deleted rows automatically from every query) and auto-timestamps (`CreatedAt`/`ModifiedAt`) via
`AppDbContext.SaveChangesAsync`.

## Caching

`ICacheService` behind two implementations: `RedisCacheService` and `MemoryCacheService`. If
`ConnectionStrings:Redis` is unset, or set but unreachable (checked with a 500ms connect timeout at
startup), the app **automatically falls back to the in-memory cache with a logged warning** rather
than failing to start. Local development never has Redis installed, so every local run exercises
(and proves) the fallback path.

## Auth & RBAC

BCrypt password hashing, JWT access token + rotating refresh token (hashed in the database,
revocable) delivered as HTTP-only cookies, TOTP two-factor auth (`Otp.NET` + `QRCoder`),
Google/Microsoft OAuth providers (real code, see [Written but unverified](#written-but-unverified)).

Authorization is entirely data-driven: `Role` ↔ `RolePermission` ↔ `Permission` (keyed by
`Module:Action` pairs, e.g. `"USERS:edit"`) with `[RequirePermission("MODULE", "action")]`
synthesizing an authorization policy on first use (`PermissionPolicyProvider`) - adding a new
module/action never requires new authorization plumbing, just new seeded `Permission` rows. Five
system roles are seeded (`SUPER_ADMIN`, `ADMIN`, `PREMIUM_USER`, `STANDARD_USER`, `GUEST`); Super
Admin's permission set is protected from being edited even by other admins (see
[ADMIN_GUIDE.md](ADMIN_GUIDE.md)).

Because CSRF cookies are scoped to the API's own origin, the API also echoes the CSRF token in the
JSON body of every login/refresh/me/external-login response; the frontend caches it and replays it
as the `x-csrf-token` header on state-changing requests, so the flow keeps working even if the
frontend and API are ever deployed to different origins/subdomains.

## Notifications

`INotificationDispatcher` is the single fan-out point every reminder-shaped module (Alarms,
Medicines) calls through. It always persists an in-app `Notification` row (the one channel
guaranteed to work with zero configuration), then best-effort fans out to email
(`SmtpEmailSender` - a no-op with a logged warning when SMTP isn't configured) and push
(`FcmPushSender` - see below). Failures on the email/push channels are logged, never thrown, so a
bad SMTP/Firebase config can never block the in-app notification every caller actually depends on.

`AlarmTriggerService` and `MedicineReminderService` are both `BackgroundService`s that poll (every
20s and 60s respectively) for due reminders, computing each owner's local time via
`TimeZoneInfo.ConvertTimeFromUtc` and deduping via a history table (`AlarmHistory`,
`MedicineReminderLog`) so a reminder never fires twice for the same occurrence.

## Real external integrations

Chosen deliberately for being **free and keyless**, so they can be genuinely live-verified rather
than shipped as "written but unverified without credentials":

- **Aladhan API** - Islamic prayer times + Hijri date conversion.
- **Hebcal API** - Hebrew calendar conversion + holidays/parasha.
- **Open-Meteo** - current weather + 4-day forecast. Chosen over the originally-planned
  OpenWeatherMap specifically because it needs no API key at all.

Hand-rolled pure-math calculators (no API needed, and none exists that's both free and
.NET-portable): `QiblaCalculator` (real great-circle bearing trigonometry to Mecca's fixed
coordinates), `ChristianFeastCalculator` (the real Anonymous Gregorian/Computus algorithm for
Easter Sunday, verified against known historical dates in tests), `PanchangCalculator`
(**explicitly approximate** synodic/sidereal-month day-counting for Hindu tithi/paksha/nakshatra -
flagged `IsApproximate: true` in both the API response and the UI, since no free or .NET-portable
real ephemeris exists).

Sikh/Buddhist/Jain festival dates are seeded as static, admin-editable `FestivalCalendarEntry` rows
with the same honesty standard - their descriptions explicitly say "approximate" where the
underlying lunar/lunisolar date isn't a fixed Gregorian date.

## Productivity scoring

`ProductivityScoreCalculator` averages only the components a user has actually **adopted** -
habits/medicines only count on days something was actually scheduled; sleep/pomodoro/prayers count
from the date each was first logged onward, not retroactively across the whole requested range. A
day with zero applicable components gets a `null` score, not a misleading `0`. (An earlier version
of this logic scored every day as `0` before a user's first log of a module, which tanked new
users' 30-day average to near-zero on day one - caught during live verification and fixed; see
[RELEASE_NOTES.md](RELEASE_NOTES.md).)

## Exports

`IExportService` (CsvHelper for CSV, ClosedXML for Excel, QuestPDF under its free Community
license for PDF) is shared across every module's export button. Every export streams the generated
bytes straight back as the HTTP response body rather than writing to `wwwroot` - deliberately
avoiding the ephemeral-filesystem problem already hit with profile photo uploads on hosts like
Render's free tier (see below).

## File storage

Profile photos are written to local disk (`wwwroot/uploads/profile-photos/`) via
`LocalFileStorageService` - **not durable** on an ephemeral filesystem PaaS host (Render's free
tier loses local disk contents on every redeploy/restart). Acceptable for this project's current
scope; a real deployment needing persistent uploads would swap this for S3/Azure Blob/Supabase
Storage behind the same `IFileStorageService` interface.

## Written but unverified

These are real, complete implementations that cannot be exercised end-to-end in this project's
development environment because doing so requires credentials/infrastructure this environment
doesn't have. Each fails soft (logs, returns `false`, or is simply inert) rather than crashing when
unconfigured:

| Feature | Why it's unverified | What happens without it |
|---|---|---|
| Google/Microsoft OAuth login | Needs a real OAuth Client ID/Secret registered with Google/Microsoft | Buttons work only once real credentials are configured |
| Firebase Cloud Messaging push | Needs a real Firebase project + a real device/browser token | `FcmPushSender` logs a warning and returns `false`; in-app + email notifications still work |
| Real SQL Server / PostgreSQL execution | Local dev only has SQLite installed | The Postgres provider **was** live-verified in production (Supabase) during this project's actual deployment; SQL Server specifically has not been run against a live instance |
| Real Redis | Not installed in this environment | Automatic in-memory cache fallback is exercised on every local run instead |
| Full networked Docker Compose stack | Docker itself isn't installed in this environment | Dockerfiles were validated by Render's own build pipeline in production, not locally |

## Frontend state

Redux Toolkit owns client-only/ephemeral state (auth session, theme, live-ticking alarm/stopwatch/
pomodoro/countdown runtime state). TanStack Query owns all server state (every module's CRUD data)
via a shared Axios client with JWT-cookie auth and silent refresh-on-401. Route guards:
`ProtectedRoute` (must be authenticated) and `RequirePermission` (must hold at least one of a set
of permissions) wrap route subtrees - the Admin Panel and its sidebar entry are gated by the latter.
