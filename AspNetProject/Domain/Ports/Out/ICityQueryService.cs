using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.Out;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Puerto de SALIDA para consultas específicas de City.
/// Ejemplo de Query Service con paginación profesional.
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
    /// </summary>
    Task<PagedResult<City>> SearchAsync(
        CitySearchCriteria criteria,
        int pageNumber = 1,
        int pageSize = 20,
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
/// Value Object: Resultado paginado genérico.
/// Encapsula datos de paginación de forma inmutable.
/// </summary>
public record PagedResult<T>
{
    /// <summary>
    /// Elementos de la página actual.
    /// </summary>
    public required IEnumerable<T> Items { get; init; }
    
    /// <summary>
    /// Número de página actual (1-indexed).
    /// </summary>
    public required int PageNumber { get; init; }
    
    /// <summary>
    /// Tamaño de página.
    /// </summary>
    public required int PageSize { get; init; }
    
    /// <summary>
    /// Total de elementos en todas las páginas.
    /// </summary>
    public required int TotalCount { get; init; }
    
    /// <summary>
    /// Total de páginas.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    /// <summary>
    /// Indica si hay página anterior.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
    
    /// <summary>
    /// Indica si hay página siguiente.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
    
    /// <summary>
    /// Número de la página anterior (si existe).
    /// </summary>
    public int? PreviousPageNumber => HasPreviousPage ? PageNumber - 1 : null;
    
    /// <summary>
    /// Número de la página siguiente (si existe).
    /// </summary>
    public int? NextPageNumber => HasNextPage ? PageNumber + 1 : null;
}

/// <summary>
/// Value Object: Criterios de búsqueda para ciudades.
/// </summary>
public record CitySearchCriteria
{
    public string? Country { get; init; }
    public string? NameContains { get; init; }
    public DateTime? CreatedAfter { get; init; }
    public DateTime? CreatedBefore { get; init; }
    
    /// <summary>
    /// Campo por el cual ordenar los resultados.
    /// </summary>
    public string? OrderBy { get; init; }
    
    /// <summary>
    /// Dirección del ordenamiento (true = ascendente, false = descendente).
    /// </summary>
    public bool OrderAscending { get; init; } = true;
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
