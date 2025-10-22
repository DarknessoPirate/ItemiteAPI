using Microsoft.EntityFrameworkCore;

namespace Domain.ValueObjects;

[Owned]
public class Location
{
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
}