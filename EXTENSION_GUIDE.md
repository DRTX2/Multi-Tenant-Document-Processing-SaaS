# Guía de Extensión - AspNetProject

Esta guía muestra cómo extender el proyecto manteniendo la arquitectura hexagonal.

## 📋 Tabla de Contenidos

1. [Agregar una Nueva Entidad](#1-agregar-una-nueva-entidad)
2. [Agregar Persistencia con Entity Framework](#2-agregar-persistencia-con-entity-framework)
3. [Agregar Autenticación JWT](#3-agregar-autenticación-jwt)
4. [Agregar Validaciones con FluentValidation](#4-agregar-validaciones-con-fluentvalidation)
5. [Agregar Tests Unitarios](#5-agregar-tests-unitarios)
6. [Agregar GraphQL](#6-agregar-graphql)

---

## 1. Agregar una Nueva Entidad

### Ejemplo: Agregar gestión de Ciudades (Cities)

#### Paso 1: Crear la Entidad de Dominio

```csharp
// Domain/Models/City.cs
namespace AspNetProject.Domain.Models;

/// <summary>
/// Entidad de dominio que representa una ciudad
/// </summary>
public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
```

#### Paso 2: Crear el Puerto de Salida

```csharp
// Domain/Ports/ICityRepository.cs
using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports;

/// <summary>
/// Puerto de salida para acceso a datos de ciudades
/// </summary>
public interface ICityRepository
{
    Task<IEnumerable<City>> GetAllAsync();
    Task<City?> GetByIdAsync(int id);
    Task<City> CreateAsync(City city);
    Task<City> UpdateAsync(City city);
    Task DeleteAsync(int id);
}
```

#### Paso 3: Crear el Puerto de Entrada (Use Case)

```csharp
// Domain/Ports/ICityService.cs
using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports;

/// <summary>
/// Puerto de entrada para casos de uso de ciudades
/// </summary>
public interface ICityService
{
    Task<IEnumerable<City>> GetAllCitiesAsync();
    Task<City?> GetCityByIdAsync(int id);
    Task<City> CreateCityAsync(City city);
    Task<City> UpdateCityAsync(int id, City city);
    Task DeleteCityAsync(int id);
}
```

#### Paso 4: Implementar el Caso de Uso

```csharp
// Application/Services/CityService.cs
using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports;

namespace AspNetProject.Application.Services;

/// <summary>
/// Implementación del caso de uso de ciudades
/// </summary>
public class CityService : ICityService
{
    private readonly ICityRepository _repository;

    public CityService(ICityRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<City>> GetAllCitiesAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<City?> GetCityByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El ID debe ser mayor a cero", nameof(id));
        }
        
        return await _repository.GetByIdAsync(id);
    }

    public async Task<City> CreateCityAsync(City city)
    {
        // Validaciones de negocio
        if (string.IsNullOrWhiteSpace(city.Name))
        {
            throw new ArgumentException("El nombre de la ciudad es requerido");
        }
        
        if (city.Latitude < -90 || city.Latitude > 90)
        {
            throw new ArgumentException("Latitud inválida");
        }
        
        if (city.Longitude < -180 || city.Longitude > 180)
        {
            throw new ArgumentException("Longitud inválida");
        }

        return await _repository.CreateAsync(city);
    }

    public async Task<City> UpdateCityAsync(int id, City city)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Ciudad con ID {id} no encontrada");
        }

        city.Id = id;
        return await _repository.UpdateAsync(city);
    }

    public async Task DeleteCityAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Ciudad con ID {id} no encontrada");
        }

        await _repository.DeleteAsync(id);
    }
}
```

#### Paso 5: Implementar el Adaptador de Infraestructura

```csharp
// Infrastructure/Repositories/InMemoryCityRepository.cs
using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports;

namespace AspNetProject.Infrastructure.Repositories;

/// <summary>
/// Implementación en memoria del repositorio de ciudades
/// </summary>
public class InMemoryCityRepository : ICityRepository
{
    private readonly List<City> _cities = new()
    {
        new City { Id = 1, Name = "Madrid", Country = "España", Latitude = 40.4168, Longitude = -3.7038 },
        new City { Id = 2, Name = "Barcelona", Country = "España", Latitude = 41.3851, Longitude = 2.1734 },
        new City { Id = 3, Name = "Valencia", Country = "España", Latitude = 39.4699, Longitude = -0.3763 }
    };
    
    private int _nextId = 4;

    public Task<IEnumerable<City>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<City>>(_cities);
    }

    public Task<City?> GetByIdAsync(int id)
    {
        var city = _cities.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(city);
    }

    public Task<City> CreateAsync(City city)
    {
        city.Id = _nextId++;
        _cities.Add(city);
        return Task.FromResult(city);
    }

    public Task<City> UpdateAsync(City city)
    {
        var index = _cities.FindIndex(c => c.Id == city.Id);
        if (index >= 0)
        {
            _cities[index] = city;
        }
        return Task.FromResult(city);
    }

    public Task DeleteAsync(int id)
    {
        var city = _cities.FirstOrDefault(c => c.Id == id);
        if (city != null)
        {
            _cities.Remove(city);
        }
        return Task.CompletedTask;
    }
}
```

#### Paso 6: Crear el Controlador

```csharp
// Api/Controllers/CityController.cs
using Microsoft.AspNetCore.Mvc;
using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports;

namespace AspNetProject.Api.Controllers;

/// <summary>
/// Controlador REST para gestión de ciudades
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CityController : ControllerBase
{
    private readonly ICityService _service;

    public CityController(ICityService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<City>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<City>>> GetAll()
    {
        var cities = await _service.GetAllCitiesAsync();
        return Ok(cities);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<City>> GetById(int id)
    {
        try
        {
            var city = await _service.GetCityByIdAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            return Ok(city);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(City), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<City>> Create([FromBody] City city)
    {
        try
        {
            var created = await _service.CreateCityAsync(city);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<City>> Update(int id, [FromBody] City city)
    {
        try
        {
            var updated = await _service.UpdateCityAsync(id, city);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteCityAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
```

#### Paso 7: Registrar en Program.cs

```csharp
// Agregar estas líneas en Program.cs
builder.Services.AddScoped<ICityRepository, InMemoryCityRepository>();
builder.Services.AddScoped<ICityService, CityService>();
```

---

## 2. Agregar Persistencia con Entity Framework

### Paso 1: Instalar paquetes

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Paso 2: Crear DbContext

```csharp
// Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using AspNetProject.Domain.Models;

namespace AspNetProject.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<City> Cities => Set<City>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
        });
    }
}
```

### Paso 3: Implementar Repositorio con EF

```csharp
// Infrastructure/Repositories/EfCityRepository.cs
using Microsoft.EntityFrameworkCore;
using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports;
using AspNetProject.Infrastructure.Data;

namespace AspNetProject.Infrastructure.Repositories;

public class EfCityRepository : ICityRepository
{
    private readonly ApplicationDbContext _context;

    public EfCityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<City>> GetAllAsync()
    {
        return await _context.Cities.ToListAsync();
    }

    public async Task<City?> GetByIdAsync(int id)
    {
        return await _context.Cities.FindAsync(id);
    }

    public async Task<City> CreateAsync(City city)
    {
        _context.Cities.Add(city);
        await _context.SaveChangesAsync();
        return city;
    }

    public async Task<City> UpdateAsync(City city)
    {
        _context.Entry(city).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return city;
    }

    public async Task DeleteAsync(int id)
    {
        var city = await _context.Cities.FindAsync(id);
        if (city != null)
        {
            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();
        }
    }
}
```

### Paso 4: Configurar en Program.cs

```csharp
// Agregar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// Cambiar el registro del repositorio
builder.Services.AddScoped<ICityRepository, EfCityRepository>();
```

### Paso 5: Crear y aplicar migraciones

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## 3. Agregar Autenticación JWT

### Paso 1: Instalar paquetes

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### Paso 2: Configurar en Program.cs

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Configurar JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-secret-key-min-32-chars-long";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AspNetProject";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Después de app.UseAuthorization();
app.UseAuthentication();
app.UseAuthorization();
```

### Paso 3: Proteger endpoints

```csharp
using Microsoft.AspNetCore.Authorization;

[Authorize] // Requiere autenticación
[HttpPost]
public async Task<ActionResult<City>> Create([FromBody] City city)
{
    // ...
}
```

---

## 4. Agregar Validaciones con FluentValidation

### Paso 1: Instalar paquete

```bash
dotnet add package FluentValidation.AspNetCore
```

### Paso 2: Crear validador

```csharp
// Application/Validators/CityValidator.cs
using FluentValidation;
using AspNetProject.Domain.Models;

namespace AspNetProject.Application.Validators;

public class CityValidator : AbstractValidator<City>
{
    public CityValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("El país es requerido")
            .MaximumLength(100).WithMessage("El país no puede exceder 100 caracteres");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("La latitud debe estar entre -90 y 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("La longitud debe estar entre -180 y 180");
    }
}
```

### Paso 3: Configurar en Program.cs

```csharp
using FluentValidation;

builder.Services.AddValidatorsFromAssemblyContaining<CityValidator>();
```

### Paso 4: Usar en el servicio

```csharp
public class CityService : ICityService
{
    private readonly ICityRepository _repository;
    private readonly IValidator<City> _validator;

    public CityService(ICityRepository repository, IValidator<City> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<City> CreateCityAsync(City city)
    {
        var validationResult = await _validator.ValidateAsync(city);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        return await _repository.CreateAsync(city);
    }
}
```

---

## 5. Agregar Tests Unitarios

### Paso 1: Crear proyecto de tests

```bash
dotnet new xunit -n AspNetProject.Tests
cd AspNetProject.Tests
dotnet add reference ../AspNetProject/AspNetProject.csproj
dotnet add package Moq
dotnet add package FluentAssertions
```

### Paso 2: Crear tests del servicio

```csharp
// AspNetProject.Tests/Application/Services/CityServiceTests.cs
using Xunit;
using Moq;
using FluentAssertions;
using AspNetProject.Application.Services;
using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports;

namespace AspNetProject.Tests.Application.Services;

public class CityServiceTests
{
    private readonly Mock<ICityRepository> _repositoryMock;
    private readonly CityService _service;

    public CityServiceTests()
    {
        _repositoryMock = new Mock<ICityRepository>();
        _service = new CityService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllCitiesAsync_ShouldReturnAllCities()
    {
        // Arrange
        var cities = new List<City>
        {
            new City { Id = 1, Name = "Madrid", Country = "España" },
            new City { Id = 2, Name = "Barcelona", Country = "España" }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cities);

        // Act
        var result = await _service.GetAllCitiesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(cities);
    }

    [Fact]
    public async Task GetCityByIdAsync_WithInvalidId_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetCityByIdAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetCityByIdAsync(-1));
    }

    [Fact]
    public async Task CreateCityAsync_WithValidCity_ShouldCreateCity()
    {
        // Arrange
        var city = new City
        {
            Name = "Sevilla",
            Country = "España",
            Latitude = 37.3891,
            Longitude = -5.9845
        };
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<City>())).ReturnsAsync(city);

        // Act
        var result = await _service.CreateCityAsync(city);

        // Assert
        result.Should().BeEquivalentTo(city);
        _repositoryMock.Verify(r => r.CreateAsync(city), Times.Once);
    }

    [Theory]
    [InlineData("", "España", 40.0, -3.0)] // Nombre vacío
    [InlineData("Madrid", "", 40.0, -3.0)] // País vacío
    [InlineData("Madrid", "España", 91.0, -3.0)] // Latitud inválida
    [InlineData("Madrid", "España", 40.0, 181.0)] // Longitud inválida
    public async Task CreateCityAsync_WithInvalidData_ShouldThrowArgumentException(
        string name, string country, double lat, double lon)
    {
        // Arrange
        var city = new City
        {
            Name = name,
            Country = country,
            Latitude = lat,
            Longitude = lon
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateCityAsync(city));
    }
}
```

### Paso 3: Ejecutar tests

```bash
dotnet test
```

---

## 6. Agregar GraphQL

### Paso 1: Instalar paquetes

```bash
dotnet add package HotChocolate.AspNetCore
```

### Paso 2: Crear Query

```csharp
// Api/GraphQL/Query.cs
using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports;

namespace AspNetProject.Api.GraphQL;

public class Query
{
    public async Task<IEnumerable<City>> GetCities([Service] ICityService service)
    {
        return await service.GetAllCitiesAsync();
    }

    public async Task<City?> GetCity(int id, [Service] ICityService service)
    {
        return await service.GetCityByIdAsync(id);
    }

    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecasts(
        int days,
        [Service] IWeatherForecastService service)
    {
        return service.GetForecasts(days);
    }
}
```

### Paso 3: Crear Mutation

```csharp
// Api/GraphQL/Mutation.cs
using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports;

namespace AspNetProject.Api.GraphQL;

public class Mutation
{
    public async Task<City> CreateCity(City input, [Service] ICityService service)
    {
        return await service.CreateCityAsync(input);
    }

    public async Task<City> UpdateCity(int id, City input, [Service] ICityService service)
    {
        return await service.UpdateCityAsync(id, input);
    }

    public async Task<bool> DeleteCity(int id, [Service] ICityService service)
    {
        await service.DeleteCityAsync(id);
        return true;
    }
}
```

### Paso 4: Configurar en Program.cs

```csharp
using AspNetProject.Api.GraphQL;

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

// Después de app.MapControllers();
app.MapGraphQL();
```

### Paso 5: Probar GraphQL

Acceder a: `https://localhost:5001/graphql`

```graphql
query {
  cities {
    id
    name
    country
    latitude
    longitude
  }
}

mutation {
  createCity(input: {
    name: "Bilbao"
    country: "España"
    latitude: 43.2630
    longitude: -2.9350
  }) {
    id
    name
  }
}
```

---

## 📚 Recursos Adicionales

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [FluentValidation](https://docs.fluentvalidation.net)
- [HotChocolate GraphQL](https://chillicream.com/docs/hotchocolate)
- [xUnit Testing](https://xunit.net)
- [Moq Framework](https://github.com/moq/moq4)
