namespace STLMS.Application.Habits.Dtos;

public record HabitDto(
    Guid Id,
    string Title,
    string? Description,
    string? Emoji,
    string? Color,
    int RepeatDaysMask,
    bool IsActive,
    int CurrentStreak,
    int LongestStreak,
    bool CompletedToday);

public record ToggleHabitLogResult(HabitDto Habit, IReadOnlyList<string> NewlyEarnedAchievementCodes);
