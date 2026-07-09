using STLMS.Application.ReligionCalculators;
using Xunit;

namespace STLMS.Application.Tests.ReligionCalculators;

/// <summary>PanchangCalculator is explicitly a documented approximation (see its own doc-comment) -
/// these tests verify structural invariants (valid ranges, determinism, honesty flag) rather than
/// asserting specific "correct" astronomical values, since there's no real ephemeris backing it.</summary>
public class PanchangCalculatorTests
{
    [Theory]
    [InlineData(2026, 1, 1)]
    [InlineData(2026, 6, 15)]
    [InlineData(2026, 12, 31)]
    [InlineData(2000, 1, 6)] // the calculator's own reference new moon date
    public void Calculate_AlwaysReturnsValuesWithinValidRanges(int year, int month, int day)
    {
        var result = PanchangCalculator.Calculate(new DateOnly(year, month, day));

        Assert.InRange(result.TithiNumber, 1, 15);
        Assert.InRange(result.NakshatraNumber, 1, 27);
        Assert.Contains(result.Paksha, new[] { "Shukla", "Krishna" });
        Assert.True(result.IsApproximate);
    }

    [Fact]
    public void Calculate_IsDeterministic_SameDateAlwaysReturnsSameResult()
    {
        var date = new DateOnly(2026, 7, 9);

        var first = PanchangCalculator.Calculate(date);
        var second = PanchangCalculator.Calculate(date);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Calculate_AtReferenceNewMoon_IsAmavasyaInKrishnaPaksha()
    {
        // The reference new moon itself should land on (or right at the boundary of) Amavasya.
        var result = PanchangCalculator.Calculate(new DateOnly(2000, 1, 6));

        Assert.Equal(15, result.TithiNumber);
        Assert.Equal("Krishna", result.Paksha);
        Assert.Equal("Amavasya", result.TithiName);
    }
}
