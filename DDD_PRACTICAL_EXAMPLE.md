# 🚀 Ejemplo Práctico: Aplicando DDD a WeatherForecast

Este documento muestra paso a paso cómo refactorizar el proyecto actual para aplicar Domain-Driven Design.

---

## 📋 Plan de Refactorización

### Fase 1: Crear Value Objects
- [x] Temperature (Celsius, Fahrenheit, Kelvin)
- [x] WeatherCondition (Enum mejorado)
- [ ] Implementar en el proyecto

### Fase 2: Mejorar Entidades
- [ ] WeatherForecast como entidad rica
- [ ] City como entidad

### Fase 3: Agregar Domain Services
- [ ] IWeatherValidationService
- [ ] ITemperatureConversionService

### Fase 4: Implementar Specifications
- [ ] ValidForecastDateSpecification
- [ ] ExtremeTemperatureSpecification

---

## Paso 1: Crear Value Objects

### 1.1 Temperature Value Object

```bash
mkdir -p AspNetProject/Domain/Models/ValueObjects
```

**Archivo:** `Domain/Models/ValueObjects/Temperature.cs`

```csharp
using AspNetProject.Domain.Exceptions;

namespace AspNetProject.Domain.Models.ValueObjects;

/// <summary>
/// Value Object para Temperature - Inmutable con conversiones automáticas
/// </summary>
public sealed class Temperature : IEquatable<Temperature>
{
    private const double AbsoluteZeroCelsius = -273.15;
    
    public double Celsius { get; }
    public double Fahrenheit => (Celsius * 9 / 5) + 32;
    public double Kelvin => Celsius + 273.15;

    private Temperature(double celsius)
    {
        Celsius = celsius;
    }

    /// <summary>
    /// Crea una temperatura desde Celsius
    /// </summary>
    public static Temperature FromCelsius(double celsius)
    {
        if (celsius < AbsoluteZeroCelsius)
            throw new DomainException(
                $"Temperature cannot be below absolute zero ({AbsoluteZeroCelsius}°C)");

        return new Temperature(celsius);
    }

    /// <summary>
    /// Crea una temperatura desde Fahrenheit
    /// </summary>
    public static Temperature FromFahrenheit(double fahrenheit)
    {
        var celsius = (fahrenheit - 32) * 5 / 9;
        return FromCelsius(celsius);
    }

    /// <summary>
    /// Crea una temperatura desde Kelvin
    /// </summary>
    public static Temperature FromKelvin(double kelvin)
    {
        if (kelvin < 0)
            throw new DomainException("Kelvin temperature cannot be negative");

        return FromCelsius(kelvin - 273.15);
    }

    // Métodos de negocio
    public bool IsExtremelyCold() => Celsius < -20;
    public bool IsCold() => Celsius < 10;
    public bool IsMild() => Celsius >= 10 && Celsius <= 25;
    public bool IsWarm() => Celsius > 25 && Celsius <= 35;
    public bool IsHot() => Celsius > 35;
    public bool IsFreezing() => Celsius <= 0;

    public string GetDescription()
    {
        return this switch
        {
            _ when IsExtremelyCold() => "Extremely Cold",
            _ when IsCold() => "Cold",
            _ when IsMild() => "Mild",
            _ when IsWarm() => "Warm",
            _ when IsHot() => "Hot",
            _ => "Unknown"
        };
    }

    // Equality
    public bool Equals(Temperature? other)
    {
        if (other is null) return false;
        return Math.Abs(Celsius - other.Celsius) < 0.01;
    }

    public override bool Equals(object? obj) => Equals(obj as Temperature);
    public override int GetHashCode() => Celsius.GetHashCode();
    public override string ToString() => $"{Celsius:F1}°C ({Fahrenheit:F1}°F)";

    // Operators
    public static bool operator >(Temperature left, Temperature right) 
        => left.Celsius > right.Celsius;
    
    public static bool operator <(Temperature left, Temperature right) 
        => left.Celsius < right.Celsius;
    
    public static bool operator >=(Temperature left, Temperature right) 
        => left.Celsius >= right.Celsius;
    
    public static bool operator <=(Temperature left, Temperature right) 
        => left.Celsius <= right.Celsius;
}
```

