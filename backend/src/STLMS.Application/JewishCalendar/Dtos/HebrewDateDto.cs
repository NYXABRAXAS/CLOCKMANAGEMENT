namespace STLMS.Application.JewishCalendar.Dtos;

public record HebrewDateDto(int HebrewYear, string HebrewMonth, int HebrewDay, string Formatted, IReadOnlyList<string> Events);
