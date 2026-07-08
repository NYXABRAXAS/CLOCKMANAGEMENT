using STLMS.Domain.Common;
using STLMS.Domain.Enums;

namespace STLMS.Domain.Entities;

/// <summary>Days-of-week repeat pattern as a bitmask (Sunday=1, Monday=2, ... Saturday=64). 0 means
/// "one-time" - the alarm fires once on its next matching Hour:Minute and is then auto-disabled.</summary>
public static class AlarmDayMask
{
    public const int Sunday = 1;
    public const int Monday = 2;
    public const int Tuesday = 4;
    public const int Wednesday = 8;
    public const int Thursday = 16;
    public const int Friday = 32;
    public const int Saturday = 64;
    public const int Everyday = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday;

    public static int ForDayOfWeek(DayOfWeek day) => day switch
    {
        DayOfWeek.Sunday => Sunday,
        DayOfWeek.Monday => Monday,
        DayOfWeek.Tuesday => Tuesday,
        DayOfWeek.Wednesday => Wednesday,
        DayOfWeek.Thursday => Thursday,
        DayOfWeek.Friday => Friday,
        DayOfWeek.Saturday => Saturday,
        _ => 0,
    };
}

public class Alarm : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string Label { get; set; } = default!;
    public int Hour { get; set; } // 0-23, in the user's own TimezoneId
    public int Minute { get; set; } // 0-59
    public int RepeatDaysMask { get; set; } // 0 = one-time
    public bool IsEnabled { get; set; } = true;
    public string SoundId { get; set; } = "classic";
    public bool SnoozeEnabled { get; set; } = true;
    public int SnoozeMinutes { get; set; } = 9;
    public AlarmChallengeType ChallengeType { get; set; } = AlarmChallengeType.None;

    public ICollection<AlarmHistory> History { get; set; } = new List<AlarmHistory>();
}

/// <summary>One row per trigger/snooze/dismiss event - both an audit trail and how
/// AlarmTriggerService avoids re-firing the same alarm twice within the same matching minute.</summary>
public class AlarmHistory : AuditableEntity
{
    public Guid AlarmId { get; set; }
    public Alarm Alarm { get; set; } = default!;
    public Guid UserId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public AlarmHistoryAction Action { get; set; }
}
