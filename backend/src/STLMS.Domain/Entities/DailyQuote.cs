using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>ReligionId null = universal (shown to everyone regardless of religion setting).</summary>
public class DailyQuote : AuditableEntity
{
    public Guid? ReligionId { get; set; }
    public Religion? Religion { get; set; }
    public string Text { get; set; } = default!;
    public string? Source { get; set; }
}
