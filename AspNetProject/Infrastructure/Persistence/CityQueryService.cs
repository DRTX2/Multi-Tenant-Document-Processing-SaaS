using Microsoft.EntityFrameworkCore;
using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.Out;
using AspNetProject.Infrastructure.Data;

namespace AspNetProject.Infrastructure.Persistence;

/// <summary>
/// Implementación del Query Service para City.
/// Demuestra cómo hacer consultas complejas en Infrastructure sin exponer
/// Expression<Func<>> en los puertos del dominio.
/// 
/// AQUÍ sí usamos LINQ y EF Core porque estamos en la capa de Infrastructure.
/// </summary>
public class CityQueryService : ICityQueryService
{
    private readonly ApplicationDbContext _context;

    public CityQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<City>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Límite de seguridad: verificar que no haya demasiados registros
        const int maxAllowedRecords = 1000;
        
        var count = await _context.Cities.CountAsync(cancellationToken);
        
        if (count > maxAllowedRecords)
        {
            throw new InvalidOperationException(
                $"GetAllAsync() no puede usarse cuando hay más de {maxAllowedRecords} registros. " +
                $"Actualmente hay {count} registros. " +
                $"Usa SearchAsync() con paginación en su lugar.");
        }
        
        return await _context.Cities
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<City>> GetByCountryAsync(
        string country, 
        CancellationToken cancellationToken = default)
    {
        // AQUÍ sí usamos Expression<Func<>> porque estamos en Infrastructure
        return await _context.Cities
            .Where(c => c.Country == country)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<City>> SearchByNameAsync(
        string nameContains, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Cities
            .Where(c => c.Name.Contains(nameContains))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<City>> SearchAsync(
        CitySearchCriteria criteria,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Validación de parámetros
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Límite máximo de seguridad

        var query = _context.Cities.AsQueryable();

        // Aplicar filtros dinámicamente según los criterios
        if (!string.IsNullOrWhiteSpace(criteria.Country))
            query = query.Where(c => c.Country == criteria.Country);

        if (!string.IsNullOrWhiteSpace(criteria.NameContains))
            query = query.Where(c => c.Name.Contains(criteria.NameContains));

        if (criteria.CreatedAfter.HasValue)
            query = query.Where(c => c.CreatedAt >= criteria.CreatedAfter.Value);

        if (criteria.CreatedBefore.HasValue)
            query = query.Where(c => c.CreatedAt <= criteria.CreatedBefore.Value);

        // Obtener total ANTES de paginar
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar ordenamiento dinámico
        query = ApplyOrdering(query, criteria.OrderBy, criteria.OrderAscending);

        // Aplicar paginación
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<City>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CityStatistics> GetStatisticsByCountryAsync(
        string country,
        CancellationToken cancellationToken = default)
    {
        var stats = await _context.Cities
            .Where(c => c.Country == country)
            .GroupBy(c => c.Country)
            .Select(g => new
            {
                TotalCities = g.Count(),
                OldestCity = g.OrderBy(c => c.CreatedAt).First().Name,
                NewestCity = g.OrderByDescending(c => c.CreatedAt).First().Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (stats == null)
        {
            return new CityStatistics
            {
                Country = country,
                TotalCities = 0,
                OldestCityName = string.Empty,
                NewestCityName = string.Empty
            };
        }

        return new CityStatistics
        {
            Country = country,
            TotalCities = stats.TotalCities,
            OldestCityName = stats.OldestCity,
            NewestCityName = stats.NewestCity
        };
    }

    public async Task<IEnumerable<City>> GetNearbyAsync(
        double latitude,
        double longitude,
        double radiusInDegrees = 1.0,
        CancellationToken cancellationToken = default)
    {
        // Búsqueda simple por rango de coordenadas
        // En producción usarías PostGIS o similar para cálculos geoespaciales reales
        return await _context.Cities
            .Where(c => 
                Math.Abs(c.Latitude - latitude) <= radiusInDegrees &&
                Math.Abs(c.Longitude - longitude) <= radiusInDegrees)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Aplica ordenamiento dinámico basado en el nombre del campo.
    /// Patrón útil para APIs REST con query parameters.
    /// </summary>
    private static IQueryable<City> ApplyOrdering(
        IQueryable<City> query, 
        string? orderBy, 
        bool ascending)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            // Ordenamiento por defecto
            return query.OrderBy(c => c.Name);
        }

        // Ordenamiento dinámico basado en el campo
        return orderBy.ToLowerInvariant() switch
        {
            "name" => ascending 
                ? query.OrderBy(c => c.Name) 
                : query.OrderByDescending(c => c.Name),
            
            "country" => ascending 
                ? query.OrderBy(c => c.Country).ThenBy(c => c.Name)
                : query.OrderByDescending(c => c.Country).ThenByDescending(c => c.Name),
            
            "createdat" or "created" => ascending 
                ? query.OrderBy(c => c.CreatedAt) 
                : query.OrderByDescending(c => c.CreatedAt),
            
            "latitude" => ascending 
                ? query.OrderBy(c => c.Latitude) 
                : query.OrderByDescending(c => c.Latitude),
            
            "longitude" => ascending 
                ? query.OrderBy(c => c.Longitude) 
                : query.OrderByDescending(c => c.Longitude),
            
            // Por defecto, ordenar por nombre
            _ => query.OrderBy(c => c.Name)
        };
    }
}
