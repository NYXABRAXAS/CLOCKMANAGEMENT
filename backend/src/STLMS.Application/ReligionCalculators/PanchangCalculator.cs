namespace STLMS.Application.ReligionCalculators;

/// <summary>Simplified, explicitly APPROXIMATE Panchang (tithi/paksha/nakshatra) via synodic- and
/// sidereal-month day-counting from a known reference new moon - not a real ephemeris. No free
/// API or .NET-portable library exists for real Panchang calculation (commercial APIs need paid
/// keys; open-source calculators are Python/JS-only with no .NET port), so this ships a documented
/// approximation rather than nothing - flagged as approximate in both the API response and the UI.
/// A real ephemeris-backed implementation is the documented upgrade path once that's worth
/// building (a paid API or a Python microservice).</summary>
public static class PanchangCalculator
{
    private static readonly DateTime ReferenceNewMoonUtc = new(2000, 1, 6, 18, 14, 0, DateTimeKind.Utc);
    private const double SynodicMonthDays = 29.530588853;
    private const double SiderealMonthDays = 27.321661;

    private static readonly string[] TithiNames =
    [
        "Pratipada", "Dwitiya", "Tritiya", "Chaturthi", "Panchami", "Shashthi", "Saptami", "Ashtami",
        "Navami", "Dashami", "Ekadashi", "Dwadashi", "Trayodashi", "Chaturdashi", "Purnima/Amavasya",
    ];

    private static readonly string[] NakshatraNames =
    [
        "Ashwini", "Bharani", "Krittika", "Rohini", "Mrigashira", "Ardra", "Punarvasu", "Pushya",
        "Ashlesha", "Magha", "Purva Phalguni", "Uttara Phalguni", "Hasta", "Chitra", "Swati",
        "Vishakha", "Anuradha", "Jyeshtha", "Mula", "Purva Ashadha", "Uttara Ashadha", "Shravana",
        "Dhanishta", "Shatabhisha", "Purva Bhadrapada", "Uttara Bhadrapada", "Revati",
    ];

    public record PanchangResult(int TithiNumber, string TithiName, string Paksha, int NakshatraNumber, string NakshatraName, bool IsApproximate);

    public static PanchangResult Calculate(DateOnly date)
    {
        var atNoonUtc = new DateTime(date.Year, date.Month, date.Day, 12, 0, 0, DateTimeKind.Utc);
        var daysSinceReference = (atNoonUtc - ReferenceNewMoonUtc).TotalDays;

        var moonAge = daysSinceReference % SynodicMonthDays;
        if (moonAge < 0) moonAge += SynodicMonthDays;
        var tithiIndex = (int)(moonAge / (SynodicMonthDays / 30)); // 0-29
        var isShuklaPaksha = tithiIndex < 15;
        var tithiInPaksha = (tithiIndex % 15) + 1; // 1-15
        var tithiName = tithiInPaksha == 15 ? (isShuklaPaksha ? "Purnima" : "Amavasya") : TithiNames[tithiInPaksha - 1];

        var siderealPosition = daysSinceReference % SiderealMonthDays;
        if (siderealPosition < 0) siderealPosition += SiderealMonthDays;
        var nakshatraIndex = (int)(siderealPosition / (SiderealMonthDays / 27)); // 0-26

        return new PanchangResult(
            tithiInPaksha, tithiName, isShuklaPaksha ? "Shukla" : "Krishna",
            nakshatraIndex + 1, NakshatraNames[nakshatraIndex], true);
    }
}
