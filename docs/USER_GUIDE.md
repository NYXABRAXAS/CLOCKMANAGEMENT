# User Guide

## Getting started

1. **Register** with your name, email, and a password (needs an uppercase letter, a number, and a
   symbol). If no SMTP server is configured, the app can't send a real verification email - in that
   case a "dev-only" verification link is shown directly after registering instead.
2. **Verify your email** via the link, then **log in**.
3. Land on the **Dashboard** - a live clock, current weather (once you've set a location - see
   Settings below), account status, and quick links to every module.

## Modules

**World Clock & Timezone Converter** - add cities from a ~100-city seeded list (each with real
lat/lon and IANA timezone), see sunrise/sunset computed locally, and convert a time between any two
timezones.

**Alarms** - repeat pattern (any combination of days, or one-time), snooze, an optional math
challenge to dismiss, three built-in sound choices. Alarms fire server-side (so they still ring
even if the tab was closed and reopened) and trigger the same in-app/email/push notification flow
described below.

**Countdown Timer, Stopwatch, Pomodoro** - Pomodoro session data feeds directly into your
Productivity score (see below); logged focus minutes there aren't decorative.

**Calendar** - one-time or recurring events (daily/weekly-with-specific-weekdays/monthly/yearly),
plus a simple event countdown list.

**Medicines, Habits, Sleep Tracker** - log doses/check-ins/nights; habits track current and longest
streaks (a scheduled rest day doesn't break a streak - only a missed *scheduled* day does); logging
your first habit check-in and hitting a 7-day streak unlock achievements. Medicines you set a
reminder time for will also trigger a server-side notification, same as alarms.

**Prayer & Festival Center** - select your religion in Settings to unlock a personalized card:

- **Islam**: real prayer times (via Aladhan) for your saved location, the Hijri date, a Qibla
  compass, and tap-to-log prayer completion.
- **Hinduism**: an approximate Panchang (tithi/paksha/nakshatra) - explicitly labeled approximate,
  since no free real-ephemeris source exists.
- **Judaism**: the real Hebrew date (via Hebcal) and the current week's parasha.
- Every religion sees upcoming festivals (Christian dates are computed via the real Easter/Computus
  algorithm; Sikh/Buddhist/Jain dates are seeded and approximate) and a daily quote.

**Productivity Dashboard** - a rolling 0-100 score averaging whichever modules you've actually
adopted (habits, medicines, sleep, pomodoro focus minutes, and prayers if you're Muslim) - a module
you've never used doesn't drag your score down, and a day with nothing applicable shows as a gap in
the trend chart, not a 0. Export your history as CSV, Excel, or PDF.

**Notifications** - the bell icon in the top bar shows your in-app notification inbox (polls every
30 seconds), with mark-read/mark-all-read. Toggle email and push delivery in Settings → Notifications
(push additionally requires the app's Firebase project to be configured - see
[ARCHITECTURE.md](ARCHITECTURE.md#written-but-unverified)).

**Weather** - set a location in Settings → Notifications ("Weather location") to see current
conditions and a 4-day forecast on the Dashboard.

## Settings

- **Appearance**: light/dark/system theme, 12h/24h time format.
- **Regional**: timezone, language, country.
- **Religion**: pick your religion; Islam additionally shows a prayer-location card (lat/lon +
  calculation method) with a "use my current location" button.
- **Notifications**: email/push toggles, weather location.

## Two-factor authentication

Enable TOTP 2FA from your Profile page - scan the generated QR code with any authenticator app
(Google Authenticator, Authy, etc.). Once enabled, logging in requires the 6-digit code from your
app after your password.
