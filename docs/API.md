# API Reference

The authoritative, always-current API reference is **Swagger/OpenAPI**, generated directly from
the controllers - run the backend locally and open `http://localhost:5080/swagger`. This document
is a map of what exists and how authorization works, not a hand-maintained copy of every endpoint
(a hand-written list would drift out of sync with the code almost immediately).

## Versioning & base path

All routes are under `/api/v1/...` (via `Asp.Versioning`; `v1` is both the default and the only
version so far).

## Authentication

JWT access token + rotating refresh token, both delivered as HTTP-only cookies set by
`/auth/login`, `/auth/refresh`, and `/auth/external-login`. Because a cookie is scoped to the API's
own origin, the CSRF token is *also* returned in the JSON body of those same responses; the
frontend caches it and replays it as the `x-csrf-token` header on every state-changing request
(`POST`/`PUT`/`DELETE`). Unauthenticated requests to any endpoint other than `/auth/*`,
`/health`, and the public reference-data reads return `401`.

## Authorization

Every controller action is gated by `[RequirePermission("MODULE", "action")]`, where `action` is
one of `view`/`create`/`edit`/`delete`. A request without the matching permission returns `403`.
Permissions are entirely data-driven (see [ARCHITECTURE.md](ARCHITECTURE.md#auth--rbac)) - the
table below is the full list of `MODULE` strings currently in use.

| Module | Covers |
|---|---|
| `DASHBOARD` | Dashboard widgets |
| `USERS` | Admin: list/activate/deactivate/unlock/reassign-role, export |
| `ROLES` | Admin: role/permission matrix |
| `AUDIT_LOGS` | Admin: audit log viewer, export |
| `RELIGIONS` | Religion reference data (read for everyone, CRUD for admins) |
| `SETTINGS` | User settings (appearance, region, religion, weather/prayer location, notification prefs) |
| `PROFILE` | Own profile, password change, 2FA setup |
| `WORLD_CLOCK` | World clock cities, timezone converter |
| `ALARMS` | Alarms CRUD |
| `TIMERS` | Countdown timer, stopwatch, pomodoro |
| `CALENDAR` | Calendar events, event countdowns |
| `HEALTH` | Medicines, habits, sleep log, achievements |
| `PRAYER_CENTER` | Prayer times, Panchang, Hebrew calendar, festivals, daily quote |
| `PRODUCTIVITY` | Productivity summary + report export |
| `NOTIFICATIONS` | Notification inbox, device registration |
| `WEATHER` | Current weather + forecast |

`SUPER_ADMIN`/`ADMIN` hold every module; `PREMIUM_USER`/`STANDARD_USER` hold every module *except*
`USERS`/`ROLES`/`AUDIT_LOGS` (never even `view`); `GUEST` holds `view` only on the same self-service
modules. Admin can edit `ROLES:view` but not create/edit/delete roles - only Super Admin can, and
even Super Admin's own role's permissions can never be edited (see
[ADMIN_GUIDE.md](ADMIN_GUIDE.md)).

## Controller inventory

27 controllers under `Controllers/v1/` (and `Controllers/v1/Admin/` for the three admin-only
ones): Auth, Profile, Settings, Religions, Cities, WorldClock, Alarms, CountdownTimers, Stopwatch,
Pomodoro, CalendarEvents, EventCountdowns, Medicines, Habits, Achievements, Sleep, PrayerTimes,
Panchang, HebrewCalendar, Festivals, Quotes, Productivity, Notifications, Weather, Health
(liveness), and `Admin/Users`, `Admin/Roles`, `Admin/AuditLogs`.

## Export endpoints

Six endpoints stream a generated file directly as the response body (`Content-Disposition:
attachment`) rather than a JSON payload - see [ARCHITECTURE.md](ARCHITECTURE.md#exports):

| Endpoint | Formats |
|---|---|
| `GET /productivity/export?from=&to=&format=` | csv, excel, pdf |
| `GET /habits/export` | csv |
| `GET /medicines/export` | csv |
| `GET /sleep-logs/export` | csv |
| `GET /admin/users/export` | csv |
| `GET /admin/audit-logs/export` | csv |

## Error shape

Unhandled exceptions are normalized by `ExceptionHandlingMiddleware` into a consistent problem-
details-style body: `{ "statusCode": 409, "title": "...", "errors": null, "traceId": "..." }`
(validation failures populate `errors` as a field → message-list map instead).
