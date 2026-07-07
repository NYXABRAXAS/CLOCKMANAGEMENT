using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>Data-driven so Admins can add a custom "Other" religion variant without a code
/// change. Full prayer/festival data model is built out in the Religion &amp; Prayer Center
/// milestone - this minimal shape exists now only so User can reference it.</summary>
public class Religion : AuditableEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int SortOrder { get; set; }
    public bool IsSystem { get; set; }
}

public static class ReligionCodes
{
    public const string Islam = "ISLAM";
    public const string Hinduism = "HINDUISM";
    public const string Christianity = "CHRISTIANITY";
    public const string Sikhism = "SIKHISM";
    public const string Buddhism = "BUDDHISM";
    public const string Jainism = "JAINISM";
    public const string Judaism = "JUDAISM";
    public const string Other = "OTHER";
}
