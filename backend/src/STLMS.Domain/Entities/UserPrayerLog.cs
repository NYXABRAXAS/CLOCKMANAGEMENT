using STLMS.Domain.Common;

namespace STLMS.Domain.Entities;

/// <summary>One prayer completion check-in (Fajr/Dhuhr/Asr/Maghrib/Isha) for a given day - the
/// same check-in pattern as HabitLog, feeding into the Productivity Dashboard milestone.</summary>
public class UserPrayerLog : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public DateOnly Date { get; set; }
    public string PrayerName { get; set; } = default!; // "Fajr" | "Dhuhr" | "Asr" | "Maghrib" | "Isha"
    public bool Completed { get; set; }
}
