using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Puerto de SALIDA para consultas específicas de WeatherForecast.
/// 
/// PRINCIPIO: Query Services manejan consultas complejas del dominio.
/// Cada método tiene una intención clara de negocio, no técnica.
/// 
/// Ventajas:
/// - Cada interfaz cuenta una historia de negocio
/// - Consultas complejas fuera del repositorio
/// - No filtras con expresiones genéricas
/// - Fácil de testear y mockear
/// </summary>
public interface IWeatherForecastQueryService
{
    /// <summary>
    /// Obtiene pronósticos por rango de fechas.
    /// Ejemplo de consulta de negocio específica.
    /// </summary>
    Task<IEnumerable<WeatherForecast>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene pronósticos con temperatura superior a un umbral.
    /// Ejemplo de consulta de negocio específica.
    /// </summary>
    Task<IEnumerable<WeatherForecast>> GetByMinimumTemperatureAsync(
        int minimumTemperatureC, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca pronósticos por criterios específicos.
    /// Ejemplo de consulta compleja con múltiples filtros.
    /// </summary>
    Task<IEnumerable<WeatherForecast>> SearchAsync(
        WeatherForecastSearchCriteria criteria, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene estadísticas de temperatura.
    /// Ejemplo de consulta agregada.
    /// </summary>
    Task<WeatherTemperatureStats> GetTemperatureStatsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Criterios de búsqueda para pronósticos del tiempo.
/// Value Object que encapsula los filtros de búsqueda.
/// </summary>
public record WeatherForecastSearchCriteria
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? MinTemperatureC { get; init; }
    public int? MaxTemperatureC { get; init; }
    public string? SummaryContains { get; init; }
}

/// <summary>
/// Estadísticas de temperatura.
/// Value Object que representa datos agregados.
/// </summary>
public record WeatherTemperatureStats
{
    public required int AverageTemperatureC { get; init; }
    public required int MinTemperatureC { get; init; }
    public required int MaxTemperatureC { get; init; }
    public required int TotalForecasts { get; init; }
}
