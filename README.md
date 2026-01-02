# AspNetProject - Arquitectura Hexagonal con PostgreSQL

Este proyecto implementa una API REST en **ASP.NET Core 10.0** siguiendo los principios de **Arquitectura Hexagonal** (también conocida como Arquitectura de Puertos y Adaptadores), con **Entity Framework Core** y **PostgreSQL** como base de datos.

## 🎯 Características

- ✅ **Arquitectura Hexagonal** completa con separación de capas
- ✅ **Domain-Driven Design (DDD)** con guías completas
- ✅ **Paginación Profesional** con PageRequest y PagedResult (estilo Spring Boot)
- ✅ **Entity Framework Core 10** con PostgreSQL
- ✅ **Repositorio Genérico** para evitar duplicación de código
- ✅ **Query Services** para consultas complejas con paginación
- ✅ **Swagger/OpenAPI** para documentación interactiva
- ✅ **Docker Compose** para desarrollo local
- ✅ **Convenciones snake_case** para PostgreSQL
- ✅ **Inyección de Dependencias** configurada correctamente

## 📄 Paginación Profesional

Este proyecto implementa **paginación profesional nivel senior** similar a Spring Data de Spring Boot:

### 📚 Guías de Paginación

| Documento | Descripción |
|-----------|-------------|
| [PAGINATION_SUMMARY.md](Docs/PAGINATION_SUMMARY.md) | 🎯 **Resumen ejecutivo** - Empieza aquí ⭐⭐⭐ |
| [PAGINATION_GUIDE.md](Docs/PAGINATION_GUIDE.md) | 📖 Guía completa: Spring Boot vs .NET |
| [PAGINATION_DIAGRAMS.md](Docs/PAGINATION_DIAGRAMS.md) | 📊 Diagramas visuales y flujos |
| [PAGINATION_FRONTEND_EXAMPLES.md](Docs/PAGINATION_FRONTEND_EXAMPLES.md) | 💻 Ejemplos React, Vue, TypeScript |
| [PAGINATION_TESTING.md](Docs/PAGINATION_TESTING.md) | 🧪 Tests unitarios e integración |

### Características de Paginación

- **PageRequest** - Similar a `Pageable` de Spring Boot (0-indexed)
- **PagedResult<T>** - Similar a `Page<T>` de Spring Data
- **Query Services** - Separación de consultas complejas del repositorio genérico
- **Ordenamiento dinámico** - Por cualquier campo con dirección ASC/DESC
- **Límites de seguridad** - Protección contra cargas masivas de datos
- **Criterios de búsqueda** - Filtros específicos del dominio

**Ejemplo de uso:**
```csharp
// Crear solicitud de paginación
var pageRequest = PageRequest.Of(0, 20, "Name", true);

// Crear criterios de búsqueda
var criteria = new CitySearchCriteria { Country = "Colombia" };

// Ejecutar búsqueda paginada
var result = await cityQueryService.SearchAsync(criteria, pageRequest);

// Resultado incluye metadatos
Console.WriteLine($"Total: {result.TotalCount}");
Console.WriteLine($"Pages: {result.TotalPages}");
Console.WriteLine($"Has next: {result.HasNextPage}");
```

**Lee primero:** [PAGINATION_SUMMARY.md](Docs/PAGINATION_SUMMARY.md) para entender por qué NO hay `GetAll()` en el repositorio.

## 🏛️ Domain-Driven Design (DDD)

Este proyecto incluye **documentación completa** sobre cómo implementar **Domain-Driven Design (DDD)**:

### 📚 Guías de DDD

| Documento | Descripción |
|-----------|-------------|
| [DDD_RESUMEN.md](DDD_RESUMEN.md) | 🎉 **Resumen ejecutivo en español** - Empieza aquí ⭐⭐⭐ |
| [DDD_GUIDE.md](DDD_GUIDE.md) | 🏛️ Guía completa de DDD con todos los building blocks |
| [DDD_PRACTICAL_EXAMPLE.md](DDD_PRACTICAL_EXAMPLE.md) | 🚀 Ejemplo paso a paso para refactorizar WeatherForecast |
| [DDD_COMPARISON.md](DDD_COMPARISON.md) | 🔄 Comparación: Modelo Anémico vs DDD |
| [DDD_QUICK_REFERENCE.md](DDD_QUICK_REFERENCE.md) | 📋 Referencia rápida con checklists |

### Building Blocks de DDD

- **Value Objects** - Objetos inmutables (Temperature, Email, Money)
- **Entities** - Objetos con identidad (WeatherForecast, Customer, Order)
- **Aggregates** - Grupos de entidades con consistencia garantizada
- **Domain Events** - Eventos del dominio (OrderCreated, WeatherAlert)
- **Specifications** - Reglas de negocio reutilizables
- **Domain Services** - Lógica que no pertenece a una entidad

