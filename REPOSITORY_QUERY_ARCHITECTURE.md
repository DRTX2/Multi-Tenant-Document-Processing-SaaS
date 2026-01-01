# 🏗️ Arquitectura de Repositorios y Query Services

## 📋 Índice
- [Principio Fundamental](#principio-fundamental)
- [Problema con Expression<Func<>>](#problema-con-expressionfunc)
- [Solución: Repositorios + Query Services](#solución-repositorios--query-services)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Ejemplos de Implementación](#ejemplos-de-implementación)
- [Patrones Complementarios](#patrones-complementarios)

---

## 🎯 Principio Fundamental

```
Repositorio = Persistencia (CRUD simple)
Query Service = Consultas (Intención de negocio)
Dominio = Reglas (Lógica de negocio)
```

**Si una interfaz mezcla estas responsabilidades → está mal diseñada.**

---

## ❌ Problema con `Expression<Func<>>`

### ¿Por qué NO debe estar en los puertos?

```csharp
// ❌ MAL - Expone detalles de infraestructura
public interface IRepository<TEntity, TId>
{
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate);  // ← LINQ/EF Core en el dominio
}
```

### Problemas:

1. **Viola Hexagonal Architecture**: El dominio depende de `System.Linq.Expressions`
2. **Acoplamiento a EF Core**: No puedes cambiar de ORM fácilmente
3. **Testabilidad**: Difícil mockear expresiones complejas
4. **Intención poco clara**: `predicate` no dice QUÉ estás buscando
5. **Fuga de infraestructura**: La capa de dominio conoce detalles técnicos

---

## ✅ Solución: Repositorios + Query Services

### 🥇 Opción 3 (La mejor para proyectos serios)

Esta es la arquitectura que usamos en este proyecto.

### 1️⃣ Repositorio Simple (CRUD)

```csharp
// ✅ BIEN - Solo persistencia básica
public interface IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
{
    // Consultas básicas
    Task<TEntity?> GetByIdAsync(TId id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<bool> ExistsAsync(TId id);
    Task<int> CountAsync();
    
    // Comandos
    Task<TEntity> AddAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task DeleteAsync(TId id);
}
```

**Características:**
- ✔ Simple y predecible
- ✔ Fácil de testear
- ✔ No expone infraestructura
- ✔ Reutilizable para todas las entidades

---

### 2️⃣ Query Service (Consultas de negocio)

```csharp
// ✅ BIEN - Consultas con intención clara
public interface IWeatherForecastQueryService
{
    // Cada método cuenta una historia de negocio
    Task<IEnumerable<WeatherForecast>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate);
    
    Task<IEnumerable<WeatherForecast>> GetByMinimumTemperatureAsync(
        int minimumTemperatureC);
    
    Task<IEnumerable<WeatherForecast>> SearchAsync(
        WeatherForecastSearchCriteria criteria);
    
    Task<WeatherTemperatureStats> GetTemperatureStatsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null);
}
```

**Características:**
- ✔ Intención de negocio clara
- ✔ Fácil de entender qué hace cada método
- ✔ Usa Value Objects para criterios
- ✔ Consultas complejas fuera del repositorio

---

### 3️⃣ Value Objects para Criterios

```csharp
// ✅ BIEN - Encapsula criterios de búsqueda
public record WeatherForecastSearchCriteria
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? MinTemperatureC { get; init; }
    public int? MaxTemperatureC { get; init; }
    public string? SummaryContains { get; init; }
}
```

**Ventajas:**
- ✔ Tipo seguro
- ✔ Inmutable (record)
- ✔ Fácil de testear
- ✔ Reutilizable

---

## 📁 Estructura del Proyecto

```
AspNetProject/
├── Domain/
│   ├── Models/
│   │   ├── IEntity.cs
│   │   └── WeatherForecast.cs
│   ├── Ports/
│   │   ├── In/
│   │   │   └── IWeatherForecastService.cs
│   │   └── Out/
│   │       ├── IRepository.cs                        ← Repositorio genérico (CRUD)
│   │       ├── IWeatherForecastQueryService.cs       ← Query Service específico
│   │       └── IWeatherForecastProvider.cs
│   └── Services/
│       └── WeatherForecastService.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   ├── EfRepository.cs                           ← Implementación del repositorio
│   │   └── WeatherForecastQueryService.cs            ← Implementación del query service
│   └── Providers/
│       └── WeatherForecastProvider.cs
└── Adapters/
    └── In/
        └── Rest/
            └── Controllers/
                └── WeatherForecastController.cs
```

---

## 💻 Ejemplos de Implementación

### Implementación del Repositorio (Infrastructure)

```csharp
// Infrastructure/Persistence/EfRepository.cs
public class EfRepository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public EfRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default)
        => await _dbSet.FindAsync(new object[] { id }, ct);

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.ToListAsync(ct);

    public async Task<bool> ExistsAsync(TId id, CancellationToken ct = default)
        => await _dbSet.FindAsync(new object[] { id }, ct) != null;

    public async Task<int> CountAsync(CancellationToken ct = default)
        => await _dbSet.CountAsync(ct);

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteAsync(TId id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
```

---

### Implementación del Query Service (Infrastructure)

```csharp
// Infrastructure/Persistence/WeatherForecastQueryService.cs
public class WeatherForecastQueryService : IWeatherForecastQueryService
{
    private readonly ApplicationDbContext _context;

    public WeatherForecastQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WeatherForecast>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken ct = default)
    {
        // AQUÍ sí usamos Expression<Func<>> porque estamos en Infrastructure
        return await _context.WeatherForecasts
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<WeatherForecast>> GetByMinimumTemperatureAsync(
        int minimumTemperatureC, 
        CancellationToken ct = default)
    {
        return await _context.WeatherForecasts
            .Where(w => w.TemperatureC >= minimumTemperatureC)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<WeatherForecast>> SearchAsync(
        WeatherForecastSearchCriteria criteria, 
        CancellationToken ct = default)
    {
        var query = _context.WeatherForecasts.AsQueryable();

        if (criteria.StartDate.HasValue)
            query = query.Where(w => w.Date >= criteria.StartDate.Value);

        if (criteria.EndDate.HasValue)
            query = query.Where(w => w.Date <= criteria.EndDate.Value);

        if (criteria.MinTemperatureC.HasValue)
            query = query.Where(w => w.TemperatureC >= criteria.MinTemperatureC.Value);

        if (criteria.MaxTemperatureC.HasValue)
            query = query.Where(w => w.TemperatureC <= criteria.MaxTemperatureC.Value);

        if (!string.IsNullOrWhiteSpace(criteria.SummaryContains))
            query = query.Where(w => w.Summary != null && 
                                    w.Summary.Contains(criteria.SummaryContains));

        return await query.ToListAsync(ct);
    }

    public async Task<WeatherTemperatureStats> GetTemperatureStatsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var query = _context.WeatherForecasts.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(w => w.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(w => w.Date <= endDate.Value);

        var stats = await query
            .GroupBy(w => 1)
            .Select(g => new
            {
                Average = (int)g.Average(w => w.TemperatureC),
                Min = g.Min(w => w.TemperatureC),
                Max = g.Max(w => w.TemperatureC),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(ct);

        return new WeatherTemperatureStats
        {
            AverageTemperatureC = stats?.Average ?? 0,
            MinTemperatureC = stats?.Min ?? 0,
            MaxTemperatureC = stats?.Max ?? 0,
            TotalForecasts = stats?.Count ?? 0
        };
    }
}
```

---

### Uso en el Servicio de Aplicación

```csharp
// Domain/Services/WeatherForecastService.cs
public class WeatherForecastService : IWeatherForecastService
{
    private readonly IRepository<WeatherForecast, int> _repository;
    private readonly IWeatherForecastQueryService _queryService;

    public WeatherForecastService(
        IRepository<WeatherForecast, int> repository,
        IWeatherForecastQueryService queryService)
    {
        _repository = repository;
        _queryService = queryService;
    }

    // CRUD simple usa el repositorio
    public async Task<WeatherForecast?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<WeatherForecast> CreateAsync(WeatherForecast forecast)
        => await _repository.AddAsync(forecast);

    // Consultas complejas usan el query service
    public async Task<IEnumerable<WeatherForecast>> GetHotDaysAsync(int minTemp)
        => await _queryService.GetByMinimumTemperatureAsync(minTemp);

    public async Task<IEnumerable<WeatherForecast>> SearchAsync(
        WeatherForecastSearchCriteria criteria)
        => await _queryService.SearchAsync(criteria);

    public async Task<WeatherTemperatureStats> GetStatsAsync()
        => await _queryService.GetTemperatureStatsAsync();
}
```

---

## 🎨 Patrones Complementarios

### 🥈 Specification Pattern (Cuando hace falta)

Úsalo solo cuando:
- El dominio es complejo
- Necesitas reglas reutilizables
- Tienes muchas combinaciones de filtros

```csharp
// Domain/Specifications/ISpecification.cs
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
}

// Domain/Specifications/HotDaySpecification.cs
public class HotDaySpecification : ISpecification<WeatherForecast>
{
    private readonly int _minimumTemperature;

    public HotDaySpecification(int minimumTemperature)
    {
        _minimumTemperature = minimumTemperature;
    }

    public bool IsSatisfiedBy(WeatherForecast forecast)
        => forecast.TemperatureC >= _minimumTemperature;
}

// Infrastructure traduce a Expression<Func<>>
public static class SpecificationExtensions
{
    public static Expression<Func<WeatherForecast, bool>> ToExpression(
        this HotDaySpecification spec)
    {
        var minTemp = spec.MinimumTemperature;
        return forecast => forecast.TemperatureC >= minTemp;
    }
}
```

---

## 🧠 Reglas de Oro

### ✅ Haz esto:

1. **Repositorios específicos por agregado**
   - Un repositorio por entidad raíz
   - Solo operaciones CRUD básicas

2. **Query Services para lectura**
   - Métodos con intención de negocio clara
   - Usa Value Objects para criterios

3. **Specification Pattern solo si el dominio lo pide**
   - No lo uses por defecto
   - Solo para reglas complejas reutilizables

4. **DTOs para queries (CQRS light)**
   - Separa modelos de lectura y escritura
   - Optimiza queries para casos de uso específicos

5. **Infraestructura bien encerrada**
   - `Expression<Func<>>` solo en Infrastructure
   - EF Core no sale de la capa de persistencia

---

### ❌ NO hagas esto:

1. ❌ Repositorios genéricos gigantes
2. ❌ `Expression<Func<>>` en puertos
3. ❌ `IQueryable` saliendo de infraestructura
4. ❌ Repositorios que hacen lógica de negocio
5. ❌ Mezclar persistencia, consultas y reglas en una interfaz

---

## 📊 Comparación de Opciones

| Aspecto | Opción 1: Métodos explícitos | Opción 2: Specification | **Opción 3: Repo + Query** |
|---------|------------------------------|-------------------------|----------------------------|
| **Simplicidad** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ |
| **Escalabilidad** | ⭐ | ⭐⭐⭐ | ⭐⭐⭐ |
| **Claridad** | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ |
| **Testabilidad** | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ |
| **Mantenibilidad** | ⭐ | ⭐⭐ | ⭐⭐⭐ |
| **Uso en producción** | Proyectos pequeños | Dominios complejos | **Proyectos serios** ✅ |

---

## 🎯 Conclusión

> **Opción 3 (Repositorios + Query Services) es la mejor base para un proyecto serio y mantenible.**

### Stack madura para producción:

```
✅ IRepository<TEntity, TId>           → CRUD simple
✅ IQueryService                       → Consultas de negocio
✅ ISpecification<T>                   → Solo cuando el dominio lo pide
✅ Value Objects                       → Criterios de búsqueda
✅ Infrastructure encapsulada          → EF Core bien aislado
```

---

## 📚 Referencias

- [Hexagonal Architecture](./ARCHITECTURE.md)
- [Domain-Driven Design](./DDD_RESUMEN.md)
- [Project Structure](./PROJECT_STRUCTURE.md)
- [Specification Pattern](https://martinfowler.com/apsupp/spec.pdf)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
