namespace STLMS.Application.Common.Interfaces;

public record HebrewDateResult(int HebrewYear, string HebrewMonth, int HebrewDay, string Formatted, IReadOnlyList<string> Events);

/// <summary>Backed by the free, keyless Hebcal API.</summary>
public interface IHebrewCalendarProvider
{
    Task<HebrewDateResult> GetHebrewDateAsync(DateOnly date, CancellationToken ct = default);
}