**Lee primero:** [DDD_RESUMEN.md](DDD_RESUMEN.md) para un resumen completo en español.

## 🏗️ Arquitectura Hexagonal

La arquitectura hexagonal separa la lógica de negocio del código de infraestructura, permitiendo que el dominio sea independiente de frameworks, bases de datos y otros detalles técnicos.

### Capas del Proyecto

```
AspNetProject/
├── Domain/                    # ⬡ NÚCLEO DEL DOMINIO
│   ├── Models/               # Entidades de dominio
│   │   └── WeatherForecast.cs
│   └── Ports/                # Interfaces (Contratos)
│       ├── IWeatherForecastService.cs    # Puerto de entrada (Use Case)
│       └── IWeatherForecastProvider.cs   # Puerto de salida (Repository)
│
├── Application/              # 🔄 CAPA DE APLICACIÓN
│   └── Services/            # Implementación de casos de uso
│       └── WeatherForecastService.cs
│
├── Infrastructure/          # 🔌 ADAPTADORES DE INFRAESTRUCTURA
│   └── Providers/          # Implementaciones de puertos de salida
│       └── RandomWeatherForecastProvider.cs
│
└── Api/                    # 🌐 ADAPTADORES DE ENTRADA
    └── Controllers/        # Controladores REST
        └── WeatherForecastController.cs
```

### Flujo de Dependencias

```
┌─────────────────────────────────────────────────────────────┐
│                    API (Adaptador de Entrada)               │
│                  WeatherForecastController                  │
└────────────────────────┬────────────────────────────────────┘
                         │ depende de ↓
┌────────────────────────▼────────────────────────────────────┐
│                   DOMINIO (Núcleo)                          │
│              IWeatherForecastService (Puerto)               │
└────────────────────────┬────────────────────────────────────┘
                         │ implementado por ↓
┌────────────────────────▼────────────────────────────────────┐
│                  APLICACIÓN (Casos de Uso)                  │
│               WeatherForecastService                        │
└────────────────────────┬────────────────────────────────────┘
                         │ depende de ↓
┌────────────────────────▼────────────────────────────────────┐
│                   DOMINIO (Núcleo)                          │
│            IWeatherForecastProvider (Puerto)                │
└────────────────────────┬────────────────────────────────────┘
                         │ implementado por ↓
┌────────────────────────▼────────────────────────────────────┐
│            INFRAESTRUCTURA (Adaptador de Salida)            │
│            RandomWeatherForecastProvider                    │
└─────────────────────────────────────────────────────────────┘
```

## 📋 Componentes

### 1. Domain (Núcleo del Dominio)

**Responsabilidad**: Contiene la lógica de negocio pura, entidades y contratos (interfaces).

- **Models/WeatherForecast.cs**: Entidad de dominio que representa un pronóstico del tiempo
- **Ports/IWeatherForecastService.cs**: Puerto de entrada - Define el caso de uso
- **Ports/IWeatherForecastProvider.cs**: Puerto de salida - Define cómo obtener datos

**Reglas**:
- ✅ No depende de ninguna otra capa
- ✅ No contiene referencias a frameworks externos
- ✅ Solo contiene lógica de negocio pura

### 2. Application (Capa de Aplicación)

**Responsabilidad**: Implementa los casos de uso y orquesta la lógica de negocio.

- **Services/WeatherForecastService.cs**: Implementa `IWeatherForecastService`
  - Valida que el número de días sea válido (1-30)
  - Orquesta la llamada al proveedor de datos

**Reglas**:
- ✅ Depende solo del Domain
- ✅ Implementa los puertos de entrada
- ✅ Usa los puertos de salida (interfaces del Domain)

### 3. Infrastructure (Adaptadores de Infraestructura)

**Responsabilidad**: Implementa los detalles técnicos (bases de datos, APIs externas, etc.).

- **Providers/RandomWeatherForecastProvider.cs**: Implementa `IWeatherForecastProvider`
  - Genera datos aleatorios para demostración
  - En producción, podría conectarse a una API real o base de datos

**Reglas**:
- ✅ Depende del Domain (implementa sus interfaces)
- ✅ Contiene detalles técnicos de implementación

### 4. Api (Adaptadores de Entrada)

**Responsabilidad**: Expone la funcionalidad a través de endpoints REST.

- **Controllers/WeatherForecastController.cs**: Controlador REST
  - Endpoint: `GET /api/weatherforecast?days=5`
  - Maneja errores y retorna respuestas HTTP apropiadas

**Reglas**:
- ✅ Depende solo de las interfaces del Domain
- ✅ No conoce los detalles de implementación

## 🗄️ Base de Datos

### PostgreSQL con Entity Framework Core

