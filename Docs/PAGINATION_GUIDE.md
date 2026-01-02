# Paginación en .NET: Guía Completa (Spring Boot → .NET)

## 📚 Comparación: Spring Boot vs .NET

### Spring Boot (JPA/Spring Data)
```java
// Repository
public interface CityRepository extends JpaRepository<City, Long> {
    Page<City> findAll(Pageable pageable);
    Page<City> findByCountry(String country, Pageable pageable);
}

// Uso
Pageable pageable = PageRequest.of(0, 20, Sort.by("name").ascending());
Page<City> result = cityRepository.findAll(pageable);
```

### .NET (Entity Framework Core)
```csharp
// Query Service (NO en el repositorio genérico)
public interface ICityQueryService
{
    Task<PagedResult<City>> SearchAsync(
        CitySearchCriteria criteria,
        PageRequest pageRequest,
        CancellationToken cancellationToken = default);
}

// Uso
var pageRequest = PageRequest.Of(0, 20, "Name", true);
var criteria = new CitySearchCriteria { Country = "Colombia" };
var result = await cityQueryService.SearchAsync(criteria, pageRequest);
```

## 🎯 ¿Por qué NO hay `GetAll()` en el repositorio?

### ❌ Problema: `GetAll()` sin paginación
```csharp
// ANTI-PATRÓN: Peligroso en producción
public interface IRepository<TEntity, TId>
{
    Task<IEnumerable<TEntity>> GetAllAsync(); // ⚠️ RIESGO
}

// Problemas:
// 1. Cargar 100,000+ registros → OutOfMemoryException
// 2. Transferir GB de datos innecesarios
// 3. No escalable
// 4. Timeout de base de datos
```

### ✅ Solución: Query Services con Paginación
```csharp
// PATRÓN CORRECTO: Separación de responsabilidades
public interface IRepository<TEntity, TId>
{
    // Solo operaciones CRUD básicas
    Task<TEntity?> GetByIdAsync(TId id);
    Task<TEntity> AddAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task DeleteAsync(TId id);
    Task<int> CountAsync(); // Para estadísticas
}

public interface ICityQueryService
{
    // Consultas complejas con paginación
    Task<PagedResult<City>> SearchAsync(
        CitySearchCriteria criteria,
        PageRequest pageRequest);
    
    // GetAll solo para catálogos pequeños (con límite de seguridad)
    Task<IEnumerable<City>> GetAllAsync(); // Lanza excepción si > 1000 registros
}
```

## 🏗️ Arquitectura de Paginación

### 1. Value Objects (Domain Layer)

#### `PageRequest.cs` - Similar a `Pageable` de Spring
```csharp
public class PageRequest
{
    public int PageNumber { get; } // 0-indexed como Spring Boot
    public int PageSize { get; }
    public string? SortBy { get; }
    public bool SortAscending { get; }
    public int Skip => PageNumber * PageSize; // Calculado

    // Factory methods
    public static PageRequest Of(int pageNumber, int pageSize)
    public static PageRequest Of(int pageNumber, int pageSize, string sortBy, bool ascending = true)
    
    // Navegación
    public PageRequest NextPage()
    public PageRequest PreviousPage()
}
```

#### `PagedResult<T>.cs` - Similar a `Page<T>` de Spring
```csharp
public record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    
    // Propiedades calculadas
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 0;
    public bool HasNextPage => PageNumber < TotalPages - 1;
    
    // Utilidades
    public static PagedResult<T> Empty(int pageNumber, int pageSize)
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapper)
}
```

### 2. Query Service (Domain/Ports/Out)

```csharp
public interface ICityQueryService
{
    /// <summary>
    /// Búsqueda avanzada con paginación.
    /// PATRÓN RECOMENDADO para consultas en producción.
    /// </summary>
    Task<PagedResult<City>> SearchAsync(
        CitySearchCriteria criteria,
        PageRequest pageRequest,
        CancellationToken cancellationToken = default);
}

public record CitySearchCriteria
{
    public string? Country { get; init; }
    public string? NameContains { get; init; }
    public DateTime? CreatedAfter { get; init; }
    public DateTime? CreatedBefore { get; init; }
}
```