---

### 1.2 WeatherCondition Enum Mejorado

**Archivo:** `Domain/Models/Enums/WeatherCondition.cs`

```bash
mkdir -p AspNetProject/Domain/Models/Enums
```

```csharp
namespace AspNetProject.Domain.Models.Enums;

/// <summary>
/// Condiciones climáticas posibles
/// </summary>
public enum WeatherCondition
{
    Sunny,
    PartlyCloudy,
    Cloudy,
    Overcast,
    LightRain,
    Rain,
    HeavyRain,
    Thunderstorm,
    LightSnow,
    Snow,
    HeavySnow,
    Blizzard,
    Fog,
    Windy,
    Hail
}

/// <summary>
/// Extensiones para WeatherCondition
/// </summary>
public static class WeatherConditionExtensions
{
    public static string GetDescription(this WeatherCondition condition)
    {
        return condition switch
        {
            WeatherCondition.Sunny => "Clear skies and sunny",
            WeatherCondition.PartlyCloudy => "Partly cloudy",
            WeatherCondition.Cloudy => "Cloudy",
            WeatherCondition.Overcast => "Overcast skies",
            WeatherCondition.LightRain => "Light rain",
            WeatherCondition.Rain => "Rainy",
            WeatherCondition.HeavyRain => "Heavy rain",
            WeatherCondition.Thunderstorm => "Thunderstorm",
            WeatherCondition.LightSnow => "Light snow",
            WeatherCondition.Snow => "Snowy",
            WeatherCondition.HeavySnow => "Heavy snow",
            WeatherCondition.Blizzard => "Blizzard conditions",
            WeatherCondition.Fog => "Foggy",
            WeatherCondition.Windy => "Windy",
            WeatherCondition.Hail => "Hail",
            _ => "Unknown"
        };
    }

    public static string GetEmoji(this WeatherCondition condition)
    {
        return condition switch
        {
            WeatherCondition.Sunny => "☀️",
            WeatherCondition.PartlyCloudy => "⛅",
            WeatherCondition.Cloudy => "☁️",
            WeatherCondition.Overcast => "☁️",
            WeatherCondition.LightRain => "🌦️",
            WeatherCondition.Rain => "🌧️",
            WeatherCondition.HeavyRain => "⛈️",
            WeatherCondition.Thunderstorm => "⚡",
            WeatherCondition.LightSnow => "🌨️",
            WeatherCondition.Snow => "❄️",
            WeatherCondition.HeavySnow => "❄️",
            WeatherCondition.Blizzard => "🌨️",
            WeatherCondition.Fog => "🌫️",
            WeatherCondition.Windy => "💨",
            WeatherCondition.Hail => "🌨️",
            _ => "❓"
        };
    }

    public static bool RequiresUmbrella(this WeatherCondition condition)
    {
        return condition is 
            WeatherCondition.LightRain or 
            WeatherCondition.Rain or 
            WeatherCondition.HeavyRain or 
            WeatherCondition.Thunderstorm;
    }

    public static bool IsDangerous(this WeatherCondition condition)
    {
        return condition is 
            WeatherCondition.Thunderstorm or 
            WeatherCondition.Blizzard or 
            WeatherCondition.HeavyRain or 
            WeatherCondition.Hail;
    }
}
```

---

## Paso 2: Crear Domain Exceptions

**Archivo:** `Domain/Exceptions/DomainException.cs`

```bash
mkdir -p AspNetProject/Domain/Exceptions
```

