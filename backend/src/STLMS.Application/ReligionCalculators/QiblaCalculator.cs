namespace STLMS.Application.ReligionCalculators;

/// <summary>Great-circle initial bearing from a point to the Kaaba - legitimate to hand-roll
/// (unlike prayer times, which depend on real astronomical/jurisprudential calculation methods
/// best left to Aladhan) since this is just spherical trigonometry with two fixed coordinates.</summary>
public static class QiblaCalculator
{
    private const double MeccaLatitude = 21.4225;
    private const double MeccaLongitude = 39.8262;

    public static double BearingDegrees(double fromLatitude, double fromLongitude)
    {
        var phi1 = ToRadians(fromLatitude);
        var phi2 = ToRadians(MeccaLatitude);
        var deltaLambda = ToRadians(MeccaLongitude - fromLongitude);

        var y = Math.Sin(deltaLambda) * Math.Cos(phi2);
        var x = Math.Cos(phi1) * Math.Sin(phi2) - Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(deltaLambda);
        var bearing = Math.Atan2(y, x) * (180 / Math.PI);

        return (bearing + 360) % 360;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
