namespace STLMS.API.Controllers.v1.Requests;

public record LogPrayerRequest(DateOnly Date, string PrayerName, bool Completed);