```csharp
namespace AspNetProject.Domain.Exceptions;

/// <summary>
/// Excepción base del dominio
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    
    public DomainException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

**Archivo:** `Domain/Exceptions/InvalidForecastDateException.cs`

```csharp
namespace AspNetProject.Domain.Exceptions;

public class InvalidForecastDateException : DomainException
{
    public InvalidForecastDateException(string message) : base(message) { }
}
```

---

## Paso 3: Refactorizar WeatherForecast como Entidad Rica

**Archivo:** `Domain/Models/Entities/WeatherForecast.cs`

```bash
mkdir -p AspNetProject/Domain/Models/Entities
```

```csharp
using AspNetProject.Domain.Exceptions;
using AspNetProject.Domain.Models.Enums;
using AspNetProject.Domain.Models.ValueObjects;

namespace AspNetProject.Domain.Models.Entities;

/// <summary>
/// Entidad WeatherForecast - Modelo rico con DDD
/// Representa un pronóstico del tiempo para una fecha específica
/// </summary>
public class WeatherForecast : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public DateOnly Date { get; private set; }
    public Temperature Temperature { get; private set; }
    public WeatherCondition Condition { get; private set; }
    public City City { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    // Constructor privado - solo se crea mediante factory method
    private WeatherForecast() { }

    /// <summary>
    /// Factory Method para crear un pronóstico válido
    /// </summary>
    public static WeatherForecast Create(
        DateOnly date, 
        Temperature temperature, 
        WeatherCondition condition,
        City city)
    {
        // Validaciones de negocio
        if (date < DateOnly.FromDateTime(DateTime.Today))
            throw new InvalidForecastDateException(
                "Cannot create forecast for past dates");

        if (date > DateOnly.FromDateTime(DateTime.Today.AddDays(30)))
            throw new InvalidForecastDateException(
                "Cannot create forecast more than 30 days in advance");

        return new WeatherForecast
        {
            Id = Guid.NewGuid(),
            Date = date,
            Temperature = temperature,
            Condition = condition,
            City = city,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Actualiza la temperatura del pronóstico
    /// </summary>
    public void UpdateTemperature(Temperature newTemperature)
    {
        if (Date < DateOnly.FromDateTime(DateTime.Today))
            throw new DomainException("Cannot update past forecasts");

        Temperature = newTemperature;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza la condición climática
    /// </summary>
    public void UpdateCondition(WeatherCondition newCondition)
    {
        if (Date < DateOnly.FromDateTime(DateTime.Today))
            throw new DomainException("Cannot update past forecasts");

        Condition = newCondition;
        LastModifiedAt = DateTime.UtcNow;
    }

    // Métodos de negocio
    public bool IsToday() => Date == DateOnly.FromDateTime(DateTime.Today);
    public bool IsTomorrow() => Date == DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    public bool IsInPast() => Date < DateOnly.FromDateTime(DateTime.Today);
    public bool IsHot() => Temperature.IsHot();
    public bool IsCold() => Temperature.IsCold();
    public bool RequiresUmbrella() => Condition.RequiresUmbrella();
    public bool IsDangerous() => Condition.IsDangerous() || Temperature.IsExtremelyCold();

    /// <summary>
    /// Obtiene una recomendación basada en el pronóstico
    /// </summary>
    public string GetRecommendation()
    {
        if (IsDangerous())
            return "⚠️ Dangerous weather conditions. Stay indoors if possible.";

        if (RequiresUmbrella())
            return "☔ Don't forget your umbrella!";

        if (Temperature.IsHot())
            return "🌞 Stay hydrated and use sunscreen!";

        if (Temperature.IsCold())
            return "🧥 Dress warmly!";

        return "✅ Pleasant weather expected!";
    }

    /// <summary>
    /// Obtiene un resumen del pronóstico
    /// </summary>
    public string GetSummary()
    {
        return $"{Condition.GetEmoji()} {Condition.GetDescription()} - {Temperature.GetDescription()}";
    }

    public override string ToString()
    {
        return $"{Date:yyyy-MM-dd} in {City.Name}: {GetSummary()}";
    }
}
```

---

## Paso 4: Crear Specifications

**Archivo:** `Domain/Specifications/ISpecification.cs`

```bash
mkdir -p AspNetProject/Domain/Specifications
```

```csharp
namespace AspNetProject.Domain.Specifications;

/// <summary>
/// Patrón Specification - Encapsula reglas de negocio
/// </summary>
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
}
```

**Archivo:** `Domain/Specifications/ValidForecastDateSpecification.cs`

```csharp
using AspNetProject.Domain.Models.Entities;

namespace AspNetProject.Domain.Specifications;

/// <summary>
/// Especificación: La fecha del pronóstico es válida
/// </summary>
public class ValidForecastDateSpecification : ISpecification<WeatherForecast>
{
    public bool IsSatisfiedBy(WeatherForecast forecast)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var maxDate = today.AddDays(30);

        return forecast.Date >= today && forecast.Date <= maxDate;
    }
}
```

**Archivo:** `Domain/Specifications/ExtremeTemperatureSpecification.cs`

```csharp
using AspNetProject.Domain.Models.Entities;

namespace AspNetProject.Domain.Specifications;

/// <summary>
/// Especificación: La temperatura es extrema (muy fría o muy caliente)
/// </summary>
public class ExtremeTemperatureSpecification : ISpecification<WeatherForecast>
{
    public bool IsSatisfiedBy(WeatherForecast forecast)
    {
        return forecast.Temperature.IsExtremelyCold() || 
               forecast.Temperature.IsHot();
    }
}
```

---

## Paso 5: Actualizar el Servicio de Aplicación

**Archivo:** `Application/Services/WeatherForecastService.cs`

```csharp
using AspNetProject.Domain.Exceptions;
using AspNetProject.Domain.Models;
using AspNetProject.Domain.Models.Entities;
using AspNetProject.Domain.Models.Enums;
using AspNetProject.Domain.Models.ValueObjects;
using AspNetProject.Domain.Ports.In;
using AspNetProject.Domain.Ports.Out;
using AspNetProject.Domain.Specifications;

namespace AspNetProject.Application.Services;

/// <summary>
/// Servicio de aplicación para pronósticos del tiempo
/// Implementa casos de uso con DDD
/// </summary>
public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastProvider _provider;
    private readonly IRepository<City, int> _cityRepository;
    private readonly ValidForecastDateSpecification _validDateSpec;
    private readonly ExtremeTemperatureSpecification _extremeTempSpec;

    public WeatherForecastService(
        IWeatherForecastProvider provider,
        IRepository<City, int> cityRepository)
    {
        _provider = provider;
        _cityRepository = cityRepository;
        _validDateSpec = new ValidForecastDateSpecification();
        _extremeTempSpec = new ExtremeTemperatureSpecification();
    }

    public IEnumerable<WeatherForecast> GetForecasts(int days)
    {
        // Validación de negocio
        if (days <= 0)
            throw new DomainException("Number of days must be positive");

        if (days > 30)
            throw new DomainException("Cannot forecast more than 30 days in advance");

        // Obtener pronósticos del provider
        var forecasts = _provider.GetForecasts(days);

        // Aplicar especificaciones
        var validForecasts = forecasts
            .Where(f => _validDateSpec.IsSatisfiedBy(f))
            .ToList();

        // Identificar pronósticos extremos
        var extremeForecasts = validForecasts
            .Where(f => _extremeTempSpec.IsSatisfiedBy(f))
            .ToList();

        if (extremeForecasts.Any())
        {
            // Aquí podrías publicar un evento de dominio
            // o enviar una notificación
            Console.WriteLine($"⚠️ {extremeForecasts.Count} extreme temperature forecasts detected!");
        }

        return validForecasts;
    }

    public WeatherForecast GetForecastForDate(DateOnly date, int cityId)
    {
        // Validar que la ciudad existe
        var city = _cityRepository.GetByIdAsync(cityId).Result;
        if (city == null)
            throw new DomainException($"City with ID {cityId} not found");

        // Validar fecha
        if (date < DateOnly.FromDateTime(DateTime.Today))
            throw new InvalidForecastDateException("Cannot get forecast for past dates");

        // Generar pronóstico (esto vendría del provider en un caso real)
        var random = new Random();
        var temperature = Temperature.FromCelsius(random.Next(-20, 40));
        var condition = (WeatherCondition)random.Next(0, 15);

        var forecast = WeatherForecast.Create(date, temperature, condition, city);

        // Validar con especificación
        if (!_validDateSpec.IsSatisfiedBy(forecast))
            throw new DomainException("Generated forecast is not valid");

        return forecast;
    }
}
```

---

## Paso 6: Actualizar el Controlador

**Archivo:** `Adapters/In/Rest/Controllers/WeatherForecastController.cs`

```csharp
using AspNetProject.Domain.Exceptions;
using AspNetProject.Domain.Ports.In;
using Microsoft.AspNetCore.Mvc;

