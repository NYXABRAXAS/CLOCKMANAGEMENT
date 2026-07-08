namespace STLMS.Application.Achievements.Dtos;

public record UserAchievementDto(string Code, string Title, string? Description, string? Emoji, DateTime EarnedAt);
