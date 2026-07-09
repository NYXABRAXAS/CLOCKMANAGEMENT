# Entity Relationship Diagram

39 entities across 24 files (several files hold a parent + its child log/detail entity together,
by convention - see [ARCHITECTURE.md](ARCHITECTURE.md)). Split into logical groups below for
readability rather than one unreadable 39-entity diagram; every entity also has `Id` (Guid),
`CreatedAt`, `ModifiedAt`, and `IsDeleted` from the shared `AuditableEntity`/`BaseEntity` base
classes (omitted from each table below to avoid repeating them 39 times).

## Identity & RBAC

```mermaid
erDiagram
    USER ||--o{ USER_ROLE : has
    ROLE ||--o{ USER_ROLE : assigned_to
    ROLE ||--o{ ROLE_PERMISSION : grants
    PERMISSION ||--o{ ROLE_PERMISSION : granted_by
    USER ||--o{ REFRESH_TOKEN : owns
    USER ||--o{ EXTERNAL_LOGIN : links
    USER ||--o{ USER_SESSION : has
    USER }o--|| RELIGION : selects

    USER {
        string Email
        bool EmailVerified
        string PasswordHash
        string FirstName
        string LastName
        string TimezoneId
        double PrayerLatitude
        double PrayerLongitude
        double WeatherLatitude
        double WeatherLongitude
        bool EmailNotificationsEnabled
        bool PushNotificationsEnabled
        bool TwoFactorEnabled
        int FailedLoginAttempts
        DateTime LockedUntil
    }
    ROLE {
        string Code
        string Name
        bool IsSystem
    }
    PERMISSION {
        string Module
        string Action
    }
    USER_ROLE {
        guid UserId FK
        guid RoleId FK
    }
    ROLE_PERMISSION {
        guid RoleId FK
        guid PermissionId FK
    }
    REFRESH_TOKEN {
        guid UserId FK
        string TokenHash
        DateTime ExpiresAt
    }
    EXTERNAL_LOGIN {
        guid UserId FK
        string Provider
        string ProviderUserId
    }
    USER_SESSION {
        guid UserId FK
        string DeviceName
        string IpAddress
    }
```

## World Clock, Alarms, Timers

```mermaid
erDiagram
    USER ||--o{ WORLD_CLOCK_CITY : favorites
    CITY ||--o{ WORLD_CLOCK_CITY : referenced_by
    USER ||--o{ ALARM : owns
    ALARM ||--o{ ALARM_HISTORY : logs
    USER ||--o{ COUNTDOWN_TIMER : owns
    USER ||--o{ STOPWATCH_SESSION : owns
    STOPWATCH_SESSION ||--o{ STOPWATCH_LAP : records
    USER ||--o{ POMODORO_SESSION : owns
    POMODORO_SESSION ||--o{ POMODORO_LOG : records

    CITY {
        string Name
        string Country
        string TimezoneId
        double Latitude
        double Longitude
    }
    ALARM {
        guid UserId FK
        string Label
        int Hour
        int Minute
        int RepeatDaysMask
        bool IsEnabled
        string ChallengeType
    }
    ALARM_HISTORY {
        guid AlarmId FK
        string Action
        DateTime OccurredAt
    }
    COUNTDOWN_TIMER {
        guid UserId FK
        string Label
        DateTime TargetAt
    }
    STOPWATCH_SESSION {
        guid UserId FK
        DateTime StartedAt
        DateTime EndedAt
    }
    STOPWATCH_LAP {
        guid StopwatchSessionId FK
        int LapNumber
        int ElapsedMs
    }
    POMODORO_SESSION {
        guid UserId FK
        int WorkMinutes
        int CyclesBeforeLongBreak
    }
    POMODORO_LOG {
        guid PomodoroSessionId FK
        string Phase
        bool CompletedFully
    }
```

## Calendar

