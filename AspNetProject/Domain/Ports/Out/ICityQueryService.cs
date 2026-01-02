using AspNetProject.Domain.Models;
using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Puerto de SALIDA para consultas específicas de City.
/// Ejemplo de Query Service con paginación profesional.
/// 
/// PATRÓN: Query Services separan las consultas complejas del repositorio genérico.
/// Esto permite optimizaciones específicas del dominio sin contaminar IRepository.
/// </summary>
public interface ICityQueryService
{
    /// <summary>
    /// Obtiene todas las ciudades.
    /// 
    /// ⚠️ ADVERTENCIA: Solo para catálogos pequeños.
    /// Si hay más de 1000 registros, lanzará InvalidOperationException.
    /// Para conjuntos grandes, usa SearchAsync() con paginación.
    /// </summary>
    Task<IEnumerable<City>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca ciudades por país.
    /// </summary>
    Task<IEnumerable<City>> GetByCountryAsync(
        string country, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca ciudades por nombre parcial.
    /// </summary>
    Task<IEnumerable<City>> SearchByNameAsync(
        string nameContains, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Búsqueda avanzada con múltiples criterios y paginación.
    /// Este es el patrón recomendado para consultas complejas en producción.
    /// 
    /// EJEMPLO DE USO:
    /// <code>
    /// var criteria = new CitySearchCriteria 
    /// { 
    ///     Country = "Colombia",
    ///     NameContains = "Bog"
    /// };
    /// var pageRequest = PageRequest.Of(0, 20, "Name", true);
    /// var result = await cityQueryService.SearchAsync(criteria, pageRequest);
    /// </code>
    /// </summary>
    Task<PagedResult<City>> SearchAsync(
        CitySearchCriteria criteria,
        PageRequest pageRequest,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene estadísticas agregadas por país.
    /// </summary>
    Task<CityStatistics> GetStatisticsByCountryAsync(
        string country,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene ciudades cercanas a una ubicación.
    /// </summary>
    Task<IEnumerable<City>> GetNearbyAsync(
        double latitude,
        double longitude,
        double radiusInDegrees = 1.0,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Value Object: Criterios de búsqueda para ciudades.
/// Encapsula los filtros de búsqueda de forma inmutable.
/// </summary>
public record CitySearchCriteria
{
    public string? Country { get; init; }
    public string? NameContains { get; init; }
    public DateTime? CreatedAfter { get; init; }
    public DateTime? CreatedBefore { get; init; }
    
    /// <summary>
    /// Crea criterios vacíos (sin filtros).
    /// </summary>
    public static CitySearchCriteria Empty() => new();
}

/// <summary>
/// Value Object: Estadísticas de ciudades por país.
/// </summary>
public record CityStatistics
{
    public required string Country { get; init; }
    public required int TotalCities { get; init; }
    public required string OldestCityName { get; init; }
    public required string NewestCityName { get; init; }
}
