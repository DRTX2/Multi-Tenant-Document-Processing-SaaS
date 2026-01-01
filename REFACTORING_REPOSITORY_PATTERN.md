# 🔧 Refactorización: Eliminación de Expression<Func<>> de los Puertos

## 📅 Fecha
2026-01-01

## 🎯 Objetivo
Eliminar `Expression<Func<TEntity, bool>>` de los puertos del dominio para cumplir con los principios de Hexagonal Architecture y DDD, siguiendo la **Opción 3: Repositorios simples + Query Services**.

---

## ❌ Problema Identificado

### Antes (MAL):
```csharp
// Domain/Ports/Out/IRepository.cs
public interface IRepository<TEntity, TId>
{
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate);  // ❌ LINQ en el dominio
    
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate);  // ❌ EF Core en el dominio
}
```

### ¿Por qué estaba mal?

1. **Viola Hexagonal Architecture**: El dominio depende de `System.Linq.Expressions`
2. **Acoplamiento a EF Core**: No puedes cambiar de ORM fácilmente
3. **Testabilidad**: Difícil mockear expresiones complejas
4. **Intención poco clara**: `predicate` no dice QUÉ estás buscando
5. **Fuga de infraestructura**: La capa de dominio conoce detalles técnicos

---

## ✅ Solución Implementada

### 1️⃣ Repositorio Simplificado

**Archivo**: `Domain/Ports/Out/IRepository.cs`

```csharp
public interface IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
{
    // ============================================
    // CONSULTAS BÁSICAS (Read)
    // ============================================
    Task<TEntity?> GetByIdAsync(TId id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<bool> ExistsAsync(TId id);
    Task<int> CountAsync();
    
    // ============================================
    // COMANDOS (Create, Update, Delete)
    // ============================================
    Task<TEntity> AddAsync(TEntity entity);
    Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task DeleteAsync(TId id);
    Task DeleteAsync(TEntity entity);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities);
}
```

**Características:**
- ✅ Solo operaciones CRUD básicas
- ✅ No depende de `System.Linq.Expressions`
- ✅ Fácil de testear y mockear
- ✅ Reutilizable para todas las entidades

---

### 2️⃣ Query Service (Ejemplo)

**Archivo**: `Domain/Ports/Out/IWeatherForecastQueryService.cs`

```csharp
public interface IWeatherForecastQueryService
{
    // Cada método tiene una intención de negocio clara
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
- ✅ Intención de negocio clara
- ✅ Usa Value Objects para criterios
- ✅ Métodos específicos del dominio
- ✅ No expone detalles de infraestructura

---

### 3️⃣ Implementación en Infrastructure

**Archivo**: `Infrastructure/Repositories/EfRepository.cs`

```csharp
public class EfRepository<TEntity, TId> : IRepository<TEntity, TId>
{
    // Solo implementa operaciones CRUD básicas
    // NO tiene FindAsync ni FirstOrDefaultAsync
}
```

**Archivo**: `Infrastructure/Persistence/CityQueryService.cs` (Ejemplo)

```csharp
public class CityQueryService
{
    private readonly ApplicationDbContext _context;

