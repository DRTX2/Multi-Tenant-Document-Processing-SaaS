# Capa de Adaptadores (Adapters Layer)

Esta capa contiene todos los **adaptadores** que conectan el núcleo de la aplicación (Domain + Application) con el mundo exterior.

## Estructura

```
Adapters/
├── In/              (Adaptadores de Entrada - Inbound)
│   ├── Rest/       - Controladores REST/HTTP
│   ├── GraphQL/    - Resolvers GraphQL (futuro)
│   └── Grpc/       - Servicios gRPC (futuro)
└── Out/            (Adaptadores de Salida - Outbound)
    └── (opcional)  - Adaptadores específicos si es necesario
```

## Tipos de Adaptadores

### 🔵 In Adapters (Adaptadores de Entrada - Inbound)

**Reciben** peticiones externas y las **envían** a la aplicación.

- **Ubicación**: `Adapters/In/`
- **Responsabilidad**: Recibir peticiones externas y delegarlas a la capa de Application
- **Puertos que usan**: `Domain/Ports/In/` (puertos de entrada como `IWeatherForecastService`)
- **Ejemplos**:
  - REST Controllers (HTTP/JSON)
  - GraphQL Resolvers
  - gRPC Services
  - CLI Commands
  - Message Queue Consumers
  - WebSocket Handlers

**Características**:
- ✅ Dependen de puertos de entrada (`Domain/Ports/In/IWeatherForecastService`)
- ✅ Convierten datos externos (HTTP, JSON) a objetos de dominio
- ✅ Manejan aspectos técnicos (validación HTTP, serialización)
- ❌ NO contienen lógica de negocio

### 🟢 Out Adapters (Adaptadores de Salida - Outbound)

Son **llamados** por la aplicación para acceder a recursos externos.

- **Ubicación**: `Infrastructure/` (principalmente) o `Adapters/Out/`
- **Responsabilidad**: Implementar las interfaces (puertos) definidas en `Domain/Ports/Out/`
- **Puertos que implementan**: `Domain/Ports/Out/` (puertos de salida como `IRepository<T>`, `IWeatherForecastProvider`)
- **Ejemplos**:
  - Repositories (acceso a base de datos)
  - HTTP Clients (llamadas a APIs externas)
  - Email Senders
  - File Storage
  - Cache Providers
  - Message Queue Publishers

**Características**:
- ✅ Implementan puertos de salida (`Domain/Ports/Out/IWeatherForecastProvider`, `IRepository<T>`)
- ✅ Manejan detalles técnicos de infraestructura
- ✅ Son intercambiables sin afectar la lógica de negocio
- ❌ NO son invocados directamente por adaptadores de entrada

## Flujo de Datos

```
┌─────────────────┐
│  In Adapter     │
│  (Inbound)      │  (ej: REST Controller)
└────────┬────────┘
         │ invoca
         ▼
┌─────────────────┐
│  Application    │
│  Service        │  (Caso de Uso)
│  (Core)         │
└────────┬────────┘
         │ usa
         ▼
┌─────────────────┐
│  Out Adapter    │
│  (Outbound)     │  (ej: Repository)
└─────────────────┘
```

## Ejemplo Práctico

### In Adapter (REST)

```csharp
// Adapters/In/Rest/Controllers/WeatherForecastController.cs
[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastService _service; // Puerto de ENTRADA (In)
    
    [HttpGet]
    public ActionResult<IEnumerable<WeatherForecast>> Get([FromQuery] int days = 5)
    {
        var forecasts = _service.GetForecasts(days);
        return Ok(forecasts);
    }
}
```

### Out Adapter (Infrastructure)

```csharp
// Infrastructure/Providers/RandomWeatherForecastProvider.cs
public class RandomWeatherForecastProvider : IWeatherForecastProvider // Puerto de SALIDA (Out)
{
    public IEnumerable<WeatherForecast> GetForecasts(int days)
    {
        // Implementación específica de infraestructura
        return GenerateRandomForecasts(days);
    }
}
```

## Ventajas de esta Separación

| Ventaja | Descripción |
|---------|-------------|
| **Flexibilidad** | Puedes agregar múltiples adaptadores de entrada (REST, GraphQL, gRPC) sin modificar el core |
| **Testabilidad** | Fácil crear mocks de adaptadores de salida para testing |
| **Mantenibilidad** | Cambios en tecnologías externas no afectan la lógica de negocio |
| **Claridad** | Separación clara entre "entrada" y "salida" |

## Reglas de Dependencia

```
In Adapters  ──────────────────────────────┐
                                            │
                                            ▼
                               ┌────────────────────┐
                               │   Domain Ports     │
                               │   In/ y Out/       │
                               └────────────────────┘
                                            ▲
                                            │
Out Adapters (Infrastructure) ──────────────┘
```

**Regla de Oro**: 
- ✅ Los adaptadores dependen del Domain
- ❌ El Domain NUNCA depende de los adaptadores

## Comparación de Nomenclaturas

| Concepto | Nombres Alternativos |
|----------|---------------------|
| **In** | Inbound, Primary, Driving, Input, Left-side |
| **Out** | Outbound, Secondary, Driven, Output, Right-side |

**En este proyecto usamos In/Out porque es más claro y directo.**
