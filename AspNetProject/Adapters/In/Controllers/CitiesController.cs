using AspNetProject.Domain.Ports.Out;
using AspNetProject.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace AspNetProject.Adapters.In.Controllers;

/// <summary>
/// Controlador REST para demostrar paginación profesional.
/// Ejemplo completo de cómo usar PageRequest y PagedResult.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly ICityQueryService _cityQueryService;
    private readonly ILogger<CitiesController> _logger;

    public CitiesController(
        ICityQueryService cityQueryService,
        ILogger<CitiesController> logger)
    {
        _cityQueryService = cityQueryService;
        _logger = logger;
    }

    /// <summary>
    /// Búsqueda de ciudades con paginación profesional.
    /// Similar a Spring Boot: GET /api/cities?country=Colombia&page=0&size=20&sortBy=Name&ascending=true
    /// </summary>
    /// <param name="country">Filtro por país (opcional)</param>
    /// <param name="nameContains">Filtro por nombre parcial (opcional)</param>
    /// <param name="page">Número de página (0-indexed, como Spring Boot)</param>
    /// <param name="size">Tamaño de página (default: 20, max: 100)</param>
    /// <param name="sortBy">Campo para ordenar (Name, Country, CreatedAt)</param>
    /// <param name="ascending">Dirección del ordenamiento (true = ASC, false = DESC)</param>
    /// <returns>Resultado paginado con metadatos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CityDto>>> Search(
        [FromQuery] string? country = null,
        [FromQuery] string? nameContains = null,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20,
        [FromQuery] string? sortBy = "Name",
        [FromQuery] bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Crear criterios de búsqueda
            var criteria = new CitySearchCriteria
            {
                Country = country,
                NameContains = nameContains
            };

            // 2. Crear solicitud de paginación (similar a Pageable de Spring)
            var pageRequest = PageRequest.Of(page, size, sortBy ?? "Name", ascending);

            // 3. Ejecutar búsqueda paginada
            var result = await _cityQueryService.SearchAsync(criteria, pageRequest, cancellationToken);

            // 4. Mapear entidades a DTOs
            var dtoResult = result.Map(city => new CityDto
            {
                Id = city.Id,
                Name = city.Name,
                Country = city.Country,
                Latitude = city.Latitude,
                Longitude = city.Longitude,
                CreatedAt = city.CreatedAt
            });

            _logger.LogInformation(
                "Cities search: page={Page}, size={Size}, totalCount={TotalCount}, totalPages={TotalPages}",
                dtoResult.PageNumber, dtoResult.PageSize, dtoResult.TotalCount, dtoResult.TotalPages);

            return Ok(dtoResult);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid pagination parameters");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las ciudades (solo para catálogos pequeños).
    /// ⚠️ Lanza excepción si hay más de 1000 registros.
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<CityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CityDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var cities = await _cityQueryService.GetAllAsync(cancellationToken);
            
            var dtos = cities.Select(city => new CityDto
            {
                Id = city.Id,
                Name = city.Name,
                Country = city.Country,
                Latitude = city.Latitude,
                Longitude = city.Longitude,
                CreatedAt = city.CreatedAt
            });

            return Ok(dtos);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "GetAll failed due to too many records");
            return BadRequest(new 
            { 
                error = ex.Message,
                suggestion = "Use GET /api/cities with pagination instead"
            });
        }
    }

    /// <summary>
    /// Obtiene ciudades por país.
    /// </summary>
    [HttpGet("by-country/{country}")]
    [ProducesResponseType(typeof(IEnumerable<CityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CityDto>>> GetByCountry(
        string country,
        CancellationToken cancellationToken = default)
    {
        var cities = await _cityQueryService.GetByCountryAsync(country, cancellationToken);
        
        var dtos = cities.Select(city => new CityDto
        {
            Id = city.Id,
            Name = city.Name,
            Country = city.Country,
            Latitude = city.Latitude,
            Longitude = city.Longitude,
            CreatedAt = city.CreatedAt
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene estadísticas de ciudades por país.
    /// </summary>
    [HttpGet("statistics/{country}")]
    [ProducesResponseType(typeof(CityStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<CityStatistics>> GetStatistics(
        string country,
        CancellationToken cancellationToken = default)
    {
        var stats = await _cityQueryService.GetStatisticsByCountryAsync(country, cancellationToken);
        return Ok(stats);
    }
}

/// <summary>
/// DTO para transferir datos de City al cliente.
/// Separamos el modelo de dominio del modelo de API.
/// </summary>
public record CityDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Country { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required DateTime CreatedAt { get; init; }
}
