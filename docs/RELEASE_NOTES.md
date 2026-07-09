# Release Notes

Built as one continuous, 16-milestone pass. Each milestone was verified live (real HTTP calls
against a running local instance, or a real browser for frontend work) before moving to the next.

## Milestone 1 - Solution scaffolding & DB foundation
Clean Architecture solution (`STLMS.Domain`/`Application`/`Infrastructure`/`API` + 3 test
projects), `BaseEntity`/`AuditableEntity`, dual-provider `AppDbContext`, first migration.

## Milestone 2 - Auth & RBAC
Register/login/forgot-password/verify-email, JWT + rotating refresh tokens, remember-me, TOTP 2FA,
Google/Microsoft OAuth, session management, seeded roles/permissions.

## Milestone 3 - React scaffold + auth UI
Vite/React 19/TypeScript, Redux store, Axios + React Query, shadcn/ui + Tailwind + dark mode,
auth pages wired to the real API.

## Milestone 4 - Dashboard shell + Settings + Profile
Navigable app shell, widget-grid dashboard, Settings (appearance/regional/religion), Profile
(photo upload, password change, 2FA setup).

## Milestone 5 - World Clock + Timezone Converter
~106 real seeded cities, sunrise/sunset via a hand-rolled NOAA solar-position formula, timezone
conversion via `Intl.DateTimeFormat`.

## Milestone 6 - Alarms
Repeat patterns, snooze, math challenge, `AlarmTriggerService` background job. Introduced
`INotificationDispatcher`, the single fan-out point every later reminder module reuses.

## Milestone 7 - Countdown Timer, Stopwatch, Pomodoro
Shared Web Audio API sound synthesis reused across all three timer types.

## Milestone 8 - Calendar + Event Countdown
`CalendarRecurrenceExpander` - a deliberately simplified frequency+interval+weekday-mask
recurrence model (not full RFC 5545 RRULE parsing), expanded in-memory to sidestep the class of
EF-Core-can't-translate-this-LINQ bug hit earlier in the session.

## Milestone 9 - Medicine Reminder + Habit Tracker + Sleep Tracker
`HabitStreakCalculator` (current/longest streak computed on read, not cached - "a derived value
that's always consistent beats a cached one that can drift"), an achievement system, sleep logging.

## Milestone 10 - Religion & Prayer Center
Real Aladhan (prayer times) and Hebcal (Hebrew calendar) API integrations - chosen specifically
for being free and keyless so they could be genuinely live-verified. Real Computus algorithm for
Christian movable feasts. Explicitly-flagged-approximate Panchang calculator and seeded Sikh/
Buddhist/Jain festival dates, since no free/portable real alternative exists for either. Closed a
real gap from Milestone 4 in the same pass: `UserProfileDto` never returned the prayer location a
user had saved, so Settings always showed it blank on reload.

## Milestone 11 - Productivity Dashboard
Aggregates real data from every module built so far into a rolling 0-100 score. **Caught and fixed
a real bug during live verification**: the first version scored every day before a user's first
log of a module as `0`, tanking a brand-new user's 30-day average to near-zero despite a perfectly
good first day. Fixed by tracking each module's actual adoption date and excluding inapplicable
days entirely (`null` score, not `0`) - this now has a dedicated regression test (Milestone 15).

## Milestone 12 - Smart Notifications + Weather
Extended `INotificationDispatcher` from in-app-only into real email (SMTP) and push (Firebase
Cloud Messaging) fan-out, gated per-user by new toggles, failing soft when unconfigured. Retrofitted
a background reminder trigger into Medicines (mirroring Alarms' design) - the one "reminder-shaped"
module that never actually got one despite the name. Weather built against Open-Meteo instead of
the originally-planned OpenWeatherMap specifically because it needs no API key, enabling genuine
live verification instead of shipping unverified.

## Milestone 13 - Admin Panel
Users (search/paginate/activate/deactivate/unlock/reassign-role), Roles & Permissions (a real
per-role permission matrix, with Super Admin's own permissions protected from being edited),
Audit Log (retrofitted into every new admin mutation - the audit service existed since early in
the build but had never actually been called), and Religions CRUD (fulfilling a doc-comment from
Milestone 2 promising exactly this). New `RequirePermission` frontend route guard, since the
existing `ProtectedRoute` only checked authentication, never authorization.

## Milestone 14 - Reports & Exports polish
A shared `IExportService` (CsvHelper/ClosedXML/QuestPDF) applied to the Productivity report
(CSV/Excel/PDF - the flagship, since it already aggregates every other module) plus CSV exports
for Habits, Medicines, Sleep, and the two new admin lists. Every export streams bytes directly as
the HTTP response rather than touching disk, avoiding the ephemeral-filesystem problem already
flagged for profile photo uploads.

## Milestone 15 - Testing hardening
Backend went from 25 tests to 90: new unit coverage for every previously-untested pure-logic
calculator (the exact class of code where Milestone 11's bug lived), new guard tests for the
Admin Panel's three risky mutations, and a real `WebApplicationFactory`-driven integration test
suite replacing a single empty placeholder test. Added Playwright E2E - nothing existed on the
frontend testing side before this (no Vitest, no Jest, no Playwright, no test script at all).

## Milestone 16 - Deployment artifacts & docs
GitHub Actions CI (backend build+test, frontend build+lint, E2E - none of this existed before),
this documentation set, and a root README (the project had none until now). Cleaned up a stale
`OpenWeatherMap` env var placeholder left over in `render.yaml` from before Milestone 12 switched
Weather to the keyless Open-Meteo API, and added the missing OAuth client-secret env var slots.

## Earlier, out-of-band work

Before this milestone sequence, in the same session: production deployment to Render (Docker
builds, fixed a `dockerBuildArgs`-unsupported blueprint error by moving the frontend's API URL to
a runtime-injected env var instead of a build-time one, fixed an `adduser: not found` crash by
switching to the built-in `$APP_UID` non-root user pattern), a PostgreSQL provider added alongside
SQL Server/SQLite for the production (Supabase) database, and a live register/login verification
against that production deployment.