    // AQUÍ sí usamos Expression<Func<>> porque estamos en Infrastructure
    public async Task<IEnumerable<City>> GetByCountryAsync(string country)
    {
        return await _context.Cities
            .Where(c => c.Country == country)  // ✅ LINQ en Infrastructure
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
```

---

## 📁 Archivos Modificados

### Modificados:
1. ✅ `Domain/Ports/Out/IRepository.cs`
   - Eliminado `FindAsync(Expression<Func<>>)`
   - Eliminado `FirstOrDefaultAsync(Expression<Func<>>)`
   - Eliminado `using System.Linq.Expressions;`

2. ✅ `Infrastructure/Repositories/EfRepository.cs`
   - Eliminado `FindAsync` implementation
   - Eliminado `FirstOrDefaultAsync` implementation
   - Eliminado `using System.Linq.Expressions;`

### Creados:
3. ✅ `Domain/Ports/Out/IWeatherForecastQueryService.cs`
   - Ejemplo de Query Service (puerto)
   - Value Objects: `WeatherForecastSearchCriteria`, `WeatherTemperatureStats`

4. ✅ `Infrastructure/Persistence/CityQueryService.cs`
   - Ejemplo de implementación de Query Service
   - Value Objects: `CitySearchCriteria`, `CityStatistics`

5. ✅ `REPOSITORY_QUERY_ARCHITECTURE.md`
   - Documentación completa de la arquitectura
   - Ejemplos y mejores prácticas
   - Comparación de opciones

---

## 🎨 Patrón Implementado

### Opción 3: Repositorios + Query Services

```
┌─────────────────────────────────────────────────┐
│              DOMAIN (Puertos)                   │
├─────────────────────────────────────────────────┤
│  IRepository<T, TId>                            │
│  - GetByIdAsync()                               │
│  - GetAllAsync()                                │
│  - AddAsync()                                   │
│  - UpdateAsync()                                │
│  - DeleteAsync()                                │
│                                                 │
│  IWeatherForecastQueryService                   │
│  - GetByDateRangeAsync()                        │
│  - GetByMinimumTemperatureAsync()               │
│  - SearchAsync(criteria)                        │
│  - GetTemperatureStatsAsync()                   │
└─────────────────────────────────────────────────┘
                    ▲
                    │ implements
                    │
┌─────────────────────────────────────────────────┐
│         INFRASTRUCTURE (Adaptadores)            │
├─────────────────────────────────────────────────┤
│  EfRepository<T, TId>                           │
│  - Implementa CRUD con EF Core                  │
│  - Usa Expression<Func<>> internamente          │
│                                                 │
│  WeatherForecastQueryService                    │
│  - Implementa consultas con LINQ                │
│  - Usa Expression<Func<>> internamente          │
│  - Traduce criterios a queries de EF Core       │
└─────────────────────────────────────────────────┘
```

---

## 🧠 Principios Aplicados

### ✅ Hexagonal Architecture
- El dominio NO depende de infraestructura
- Los puertos definen contratos de negocio
- La infraestructura implementa los detalles técnicos

### ✅ Domain-Driven Design
- Repositorio = Persistencia de agregados
- Query Service = Consultas del dominio
- Value Objects = Criterios y resultados

### ✅ Separation of Concerns
- **Repositorio**: Solo CRUD básico
- **Query Service**: Consultas complejas
- **Dominio**: Reglas de negocio

### ✅ Dependency Inversion
- El dominio define interfaces (puertos)
- La infraestructura implementa las interfaces
- El flujo de dependencias apunta hacia el dominio

---

## 📊 Comparación

| Aspecto | Antes (❌) | Después (✅) |
|---------|-----------|-------------|
| **Dependencias** | Dominio → System.Linq.Expressions | Dominio → Solo .NET base |
| **Acoplamiento** | Alto (EF Core en dominio) | Bajo (EF Core en Infrastructure) |
| **Testabilidad** | Difícil (mockear Expression<>) | Fácil (métodos simples) |
| **Intención** | Técnica (`predicate`) | Negocio (`GetByCountry`) |
| **Flexibilidad** | Baja (atado a LINQ) | Alta (cambiar ORM fácil) |

---

## 🎯 Reglas de Oro

### ✅ Haz esto:

1. **Repositorios simples**
   - Solo CRUD básico
   - Un repositorio por agregado

2. **Query Services para consultas**
   - Métodos con intención de negocio
   - Usa Value Objects para criterios

3. **Expression<Func<>> solo en Infrastructure**
   - LINQ y EF Core quedan encapsulados
   - El dominio no los conoce

### ❌ NO hagas esto:

1. ❌ `Expression<Func<>>` en puertos
2. ❌ `IQueryable` saliendo de Infrastructure
3. ❌ Repositorios genéricos gigantes
4. ❌ Mezclar persistencia y consultas

---

## 📚 Referencias

- [REPOSITORY_QUERY_ARCHITECTURE.md](./REPOSITORY_QUERY_ARCHITECTURE.md) - Documentación completa
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Arquitectura hexagonal
- [DDD_RESUMEN.md](./DDD_RESUMEN.md) - Domain-Driven Design

---

## ✅ Resultado

El proyecto ahora cumple con:
- ✅ Hexagonal Architecture correcta
- ✅ DDD con repositorios simples
- ✅ Query Services para consultas complejas
- ✅ Separación clara de responsabilidades
- ✅ Fácil de testear y mantener
- ✅ Compilación exitosa

**Build Status**: ✅ SUCCESS (0 warnings, 0 errors)