### 3. Implementación (Infrastructure Layer)

```csharp
public class CityQueryService : ICityQueryService
{
    private readonly ApplicationDbContext _context;

    public async Task<PagedResult<City>> SearchAsync(
        CitySearchCriteria criteria,
        PageRequest pageRequest,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Cities.AsQueryable();

        // 1. Aplicar filtros
        if (!string.IsNullOrWhiteSpace(criteria.Country))
            query = query.Where(c => c.Country == criteria.Country);

        if (!string.IsNullOrWhiteSpace(criteria.NameContains))
            query = query.Where(c => c.Name.Contains(criteria.NameContains));

        // 2. Contar total ANTES de paginar (importante!)
        var totalCount = await query.CountAsync(cancellationToken);

        // 3. Aplicar ordenamiento
        query = ApplyOrdering(query, pageRequest.SortBy, pageRequest.SortAscending);

        // 4. Aplicar paginación
        var items = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .ToListAsync(cancellationToken);

        // 5. Retornar resultado paginado
        return new PagedResult<City>
        {
            Items = items,
            PageNumber = pageRequest.PageNumber,
            PageSize = pageRequest.PageSize,
            TotalCount = totalCount
        };
    }

    private static IQueryable<City> ApplyOrdering(
        IQueryable<City> query, 
        string? sortBy, 
        bool ascending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderBy(c => c.Name);

        return sortBy.ToLowerInvariant() switch
        {
            "name" => ascending 
                ? query.OrderBy(c => c.Name) 
                : query.OrderByDescending(c => c.Name),
            
            "country" => ascending 
                ? query.OrderBy(c => c.Country).ThenBy(c => c.Name)
                : query.OrderByDescending(c => c.Country).ThenByDescending(c => c.Name),
            
            "createdat" => ascending 
                ? query.OrderBy(c => c.CreatedAt) 
                : query.OrderByDescending(c => c.CreatedAt),
            
            _ => query.OrderBy(c => c.Name)
        };
    }
}
```

### 4. Uso en Application Layer (Use Cases)

```csharp
public class SearchCitiesUseCase
{
    private readonly ICityQueryService _cityQueryService;

    public async Task<PagedResult<CityDto>> ExecuteAsync(
        string? country,
        string? nameContains,
        int pageNumber,
        int pageSize,
        string? sortBy)
    {
        // 1. Crear criterios de búsqueda
        var criteria = new CitySearchCriteria
        {
            Country = country,
            NameContains = nameContains
        };

        // 2. Crear solicitud de paginación
        var pageRequest = PageRequest.Of(pageNumber, pageSize, sortBy ?? "Name", true);

        // 3. Ejecutar búsqueda
        var result = await _cityQueryService.SearchAsync(criteria, pageRequest);

        // 4. Mapear a DTOs
        return result.Map(city => new CityDto
        {
            Id = city.Id,
            Name = city.Name,
            Country = city.Country
        });
    }
}
```

### 5. Uso en Controller (REST API)

```csharp
[ApiController]
[Route("api/cities")]
public class CityController : ControllerBase
{
    private readonly ICityQueryService _cityQueryService;

    [HttpGet]
    public async Task<ActionResult<PagedResult<CityDto>>> Search(
        [FromQuery] string? country,
        [FromQuery] string? nameContains,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20,
        [FromQuery] string? sortBy = "Name",
        [FromQuery] bool ascending = true)
    {
        var criteria = new CitySearchCriteria
        {
            Country = country,
            NameContains = nameContains
        };

        var pageRequest = PageRequest.Of(page, size, sortBy, ascending);
        var result = await _cityQueryService.SearchAsync(criteria, pageRequest);

        // Mapear a DTOs
        var dtoResult = result.Map(city => new CityDto
        {
            Id = city.Id,
            Name = city.Name,
            Country = city.Country
        });

        return Ok(dtoResult);
    }
}

// Ejemplo de llamada:
// GET /api/cities?country=Colombia&page=0&size=20&sortBy=Name&ascending=true
```

## 📊 Respuesta JSON