namespace AspNetProject.Adapters.In.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastService _service;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(
        IWeatherForecastService service,
        ILogger<WeatherForecastController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene pronósticos para los próximos N días
    /// </summary>
    [HttpGet]
    public IActionResult GetForecasts([FromQuery] int days = 5)
    {
        try
        {
            var forecasts = _service.GetForecasts(days);
            
            var response = forecasts.Select(f => new
            {
                id = f.Id,
                date = f.Date.ToString("yyyy-MM-dd"),
                city = f.City.Name,
                temperature = new
                {
                    celsius = f.Temperature.Celsius,
                    fahrenheit = f.Temperature.Fahrenheit,
                    kelvin = f.Temperature.Kelvin,
                    description = f.Temperature.GetDescription()
                },
                condition = new
                {
                    name = f.Condition.ToString(),
                    description = f.Condition.GetDescription(),
                    emoji = f.Condition.GetEmoji()
                },
                summary = f.GetSummary(),
                recommendation = f.GetRecommendation(),
                flags = new
                {
                    isToday = f.IsToday(),
                    isTomorrow = f.IsTomorrow(),
                    requiresUmbrella = f.RequiresUmbrella(),
                    isDangerous = f.IsDangerous()
                }
            });

            return Ok(response);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation error");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting forecasts");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Obtiene pronóstico para una fecha y ciudad específica
    /// </summary>
    [HttpGet("{cityId}/{date}")]
    public IActionResult GetForecastForDate(int cityId, string date)
    {
        try
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
                return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd" });

            var forecast = _service.GetForecastForDate(parsedDate, cityId);

            var response = new
            {
                id = forecast.Id,
                date = forecast.Date.ToString("yyyy-MM-dd"),
                city = forecast.City.Name,
                temperature = new
                {
                    celsius = forecast.Temperature.Celsius,
                    fahrenheit = forecast.Temperature.Fahrenheit,
                    description = forecast.Temperature.GetDescription()
                },
                condition = forecast.Condition.GetDescription(),
                summary = forecast.GetSummary(),
                recommendation = forecast.GetRecommendation()
            };

            return Ok(response);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation error");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }
}
```

---

## Paso 7: Actualizar el Provider

**Archivo:** `Infrastructure/Providers/RandomWeatherForecastProvider.cs`

```csharp
using AspNetProject.Domain.Models;
using AspNetProject.Domain.Models.Entities;
using AspNetProject.Domain.Models.Enums;
using AspNetProject.Domain.Models.ValueObjects;
using AspNetProject.Domain.Ports.Out;

namespace AspNetProject.Infrastructure.Providers;

public class RandomWeatherForecastProvider : IWeatherForecastProvider
{
    private readonly IRepository<City, int> _cityRepository;
    private readonly Random _random = new();

    public RandomWeatherForecastProvider(IRepository<City, int> cityRepository)
    {
        _cityRepository = cityRepository;
    }

    public IEnumerable<WeatherForecast> GetForecasts(int days)
    {
        // Obtener una ciudad por defecto (o la primera disponible)
        var city = _cityRepository.GetAllAsync().Result.FirstOrDefault()
            ?? throw new InvalidOperationException("No cities available");

        var forecasts = new List<WeatherForecast>();

        for (int i = 0; i < days; i++)
        {
            var date = DateOnly.FromDateTime(DateTime.Today.AddDays(i));
            var temperature = Temperature.FromCelsius(_random.Next(-20, 40));
            var condition = (WeatherCondition)_random.Next(0, 15);

            var forecast = WeatherForecast.Create(date, temperature, condition, city);
            forecasts.Add(forecast);
        }

        return forecasts;
    }
}
```

---

## Resumen de Cambios

### ✅ Antes (Modelo Anémico)
```csharp
public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}
```

### ✅ Después (DDD)
```csharp
public class WeatherForecast : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public DateOnly Date { get; private set; }
    public Temperature Temperature { get; private set; }  // Value Object
    public WeatherCondition Condition { get; private set; }  // Enum rico
    public City City { get; private set; }  // Relación con otra entidad
    
    // Factory Method
    public static WeatherForecast Create(...) { }
    
    // Métodos de negocio
    public bool IsHot() => Temperature.IsHot();
    public string GetRecommendation() { }
}
```

---

## Beneficios Obtenidos

| Aspecto | Antes | Después |
|---------|-------|---------|
| **Validación** | En controlador | En el dominio |
| **Conversiones** | Manual | Automática (Value Object) |
| **Lógica de negocio** | Dispersa | Encapsulada en entidades |
| **Inmutabilidad** | No garantizada | Garantizada (setters privados) |
| **Testabilidad** | Difícil | Fácil (lógica en dominio) |
| **Expresividad** | Baja | Alta (lenguaje ubicuo) |

---

## Próximos Pasos

1. ✅ Crear los archivos de Value Objects
2. ✅ Crear las excepciones de dominio
3. ✅ Refactorizar WeatherForecast
4. ✅ Implementar Specifications
5. ✅ Actualizar el servicio
6. ✅ Actualizar el controlador
7. ⬜ Agregar tests unitarios
8. ⬜ Implementar Domain Events
9. ⬜ Agregar más especificaciones

---

## Comandos para Implementar

```bash
# 1. Crear estructura de carpetas
mkdir -p AspNetProject/Domain/Models/ValueObjects
mkdir -p AspNetProject/Domain/Models/Entities
mkdir -p AspNetProject/Domain/Models/Enums
mkdir -p AspNetProject/Domain/Exceptions
mkdir -p AspNetProject/Domain/Specifications

# 2. Compilar y verificar
dotnet build

# 3. Ejecutar tests (cuando los crees)
dotnet test

# 4. Ejecutar la aplicación
dotnet run --project AspNetProject
```

---

## Ejemplo de Respuesta de la API

**Antes:**
```json
[
  {
    "date": "2025-12-31",
    "temperatureC": 25,
    "temperatureF": 76,
    "summary": "Warm"
  }
]
```

**Después (con DDD):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "date": "2025-12-31",
    "city": "New York",
    "temperature": {
      "celsius": 25,
      "fahrenheit": 77,
      "kelvin": 298.15,
      "description": "Mild"
    },
    "condition": {
      "name": "Sunny",
      "description": "Clear skies and sunny",
      "emoji": "☀️"
    },
    "summary": "☀️ Clear skies and sunny - Mild",
    "recommendation": "✅ Pleasant weather expected!",
    "flags": {
      "isToday": false,
      "isTomorrow": true,
      "requiresUmbrella": false,
      "isDangerous": false
    }
  }
]
```

¡Mucho más rico y expresivo! 🎉
