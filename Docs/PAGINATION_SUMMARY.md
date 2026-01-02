# Resumen: Paginación Profesional en .NET

## 🎯 Respuesta a tu Pregunta

### ❓ Pregunta Original:
> "¿Por qué no existe un método como `fetchAll()` (o `getAll()` en .NET)? Vengo de Spring Boot."

### ✅ Respuesta:

**En Spring Boot:**
```java
// Repository con JPA
public interface CityRepository extends JpaRepository<City, Long> {
    List<City> findAll();                    // ⚠️ Carga TODOS los registros
    Page<City> findAll(Pageable pageable);   // ✅ Paginado
}

// Uso
List<City> allCities = cityRepository.findAll(); // Peligroso si hay muchos registros
Page<City> page = cityRepository.findAll(PageRequest.of(0, 20));
```

**En .NET (este proyecto):**
```csharp
// IRepository - Solo CRUD básico
public interface IRepository<TEntity, TId>
{
    Task<TEntity?> GetByIdAsync(TId id);
    Task<TEntity> AddAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task DeleteAsync(TId id);
    // ❌ NO hay GetAllAsync() aquí
}

// ICityQueryService - Consultas complejas con paginación
public interface ICityQueryService
{
    // ✅ Paginación obligatoria para búsquedas
    Task<PagedResult<City>> SearchAsync(
        CitySearchCriteria criteria,
        PageRequest pageRequest);
    
    // ⚠️ GetAll solo para catálogos pequeños (con límite de seguridad)
    Task<IEnumerable<City>> GetAllAsync(); // Lanza excepción si > 1000 registros
}
```

## 🏗️ Arquitectura Implementada

### 1. **Value Objects** (Domain/ValueObjects/)

#### `PageRequest.cs` - Equivalente a `Pageable` de Spring
```csharp
var pageRequest = PageRequest.Of(0, 20, "Name", true);
// page: 0 (0-indexed como Spring Boot)
// size: 20
// sortBy: "Name"
// ascending: true
```

#### `PagedResult<T>.cs` - Equivalente a `Page<T>` de Spring
```csharp
public record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; }
    public int PageNumber { get; init; }      // 0-indexed
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; }            // Calculado
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
}
```

### 2. **Query Service** (Domain/Ports/Out/)

```csharp
public interface ICityQueryService
{
    Task<PagedResult<City>> SearchAsync(
        CitySearchCriteria criteria,
        PageRequest pageRequest,
        CancellationToken cancellationToken = default);
}
```

### 3. **Implementación** (Infrastructure/Persistence/)

```csharp
public class CityQueryService : ICityQueryService
{
    public async Task<PagedResult<City>> SearchAsync(
        CitySearchCriteria criteria,
        PageRequest pageRequest,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Cities.AsQueryable();
        
        // 1. Aplicar filtros
        if (!string.IsNullOrWhiteSpace(criteria.Country))
            query = query.Where(c => c.Country == criteria.Country);
        
        // 2. Contar total ANTES de paginar
        var totalCount = await query.CountAsync(cancellationToken);
        
        // 3. Aplicar ordenamiento
        query = ApplyOrdering(query, pageRequest.SortBy, pageRequest.SortAscending);
        
        // 4. Aplicar paginación
        var items = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.PageSize)
            .ToListAsync(cancellationToken);
        
        // 5. Retornar resultado
        return new PagedResult<City>
        {
            Items = items,
            PageNumber = pageRequest.PageNumber,
            PageSize = pageRequest.PageSize,
            TotalCount = totalCount
        };
    }
}
```

### 4. **Controller** (Adapters/In/Controllers/)

```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<CityDto>>> Search(
    [FromQuery] string? country = null,
    [FromQuery] int page = 0,
    [FromQuery] int size = 20,
    [FromQuery] string? sortBy = "Name",
    [FromQuery] bool ascending = true)
{
    var criteria = new CitySearchCriteria { Country = country };
    var pageRequest = PageRequest.Of(page, size, sortBy ?? "Name", ascending);
    var result = await _cityQueryService.SearchAsync(criteria, pageRequest);
    
    return Ok(result.Map(city => new CityDto { /* ... */ }));
}
```

## 📊 Comparación Completa

| Aspecto | Spring Boot | .NET (Este Proyecto) |
|---------|-------------|----------------------|
| **Paginación en Repository** | ✅ `Page<T> findAll(Pageable)` | ❌ NO (solo en Query Services) |
| **Objeto de Solicitud** | `Pageable` / `PageRequest` | `PageRequest` |
| **Objeto de Resultado** | `Page<T>` | `PagedResult<T>` |
| **Índice de Página** | 0-indexed | 0-indexed (igual) |
| **GetAll sin paginación** | ✅ `List<T> findAll()` | ⚠️ Solo con límite de seguridad |
| **Consultas Complejas** | Custom queries en Repository | Query Services separados |
| **Ordenamiento** | `Sort.by("field")` | `PageRequest.Of(0, 20, "Field", true)` |

