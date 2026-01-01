namespace AspNetProject.Domain.Models;

/// <summary>
/// Entidad de dominio que representa una ciudad
/// </summary>
public class City : IEntity<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public City(string name, string country, double latitude, double longitude)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("City name is required");

        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));

        Name = name;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;
        CreatedAt = DateTime.UtcNow;
    }
    
    public void Update(string name, string country, double latitude, double longitude)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("City name is required");

        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));

        Name = name;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = DateTime.UtcNow;
    }
}