```json
{
  "items": [
    {
      "id": 1,
      "name": "Bogotá",
      "country": "Colombia"
    },
    {
      "id": 2,
      "name": "Medellín",
      "country": "Colombia"
    }
  ],
  "pageNumber": 0,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true,
  "isFirstPage": true,
  "isLastPage": false,
  "previousPageNumber": null,
  "nextPageNumber": 1
}
```

## 🎯 Mejores Prácticas

### 1. **Límites de Seguridad**
```csharp
public class PageRequest
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    public PageRequest(int pageNumber, int pageSize)
    {
        if (pageSize > MaxPageSize)
            throw new ArgumentException($"Page size must be <= {MaxPageSize}");
        
        PageSize = pageSize;
    }
}
```

### 2. **GetAll con Protección**
```csharp
public async Task<IEnumerable<City>> GetAllAsync()
{
    const int maxAllowedRecords = 1000;
    var count = await _context.Cities.CountAsync();
    
    if (count > maxAllowedRecords)
    {
        throw new InvalidOperationException(
            $"GetAllAsync() no puede usarse cuando hay más de {maxAllowedRecords} registros. " +
            $"Usa SearchAsync() con paginación.");
    }
    
    return await _context.Cities.OrderBy(c => c.Name).ToListAsync();
}
```

### 3. **Contar ANTES de Paginar**
```csharp
// ✅ CORRECTO: Contar antes de Skip/Take
var totalCount = await query.CountAsync();
var items = await query.Skip(skip).Take(pageSize).ToListAsync();

// ❌ INCORRECTO: Contar después de Skip/Take
var items = await query.Skip(skip).Take(pageSize).ToListAsync();
var totalCount = items.Count(); // ⚠️ Siempre será <= pageSize
```

### 4. **Ordenamiento Dinámico Seguro**
```csharp
// Usar switch expression para evitar SQL injection
private static IQueryable<City> ApplyOrdering(IQueryable<City> query, string? sortBy)
{
    return sortBy?.ToLowerInvariant() switch
    {
        "name" => query.OrderBy(c => c.Name),
        "country" => query.OrderBy(c => c.Country),
        _ => query.OrderBy(c => c.Name) // Default seguro
    };
}
```

## 🔄 Comparación Final: Spring Boot vs .NET

| Aspecto | Spring Boot | .NET (Este Proyecto) |
|---------|-------------|----------------------|
| **Paginación en Repository** | `Page<T> findAll(Pageable)` | ❌ NO (solo en Query Services) |
| **Objeto de Solicitud** | `Pageable` / `PageRequest` | `PageRequest` |
| **Objeto de Resultado** | `Page<T>` | `PagedResult<T>` |
| **Índice de Página** | 0-indexed | 0-indexed (igual) |
| **Ordenamiento** | `Sort.by("field")` | `PageRequest.Of(0, 20, "Field", true)` |
| **Consultas Complejas** | Custom queries en Repository | Query Services separados |
| **GetAll sin paginación** | `List<T> findAll()` | Solo con límite de seguridad |

## 🚀 Ventajas de este Enfoque

1. **Separación de Responsabilidades**: Repository para CRUD, Query Services para consultas
2. **Seguridad**: Límites automáticos de tamaño de página
3. **Performance**: Siempre paginado, nunca carga todo en memoria
4. **Flexibilidad**: Criterios de búsqueda específicos del dominio
5. **Testeable**: Fácil de mockear interfaces
6. **Escalable**: Funciona con millones de registros

## 📝 Resumen

**En Spring Boot:**
- `findAll()` → Devuelve TODOS los registros (peligroso)
- `findAll(Pageable)` → Devuelve página

**En .NET (este proyecto):**
- `GetAllAsync()` → Solo para catálogos pequeños (<1000), con protección
- `SearchAsync(criteria, pageRequest)` → SIEMPRE paginado (recomendado)

**Conclusión**: En .NET moderno con arquitectura hexagonal, preferimos **Query Services** con paginación obligatoria sobre un `GetAll()` genérico en el repositorio. Esto garantiza escalabilidad y performance desde el día 1.
