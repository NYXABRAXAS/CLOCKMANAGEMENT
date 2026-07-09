using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>A festival/holy day for a specific real calendar date. For religions with no free
/// computable-or-API calendar (Sikhism, Buddhism, Jainism), these are seeded static dates that
/// need periodic admin re-seeding for future years - documented honestly rather than pretending
/// to compute lunar/lunisolar dates this project doesn't have a real source for. Christianity's
/// entries ARE computed (Easter via the Computus algorithm) at seed time, not guessed.</summary>
public class FestivalCalendarEntry : AuditableEntity
{
    public Guid ReligionId { get; set; }
    public Religion Religion { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateOnly Date { get; set; }
    public string? Emoji { get; set; }
    public bool IsSystem { get; set; }
}
