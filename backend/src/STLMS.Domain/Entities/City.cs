using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>Global reference data (seeded, later admin-editable) - not owned by any one user.
/// Backs both the World Clock city picker and the Timezone Converter.</summary>
public class City : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string CountryCode { get; set; } = default!;
    public string TimezoneId { get; set; } = default!; // IANA id, e.g. "Asia/Kolkata"
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsSystem { get; set; }
}