```mermaid
erDiagram
    USER ||--o{ CALENDAR_EVENT : owns
    USER ||--o{ EVENT_COUNTDOWN : owns

    CALENDAR_EVENT {
        guid UserId FK
        string Title
        DateTime StartAt
        DateTime EndAt
        string RecurrenceFrequency
        int RecurrenceInterval
        int RecurrenceDaysMask
        DateTime RecurrenceEndDate
    }
    EVENT_COUNTDOWN {
        guid UserId FK
        string Title
        DateTime TargetAt
    }
```

## Health & Habits

```mermaid
erDiagram
    USER ||--o{ MEDICINE : owns
    MEDICINE ||--o{ MEDICINE_TIME : scheduled_at
    MEDICINE ||--o{ MEDICINE_LOG : logged_as
    MEDICINE ||--o{ MEDICINE_REMINDER_LOG : reminded_via
    USER ||--o{ HABIT : owns
    HABIT ||--o{ HABIT_LOG : checked_in_as
    USER ||--o{ SLEEP_LOG : logs
    ACHIEVEMENT ||--o{ USER_ACHIEVEMENT : earned_as
    USER ||--o{ USER_ACHIEVEMENT : earns

    MEDICINE {
        guid UserId FK
        string Name
        string Dosage
        DateOnly StartDate
        DateOnly EndDate
        int RepeatDaysMask
        bool IsActive
    }
    MEDICINE_TIME {
        guid MedicineId FK
        int Hour
        int Minute
    }
    MEDICINE_LOG {
        guid MedicineId FK
        DateOnly ScheduledDate
        string Status
    }
    MEDICINE_REMINDER_LOG {
        guid MedicineId FK
        DateOnly Date
        int Hour
        int Minute
    }
    HABIT {
        guid UserId FK
        string Title
        int RepeatDaysMask
        bool IsActive
    }
    HABIT_LOG {
        guid HabitId FK
        DateOnly Date
        bool Completed
    }
    SLEEP_LOG {
        guid UserId FK
        DateOnly Date
        DateTime BedTime
        DateTime WakeTime
        int DurationMinutes
        string Quality
    }
    ACHIEVEMENT {
        string Code
        string Title
        string Emoji
    }
    USER_ACHIEVEMENT {
        guid UserId FK
        guid AchievementId FK
        DateTime EarnedAt
    }
```

## Religion & Prayer Center

```mermaid
erDiagram
    RELIGION ||--o{ FESTIVAL_CALENDAR_ENTRY : has
    RELIGION ||--o{ DAILY_QUOTE : has
    USER ||--o{ USER_PRAYER_LOG : logs

    RELIGION {
        string Code
        string Name
        bool IsSystem
    }
    FESTIVAL_CALENDAR_ENTRY {
        guid ReligionId FK
        string Name
        string Description
        DateOnly Date
        bool IsSystem
    }
    DAILY_QUOTE {
        guid ReligionId FK "nullable - null means universal"
        string Text
        string Source
    }
    USER_PRAYER_LOG {
        guid UserId FK
        DateOnly Date
        string PrayerName
        bool Completed
    }
```

## Notifications & Devices

```mermaid
erDiagram
    USER ||--o{ NOTIFICATION : receives
    USER ||--o{ USER_DEVICE : registers

    NOTIFICATION {
        guid UserId FK
        string Title
        string Message
        bool IsRead
    }
    USER_DEVICE {
        guid UserId FK
        string FcmToken
        string Platform
    }
```

## Admin / Audit

```mermaid
erDiagram
    USER ||--o{ AUDIT_LOG : performs

    AUDIT_LOG {
        guid ActorId FK
        string Action
        string EntityType
        guid EntityId
        string Description
        string IpAddress
    }
```

Note: `AuditLog` extends `BaseEntity` (not `AuditableEntity`) - it has its own explicit `CreatedAt`
but no `ModifiedAt`/`IsDeleted`, since an audit trail is append-only by design.