## 🚀 Ventajas de este Enfoque

### 1. **Separación de Responsabilidades**
- **Repository**: Solo CRUD básico (GetById, Add, Update, Delete)
- **Query Services**: Consultas complejas con paginación

### 2. **Seguridad por Defecto**
```csharp
// Límites automáticos
private const int MaxPageSize = 100;
private const int DefaultPageSize = 10;

// GetAll con protección
if (count > 1000)
    throw new InvalidOperationException("Usa SearchAsync() con paginación");
```

### 3. **Performance Garantizado**
- Siempre paginado
- Nunca carga todo en memoria
- Funciona con millones de registros

### 4. **Flexibilidad**
```csharp
// Criterios específicos del dominio
public record CitySearchCriteria
{
    public string? Country { get; init; }
    public string? NameContains { get; init; }
    public DateTime? CreatedAfter { get; init; }
}
```

## 📝 Ejemplos de Uso

### Backend (C#)
```csharp
// Crear criterios
var criteria = new CitySearchCriteria 
{ 
    Country = "Colombia",
    NameContains = "Bog"
};

// Crear paginación
var pageRequest = PageRequest.Of(0, 20, "Name", true);

// Ejecutar búsqueda
var result = await cityQueryService.SearchAsync(criteria, pageRequest);

// Acceder a resultados
Console.WriteLine($"Total: {result.TotalCount}");
Console.WriteLine($"Pages: {result.TotalPages}");
Console.WriteLine($"Has next: {result.HasNextPage}");
```

### Frontend (TypeScript)
```typescript
// Llamada a la API
const response = await fetch(
    '/api/cities?country=Colombia&page=0&size=20&sortBy=Name&ascending=true'
);
const result = await response.json();

// Usar resultados
console.log('Items:', result.items);
console.log('Total pages:', result.totalPages);
console.log('Has next:', result.hasNextPage);
```

### cURL
```bash
# Búsqueda paginada
curl "http://localhost:5000/api/cities?country=Colombia&page=0&size=20&sortBy=Name&ascending=true"
```

## 🎓 Conclusión

### ¿Por qué NO hay `GetAll()` en el repositorio?

1. **Escalabilidad**: Evita cargar miles/millones de registros en memoria
2. **Performance**: Siempre paginado desde el día 1
3. **Arquitectura**: Separación clara entre CRUD y consultas complejas
4. **Seguridad**: Límites automáticos de tamaño de página

### ¿Cuándo usar cada uno?

| Método | Cuándo Usar |
|--------|-------------|
| `GetByIdAsync()` | Obtener UNA entidad por ID |
| `GetAllAsync()` | Solo catálogos pequeños (<1000 registros) |
| `SearchAsync()` | **SIEMPRE** para listas en producción |
| `CountAsync()` | Obtener totales sin cargar datos |

### Migración desde Spring Boot

Si vienes de Spring Boot, piensa:

```java
// Spring Boot
cityRepository.findAll(PageRequest.of(0, 20, Sort.by("name")))
```

Se convierte en:

```csharp
// .NET
var criteria = CitySearchCriteria.Empty();
var pageRequest = PageRequest.Of(0, 20, "Name", true);
await cityQueryService.SearchAsync(criteria, pageRequest);
```

## 📚 Archivos Creados

1. **`Domain/ValueObjects/PageRequest.cs`** - Solicitud de paginación
2. **`Domain/ValueObjects/PagedResult.cs`** - Resultado paginado
3. **`Domain/Ports/Out/ICityQueryService.cs`** - Puerto actualizado
4. **`Infrastructure/Persistence/CityQueryService.cs`** - Implementación actualizada
5. **`Adapters/In/Controllers/CitiesController.cs`** - Controlador de ejemplo
6. **`Docs/PAGINATION_GUIDE.md`** - Guía completa
7. **`Docs/PAGINATION_FRONTEND_EXAMPLES.md`** - Ejemplos frontend

## 🔗 Referencias

- [Documentación completa](./PAGINATION_GUIDE.md)
- [Ejemplos frontend](./PAGINATION_FRONTEND_EXAMPLES.md)
- [Arquitectura del proyecto](../ARCHITECTURE.md)

---

**¡Ahora tienes paginación profesional nivel senior en .NET!** 🚀
