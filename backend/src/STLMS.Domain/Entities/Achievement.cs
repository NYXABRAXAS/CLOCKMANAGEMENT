using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>Seed-defined, admin-editable-later achievement definitions - see DbSeeder for the
/// initial set. Introduced in the Habit Tracker milestone but written generically enough for any
/// later module (Medicine adherence streaks, Pomodoro focus totals, etc.) to award the same way.</summary>
public class Achievement : AuditableEntity
{
    public string Code { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? Emoji { get; set; }
    public bool IsSystem { get; set; }
}

public class UserAchievement : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public Guid AchievementId { get; set; }
    public Achievement Achievement { get; set; } = default!;
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}

public static class AchievementCodes
{
    public const string HabitStreak7 = "HABIT_STREAK_7";
    public const string HabitStreak30 = "HABIT_STREAK_30";
    public const string HabitFirstCheckIn = "HABIT_FIRST_CHECKIN";
    public const string HabitCheckIns100 = "HABIT_CHECKINS_100";
}
