using PawRescue.Domain.Common;

namespace PawRescue.Domain.Entities;

/// <summary>
/// GPS position of a report. Kept in its own table so that, later,
/// the same location can be shared/geocoded/upgraded to PostGIS without
/// touching the AnimalReports table.
/// </summary>
public class Location : BaseEntity
{
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string? Address { get; private set; }
    public string? Barangay { get; private set; }
    public string? City { get; private set; }
    public string? Province { get; private set; }

    private Location() { }

    public Location(double latitude, double longitude, string? address = null,
                    string? barangay = null, string? city = null, string? province = null)
    {
        if (latitude is < -90 or > 90) throw new DomainException("Latitude must be between -90 and 90.");
        if (longitude is < -180 or > 180) throw new DomainException("Longitude must be between -180 and 180.");

        Latitude = latitude;
        Longitude = longitude;
        Address = address;
        Barangay = barangay;
        City = city;
        Province = province;
    }

    public void Update(double latitude, double longitude, string? address)
    {
        if (latitude is < -90 or > 90) throw new DomainException("Latitude must be between -90 and 90.");
        if (longitude is < -180 or > 180) throw new DomainException("Longitude must be between -180 and 180.");
        Latitude = latitude;
        Longitude = longitude;
        Address = address;
        Touch();
    }
}
