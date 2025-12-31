using Microsoft.EntityFrameworkCore;
using AspNetProject.Domain.Models;
using AspNetProject.Infrastructure.Data;

namespace AspNetProject.Infrastructure.Repositories;

/// <summary>
/// Repositorio específico para City
/// Hereda de EfRepository para reutilizar código CRUD básico
/// Agrega métodos específicos de negocio si son necesarios
/// </summary>
public class CityRepository : EfRepository<City, int>
{
    public CityRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Ejemplo de método específico: Buscar ciudades por país
    /// </summary>
    public async Task<IEnumerable<City>> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.Country == country)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Ejemplo de método específico: Buscar ciudades cercanas a coordenadas
    /// </summary>
    public async Task<IEnumerable<City>> GetNearbyAsync(
        double latitude, 
        double longitude, 
        double radiusKm = 50, 
        CancellationToken cancellationToken = default)
    {
        // Fórmula simplificada de Haversine para distancia aproximada
        // En producción, considera usar PostGIS para consultas geoespaciales
        var cities = await DbSet.ToListAsync(cancellationToken);
        
        return cities.Where(c =>
        {
            var distance = CalculateDistance(latitude, longitude, c.Latitude, c.Longitude);
            return distance <= radiusKm;
        }).OrderBy(c => CalculateDistance(latitude, longitude, c.Latitude, c.Longitude));
    }

    /// <summary>
    /// Calcula la distancia entre dos puntos geográficos usando la fórmula de Haversine
    /// </summary>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
