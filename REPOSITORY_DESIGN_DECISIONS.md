# 🧠 Decisiones de Diseño del Repositorio

## 📋 Índice
- [Decisiones Tomadas](#decisiones-tomadas)
- [Alternativas Consideradas](#alternativas-consideradas)
- [Justificaciones](#justificaciones)
- [Trade-offs](#trade-offs)

---

## ✅ Decisiones Tomadas

### 1️⃣ Eliminar `GetAllAsync()` del Repositorio Genérico

**Decisión**: ❌ NO incluir `GetAllAsync()` en `IRepository<TEntity, TId>`.

**Razón**:
- Previene cargar miles/millones de registros accidentalmente
- Fuerza a los desarrolladores a pensar en paginación
- Evita OutOfMemoryException en producción
- Mantiene el repositorio enfocado en persistencia, no consultas

**Alternativa implementada**: 
Query Services específicos con `GetAllAsync()` seguro (límite de 1000 registros).

```csharp
// ❌ NO está en IRepository
public interface IRepository<TEntity, TId>
{
    // NO tiene GetAllAsync()
    Task<TEntity?> GetByIdAsync(TId id);
    Task<bool> ExistsAsync(TId id);
    Task<int> CountAsync();
}

// ✅ SÍ está en Query Services (con límite)
public interface ICityQueryService
{
    Task<IEnumerable<City>> GetAllAsync(); // Máximo 1000 o excepción
    Task<PagedResult<City>> SearchAsync(...); // Recomendado
}
```

**Beneficios**:
- ✅ Seguridad por defecto
- ✅ Fail Fast con mensajes claros
- ✅ Permite catálogos pequeños (< 1000 registros)
- ✅ Fuerza paginación para conjuntos grandes

**Ver**: [GETALLASYNC_SOLUTION.md](./GETALLASYNC_SOLUTION.md) para detalles completos.

---

### 2️⃣ Incluir `AddRangeAsync()` y `DeleteRangeAsync()`

**Decisión**: Mantener operaciones batch en el repositorio.

**Razón**:
- Casos de uso legítimos (importaciones, migraciones, limpieza)
- Mejora de rendimiento vs. múltiples llamadas individuales
- No rompe hexagonal architecture

**Garantías documentadas**:
```csharp
/// ⚠️ NOTA: La implementación debe garantizar atomicidad.
/// Si falla una entidad, todas deben revertirse.
```

**Alternativa considerada**: Solo en capa de infraestructura
- ✔ Válida, pero menos conveniente
- ❌ Perdemos abstracción en casos de uso batch

---

### 3️⃣ `UpdateAsync()` devuelve la entidad

**Decisión**: El método `UpdateAsync` retorna `Task<TEntity>`.

**Razón**:
- EF Core puede refrescar el estado de la entidad
- Útil para obtener valores generados por la BD (timestamps, triggers)
- Consistente con `AddAsync`

**Código**:
```csharp
Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
```

**Alternativa considerada**: `Task UpdateAsync(...)`
- ✔ Más simple
- ❌ Menos flexible para casos donde necesitas el estado actualizado

---

### 4️⃣ Repositorio Genérico vs. Específicos

**Decisión**: Usar `IRepository<TEntity, TId>` genérico como base.

**Razón**:
- Evita duplicación de código CRUD básico
- Fácil de extender con repositorios específicos si es necesario
- Mantiene el contrato simple y predecible

**Patrón de extensión**:
```csharp
// Si necesitas métodos específicos, extiende:
public interface ICityRepository : IRepository<City, int>
{
    // Métodos específicos de City (si los necesitas)
    // Pero preferiblemente usa Query Services
}
```

**Alternativa rechazada**: Solo repositorios específicos
- ❌ Mucha duplicación de código
- ❌ Difícil mantener consistencia

---

### 5️⃣ No incluir paginación en el repositorio

**Decisión**: La paginación vive en Query Services, NO en el repositorio.

**Razón**:
- El repositorio es para persistencia, no para consultas complejas
- La paginación es una preocupación de lectura/query
- Mantiene el repositorio simple y enfocado

**Dónde va la paginación**:
```csharp
// ✅ En Query Service
public interface ICityQueryService
{
    Task<PagedResult<City>> SearchAsync(
        CitySearchCriteria criteria,
        int pageNumber,
        int pageSize);
}

// ❌ NO en repositorio
public interface IRepository<TEntity, TId>
{
    // NO incluir esto:
    // Task<PagedResult<TEntity>> GetPagedAsync(int page, int size);
}
```

---

## 🔄 Alternativas Consideradas

### Opción A: Repositorio Ultra-Minimalista

```csharp
public interface IRepository<TEntity, TId>
{
    Task<TEntity?> GetByIdAsync(TId id);
    Task<TEntity> AddAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task DeleteAsync(TId id);
}
```

**Pros**:
- ✔ Extremadamente simple
- ✔ Mínima superficie de API

**Contras**:
- ❌ Demasiado limitado para casos reales
- ❌ Fuerza crear Query Services para TODO

**Veredicto**: ❌ Rechazada (demasiado restrictiva)

---

### Opción B: Repositorio con Specification Pattern

```csharp
public interface IRepository<TEntity, TId>
{
    Task<IEnumerable<TEntity>> FindAsync(ISpecification<TEntity> spec);
}
```

**Pros**:
- ✔ Flexible
- ✔ Reutilizable
- ✔ Testeable

**Contras**:
- ❌ Más complejidad
- ❌ Overkill para CRUD simple
- ❌ Curva de aprendizaje

**Veredicto**: ⚠️ Usar solo cuando el dominio lo requiera (complemento, no base)

---

### Opción C: Repositorio con Expression<Func<>>

```csharp
public interface IRepository<TEntity, TId>
{
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
}
```

**Pros**:
- ✔ Muy flexible

**Contras**:
- ❌ Rompe hexagonal architecture
- ❌ Acopla a LINQ/EF Core
- ❌ Difícil testear
- ❌ Fuga de infraestructura

**Veredicto**: ❌ **RECHAZADA** (viola principios arquitectónicos)

---

## ⚖️ Trade-offs

### Trade-off 1: Simplicidad vs. Flexibilidad

**Decisión**: Priorizar simplicidad con escape hatch (Query Services)

| Aspecto | Repositorio Simple | Repositorio Complejo |
|---------|-------------------|---------------------|
| **Curva de aprendizaje** | ✅ Baja | ❌ Alta |
| **Mantenibilidad** | ✅ Alta | ❌ Media |
| **Flexibilidad** | ⚠️ Media | ✅ Alta |
| **Casos de uso** | ✅ 80% | ✅ 100% |

**Conclusión**: El 80% de casos se resuelve con repositorio simple. El 20% restante usa Query Services.

---

### Trade-off 2: Genérico vs. Específico

**Decisión**: Genérico como base, específico cuando sea necesario

```csharp
// Base genérica (reutilizable)
IRepository<City, int>

// Extensión específica (solo si es necesario)
ICityRepository : IRepository<City, int>
{
    // Métodos específicos de dominio
}
```

**Ventaja**: Mejor de ambos mundos

---

### Trade-off 3: Retornar entidad vs. void en Update

**Decisión**: Retornar `Task<TEntity>`

**Escenarios**:

```csharp
// Escenario 1: Necesitas el estado actualizado
var updated = await _repository.UpdateAsync(city);
Console.WriteLine(updated.UpdatedAt); // ✅ Timestamp generado por BD

// Escenario 2: No necesitas el retorno
await _repository.UpdateAsync(city); // ✅ Ignoras el retorno
```

**Conclusión**: Más flexible sin costo significativo

---

## 📊 Matriz de Decisiones

| Característica | Incluida en IRepository | Razón |
|----------------|------------------------|-------|
| `GetByIdAsync()` | ✅ | CRUD básico esencial |
| `GetAllAsync()` | ❌ | **Eliminado** - Riesgo de rendimiento. Usar Query Services con límite |
| `ExistsAsync()` | ✅ | Patrón común, evita cargar entidad completa |
| `CountAsync()` | ✅ | Estadística básica sin cargar datos |
| `AddAsync()` | ✅ | CRUD básico esencial |
| `AddRangeAsync()` | ✅ | Casos batch legítimos (con advertencias de atomicidad) |
| `UpdateAsync()` | ✅ | CRUD básico esencial |
| `DeleteAsync(id)` | ✅ | CRUD básico esencial |
| `DeleteAsync(entity)` | ✅ | Sobrecarga útil cuando ya tienes la entidad |
| `DeleteRangeAsync()` | ✅ | Casos batch legítimos (con advertencias de atomicidad) |
| `FindAsync(Expression<>)` | ❌ | Rompe hexagonal, va a Query Services |
| `GetPagedAsync()` | ❌ | Consulta compleja, va a Query Services |
| `SearchAsync()` | ❌ | Consulta compleja, va a Query Services |

**Nota**: `GetAllAsync()` SÍ está disponible en Query Services específicos con límite de seguridad (1000 registros).

---

## 🎯 Principios Guía

### 1. **Repositorio = Persistencia**
- Solo operaciones CRUD básicas
- Sin lógica de negocio
- Sin consultas complejas

### 2. **Query Service = Consultas**
- Búsquedas complejas
- Filtros múltiples
- Paginación
- Agregaciones
- Estadísticas

### 3. **Dominio = Reglas**
- Invariantes
- Validaciones
- Lógica de negocio

---

## 📚 Referencias

### Patrones Aplicados:
- ✅ Repository Pattern (simplificado)
- ✅ Query Object Pattern (Query Services)
- ✅ Dependency Inversion Principle
- ✅ Interface Segregation Principle

### Arquitecturas Respetadas:
- ✅ Hexagonal Architecture (Ports & Adapters)
- ✅ Clean Architecture
- ✅ Domain-Driven Design

### Fuentes:
- [Martin Fowler - Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [Eric Evans - Domain-Driven Design](https://www.domainlanguage.com/ddd/)
- [Alistair Cockburn - Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)

---

## 🔄 Evolución Futura

### Cuándo extender este diseño:

1. **Si el dominio se vuelve muy complejo**:
   - Considera Specification Pattern
   - Mantén Query Services como base

2. **Si necesitas optimizaciones de rendimiento**:
   - Implementa caché en Query Services
   - Considera CQRS completo (Command/Query separation)

3. **Si necesitas auditoría**:
   - Implementa en `SaveChangesAsync` del DbContext
   - No contamines el repositorio

4. **Si necesitas multi-tenancy**:
   - Implementa filtros globales en EF Core
   - No expongas en la interfaz del repositorio

---

## ✅ Conclusión

Este diseño representa un **balance maduro** entre:
- ✅ Simplicidad
- ✅ Flexibilidad
- ✅ Principios arquitectónicos
- ✅ Pragmatismo

**Es apropiado para proyectos serios en producción.**

---

**Última actualización**: 2026-01-01
**Revisado por**: Arquitectura del proyecto
**Estado**: ✅ Aprobado
