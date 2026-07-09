using STLMS.Application.ReligionCalculators;
using Xunit;

namespace STLMS.Application.Tests.ReligionCalculators;

public class QiblaCalculatorTests
{
    [Fact]
    public void BearingDegrees_London_MatchesRealWorldQiblaDirection()
    {
        // London -> Mecca is a well-known, widely-published bearing of roughly 119 degrees -
        // independently confirmed via a live Aladhan API call during this project's own build.
        var bearing = QiblaCalculator.BearingDegrees(51.5074, -0.1278);

        Assert.InRange(bearing, 117, 121);
    }

    [Fact]
    public void BearingDegrees_NewYork_PointsRoughlyNortheast()
    {
        // New York -> Mecca is commonly cited as approximately 58 degrees (northeast).
        var bearing = QiblaCalculator.BearingDegrees(40.7128, -74.0060);

        Assert.InRange(bearing, 55, 61);
    }

    [Theory]
    [InlineData(51.5074, -0.1278)]
    [InlineData(-33.8688, 151.2093)] // Sydney
    [InlineData(35.6762, 139.6503)] // Tokyo
    [InlineData(0, 0)]
    public void BearingDegrees_IsAlwaysWithinACompassRange(double lat, double lon)
    {
        var bearing = QiblaCalculator.BearingDegrees(lat, lon);

        Assert.InRange(bearing, 0, 360);
    }
}
