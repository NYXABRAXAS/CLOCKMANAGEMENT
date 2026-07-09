# Software Requirements Specification

## 1. Purpose & scope

STLMS (Smart Time & Lifestyle Management System) is a personal productivity web application
covering time-management tools (world clock, alarms, timers, calendar), lifestyle tracking
(medicines, habits, sleep), a multi-religion prayer/festival center, a cross-module productivity
score, notifications, weather, and administrative user/role management. Single-tenant per
deployment; multi-user within that deployment via role-based access control.

## 2. Functional requirements

### 2.1 Identity & Access
- FR-1.1: Users register with email/password; email verification required before login.
- FR-1.2: Login supports "remember me" (30-day refresh token vs. 7-day default) and optional TOTP
  two-factor authentication.
- FR-1.3: Accounts lock after 5 consecutive failed login attempts.
- FR-1.4: Google and Microsoft OAuth login are supported as alternatives to password login.
- FR-1.5: Every action is gated by a `Module:Action` permission; permissions are assignable per
  role by a Super Admin, not hardcoded per feature.

### 2.2 Time Tools
- FR-2.1: Users can add cities to a personal world clock list and see sunrise/sunset times.
- FR-2.2: Users can convert a time between any two timezones.
- FR-2.3: Alarms support repeat-day patterns, snooze, an optional dismiss challenge, and fire
  server-side (independent of the client tab being open).
- FR-2.4: Countdown timers, a stopwatch with laps, and a configurable Pomodoro timer are provided.

### 2.3 Calendar
- FR-3.1: Users can create one-time or recurring (daily/weekly-with-weekday-selection/monthly/
  yearly) events.
- FR-3.2: Users can track a standalone countdown to a named future date.

### 2.4 Health & Habits
- FR-4.1: Users can schedule medicines with one or more daily reminder times and log each dose as
  taken or skipped; a scheduled dose past its time and not yet logged triggers a reminder
  notification.
- FR-4.2: Users can track habits with a repeat-day schedule and see current/longest streaks; a
  non-scheduled day never breaks a streak.
- FR-4.3: Users can log nightly sleep (bed/wake time, quality rating) and see a rolling average.
- FR-4.4: Habit milestones (first check-in, 7-day streak, 30-day streak, 100 total check-ins)
  unlock achievements.

### 2.5 Religion & Prayer Center
- FR-5.1: Users select one of 7 supported religions (or none); the selection personalizes the
  Prayer Center, festival list, and daily quote.
- FR-5.2: Muslim users receive real prayer times and Qibla direction for a saved location, and can
  log each of the 5 daily prayers.
- FR-5.3: Hindu users see an approximate Panchang (tithi/paksha/nakshatra), explicitly labeled
  approximate.
- FR-5.4: Jewish users see the real Hebrew date and current parasha.
- FR-5.5: All users see upcoming festivals for their religion and across all religions, and a
  daily quote (religion-specific when applicable, otherwise universal).

### 2.6 Productivity
- FR-6.1: A rolling 0-100 score is computed daily, averaging only the modules a user has actually
  adopted (habits/medicines only on days something was scheduled; sleep/pomodoro/prayers from
  first use onward). A day with nothing applicable has no score, not a zero.
- FR-6.2: Current and best productive-day streaks (score ≥ 60) are tracked.
- FR-6.3: The productivity report is exportable as CSV, Excel, or PDF for a selected date range.

### 2.7 Notifications & Weather
- FR-7.1: Every reminder (alarms, medicines) produces an in-app notification, and optionally an
  email and/or push notification per the user's own toggles.
- FR-7.2: Users can view, mark-read, and mark-all-read their notification inbox.
- FR-7.3: Users can set a weather location and see current conditions plus a 4-day forecast.

### 2.8 Administration
- FR-8.1: Admins can search/list all users and activate, deactivate, unlock, or reassign the role
  of any account other than their own.
- FR-8.2: Super Admin can grant/revoke any permission for any role except Super Admin's own role.
- FR-8.3: Admins can view a full audit trail of administrative actions.
- FR-8.4: Admins can create/edit/delete religion reference data (built-in religions can't be
  deleted).
- FR-8.5: User and audit-log lists are exportable as CSV.

## 3. Non-functional requirements

- NFR-1: Passwords are hashed with BCrypt; refresh tokens are stored hashed, never in plaintext.
- NFR-2: All authenticated cookies are HTTP-only; CSRF protection via a double-submit token on
  every state-changing request.
- NFR-3: The database provider (SQLite/SQL Server/PostgreSQL) and cache provider (in-memory/Redis)
  are each swappable via configuration alone, with automatic fallback to the zero-infrastructure
  option (SQLite, in-memory cache) when the alternative isn't configured or reachable.
- NFR-4: External API failures (weather, prayer times, email, push) degrade gracefully - they log
  and return a safe default/failure rather than crashing the request that triggered them.
- NFR-5: The frontend is a single-page app targeting evergreen browsers; no IE11 support.

## 4. Constraints & assumptions

- Single time zone per user (stored on the account, not per-event) - a user traveling doesn't get
  automatic per-device timezone detection beyond what their browser reports at Settings time.
- The application assumes one deployment serves one organization/community's worth of users;
  there is no multi-tenancy (separate customer data isolation) built in beyond per-user data
  ownership.
- Panchang, and Sikh/Buddhist/Jain festival dates, are approximations - see
  [ARCHITECTURE.md](ARCHITECTURE.md) for why, and the honesty flags surfaced in the API/UI.

## 5. Out of scope / known limitations

See [ARCHITECTURE.md § Written but unverified](ARCHITECTURE.md#written-but-unverified) for the
full list (OAuth login, FCM push, real SQL Server/Redis/Docker) and [TEST_PLAN.md](TEST_PLAN.md)
for what test coverage does and doesn't include. Not attempted at all: multi-tenancy, load/
performance testing, native mobile apps, offline support, real-time (WebSocket) notification
delivery (the inbox polls every 30 seconds instead), and localization beyond a language field that
isn't yet wired to any translated UI strings.
