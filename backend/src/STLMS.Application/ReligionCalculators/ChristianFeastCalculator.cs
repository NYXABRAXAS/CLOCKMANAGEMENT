namespace STLMS.Application.ReligionCalculators;

/// <summary>Real computation, not guessed dates - the Anonymous Gregorian algorithm (aka Meeus/
/// Jones/Butcher Computus) for Easter Sunday, with every other movable feast derived as a fixed
/// offset from it. This is the same algorithm used by real liturgical calendars.</summary>
public static class ChristianFeastCalculator
{
    public static DateOnly EasterSunday(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var month = (h + l - 7 * m + 114) / 31;
        var day = (h + l - 7 * m + 114) % 31 + 1;
        return new DateOnly(year, month, day);
    }

    public record MovableFeast(string Name, string Emoji, DateOnly Date);

    public static IReadOnlyList<MovableFeast> MovableFeastsForYear(int year)
    {
        var easter = EasterSunday(year);
        return
        [
            new MovableFeast("Ash Wednesday", "✝️", easter.AddDays(-46)),
            new MovableFeast("Palm Sunday", "🌿", easter.AddDays(-7)),
            new MovableFeast("Good Friday", "✝️", easter.AddDays(-2)),
            new MovableFeast("Easter Sunday", "🐣", easter),
            new MovableFeast("Ascension Day", "☁️", easter.AddDays(39)),
            new MovableFeast("Pentecost", "🔥", easter.AddDays(49)),
        ];
    }

    public static IReadOnlyList<MovableFeast> FixedFeastsForYear(int year) =>
    [
        new MovableFeast("Epiphany", "⭐", new DateOnly(year, 1, 6)),
        new MovableFeast("All Saints' Day", "🕯️", new DateOnly(year, 11, 1)),
        new MovableFeast("Christmas Day", "🎄", new DateOnly(year, 12, 25)),
    ];
}