El proyecto usa **PostgreSQL** como base de datos y **Entity Framework Core** con un **patrón de repositorio genérico** que evita duplicación de código.

#### Iniciar PostgreSQL con Docker

```bash
# Iniciar PostgreSQL y pgAdmin
docker-compose up -d

# Verificar que esté corriendo
docker-compose ps
```

Acceso a pgAdmin: `http://localhost:5050`
- Email: `admin@aspnetproject.com`
- Password: `admin`

#### Aplicar Migraciones

```bash
# Crear la primera migración
dotnet ef migrations add InitialCreate --project AspNetProject/AspNetProject.csproj

# Aplicar migraciones a la base de datos
dotnet ef database update --project AspNetProject/AspNetProject.csproj
```

#### Configuración de Conexión

En `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=aspnetproject;Username=postgres;Password=postgres;Port=5432"
  }
}
```

**Para producción**, usa variables de entorno:

```bash
export ConnectionStrings__DefaultConnection="Host=prod-server;Database=aspnetproject;Username=user;Password=secure-pass"
```

#### Repositorio Genérico

El proyecto incluye un repositorio genérico que puedes usar con cualquier entidad:

```csharp
// Inyectar el repositorio genérico
public class MyService
{
    private readonly IRepository<City, int> _repository;
    
    public MyService(IRepository<City, int> repository)
    {
        _repository = repository;
    }
    
    // Usar métodos CRUD sin duplicar código
    public async Task<City?> GetCityAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
```

Ver [DATABASE_GUIDE.md](DATABASE_GUIDE.md) para más detalles sobre cómo usar Entity Framework Core.

### Compilar el proyecto

```bash
dotnet build
```

### Ejecutar el proyecto

```bash
dotnet run --project AspNetProject/AspNetProject.csproj
```

### Acceder a la documentación Swagger

Una vez ejecutado, abre tu navegador en:
```
https://localhost:5001
```

## 🔧 Inyección de Dependencias

En `Program.cs`, las dependencias se registran siguiendo el principio de inversión de dependencias:

```csharp
// Puerto de salida (Adaptador de Infraestructura)
builder.Services.AddScoped<IWeatherForecastProvider, RandomWeatherForecastProvider>();

// Puerto de entrada (Caso de Uso)
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
```

Esto permite:
- ✅ Cambiar implementaciones sin modificar el código que las usa
- ✅ Facilitar testing con mocks
- ✅ Mantener bajo acoplamiento

## 📝 Endpoints

### GET /api/weatherforecast

Obtiene pronósticos del tiempo para los próximos días.

**Parámetros**:
- `days` (query, opcional): Número de días (1-30). Default: 5

**Respuestas**:
- `200 OK`: Retorna lista de pronósticos
- `400 Bad Request`: Si el número de días es inválido

**Ejemplo**:
```bash
curl -X GET "https://localhost:5001/api/weatherforecast?days=7"
```

**Respuesta**:
```json
[
  {
    "date": "2025-12-31",
    "temperatureC": 15,
    "temperatureF": 58,
    "summary": "Mild"
  },
  ...
]
```

## 🧪 Testing

La arquitectura hexagonal facilita el testing:

```csharp
// Mock del proveedor
var mockProvider = new Mock<IWeatherForecastProvider>();
mockProvider.Setup(p => p.GetForecasts(It.IsAny<int>()))
    .Returns(new List<WeatherForecast> { /* ... */ });

// Test del servicio
var service = new WeatherForecastService(mockProvider.Object);
var result = service.GetForecasts(5);
```

## 🎯 Beneficios de esta Arquitectura

1. **Independencia del Framework**: El dominio no depende de ASP.NET Core
2. **Testeable**: Fácil crear tests unitarios con mocks
3. **Mantenible**: Cambios en infraestructura no afectan la lógica de negocio
4. **Escalable**: Fácil agregar nuevos adaptadores (GraphQL, gRPC, etc.)
5. **Flexible**: Cambiar de base de datos o API externa sin tocar el dominio

## 📚 Próximos Pasos

Para extender este proyecto:

1. **Agregar persistencia**: Implementar `IWeatherForecastProvider` con Entity Framework
2. **Agregar autenticación**: Implementar JWT en la capa API
3. **Agregar más casos de uso**: Crear nuevos servicios en Application
4. **Agregar validaciones**: Usar FluentValidation en la capa Application
5. **Agregar logging**: Implementar logging sin contaminar el dominio

## 📖 Referencias

- [Hexagonal Architecture (Alistair Cockburn)](https://alistair.cockburn.us/hexagonal-architecture/)
- [Clean Architecture (Robert C. Martin)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Ports and Adapters Pattern](https://herbertograca.com/2017/09/14/ports-adapters-architecture/)
