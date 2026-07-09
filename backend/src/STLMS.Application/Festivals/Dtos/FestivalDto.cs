namespace STLMS.Application.Festivals.Dtos;

public record FestivalDto(Guid Id, string ReligionCode, string ReligionName, string Name, string? Description, DateOnly Date, string? Emoji, int DaysAway);
