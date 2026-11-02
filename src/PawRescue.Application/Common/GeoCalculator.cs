namespace PawRescue.Application.Common;

/// <summary>
/// Plain-math geo helpers so we don't need PostGIS for the capstone.
/// Nearby search = cheap bounding-box filter in SQL (uses the lat/lng index)
/// followed by an exact Haversine distance in memory.
/// </summary>
public static class GeoCalculator
{
    private const double EarthRadiusKm = 6371.0;

    public static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return EarthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    /// <summary>Returns (minLat, maxLat, minLon, maxLon) of a square around a point.</summary>
    public static (double MinLat, double MaxLat, double MinLon, double MaxLon) BoundingBox(double lat, double lon, double radiusKm)
    {
        var latDelta = radiusKm / 111.32;
        var lonDelta = radiusKm / (111.32 * Math.Cos(ToRad(lat)));
        return (lat - latDelta, lat + latDelta, lon - lonDelta, lon + lonDelta);
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;
}
