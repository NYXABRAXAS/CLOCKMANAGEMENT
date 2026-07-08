namespace STLMS.Application.EventCountdowns.Dtos;

public record EventCountdownDto(Guid Id, string Title, DateOnly TargetDate, string? Emoji, string? Color);
