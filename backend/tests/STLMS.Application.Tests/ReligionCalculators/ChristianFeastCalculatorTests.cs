using STLMS.Application.ReligionCalculators;
using Xunit;

namespace STLMS.Application.Tests.ReligionCalculators;

public class ChristianFeastCalculatorTests
{
    // Real, independently-known Easter Sunday dates - not values the algorithm itself produced.
    [Theory]
    [InlineData(2024, 3, 31)]
    [InlineData(2025, 4, 20)]
    [InlineData(2026, 4, 5)]
    [InlineData(2027, 3, 28)]
    public void EasterSunday_MatchesKnownHistoricalDates(int year, int month, int day)
    {
        var easter = ChristianFeastCalculator.EasterSunday(year);

        Assert.Equal(new DateOnly(year, month, day), easter);
    }

    [Fact]
    public void EasterSunday_IsAlwaysASunday()
    {
        foreach (var year in Enumerable.Range(2020, 20))
        {
            Assert.Equal(DayOfWeek.Sunday, ChristianFeastCalculator.EasterSunday(year).DayOfWeek);
        }
    }

    [Fact]
    public void MovableFeastsForYear_AreAllOffsetFromEasterByTheCorrectFixedInterval()
    {
        var easter = ChristianFeastCalculator.EasterSunday(2026);
        var feasts = ChristianFeastCalculator.MovableFeastsForYear(2026).ToDictionary(f => f.Name);

        Assert.Equal(easter.AddDays(-46), feasts["Ash Wednesday"].Date);
        Assert.Equal(easter.AddDays(-7), feasts["Palm Sunday"].Date);
        Assert.Equal(easter.AddDays(-2), feasts["Good Friday"].Date);
        Assert.Equal(easter, feasts["Easter Sunday"].Date);
        Assert.Equal(easter.AddDays(39), feasts["Ascension Day"].Date);
        Assert.Equal(easter.AddDays(49), feasts["Pentecost"].Date);
    }

    [Fact]
    public void FixedFeastsForYear_UseTheCalendarDateRegardlessOfEaster()
    {
        var feasts = ChristianFeastCalculator.FixedFeastsForYear(2026).ToDictionary(f => f.Name);

        Assert.Equal(new DateOnly(2026, 1, 6), feasts["Epiphany"].Date);
        Assert.Equal(new DateOnly(2026, 11, 1), feasts["All Saints' Day"].Date);
        Assert.Equal(new DateOnly(2026, 12, 25), feasts["Christmas Day"].Date);
    }
}
