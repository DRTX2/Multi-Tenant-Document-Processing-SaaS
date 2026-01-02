# Testing de Paginación

## 🧪 Tests Unitarios

### 1. Tests de PageRequest

```csharp
using Xunit;
using AspNetProject.Domain.ValueObjects;

public class PageRequestTests
{
    [Fact]
    public void Of_WithValidParameters_CreatesPageRequest()
    {
        // Arrange & Act
        var pageRequest = PageRequest.Of(0, 20, "Name", true);

        // Assert
        Assert.Equal(0, pageRequest.PageNumber);
        Assert.Equal(20, pageRequest.PageSize);
        Assert.Equal("Name", pageRequest.SortBy);
        Assert.True(pageRequest.SortAscending);
        Assert.Equal(0, pageRequest.Skip); // 0 * 20 = 0
    }

    [Fact]
    public void Skip_CalculatesCorrectly()
    {
        // Arrange
        var page0 = PageRequest.Of(0, 20);
        var page1 = PageRequest.Of(1, 20);
        var page2 = PageRequest.Of(2, 20);

        // Assert
        Assert.Equal(0, page0.Skip);   // 0 * 20 = 0
        Assert.Equal(20, page1.Skip);  // 1 * 20 = 20
        Assert.Equal(40, page2.Skip);  // 2 * 20 = 40
    }

    [Fact]
    public void Constructor_WithNegativePageNumber_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new PageRequest(-1, 20));
    }

    [Fact]
    public void Constructor_WithZeroPageSize_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new PageRequest(0, 0));
    }

    [Fact]
    public void Constructor_WithPageSizeExceedingMax_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new PageRequest(0, 101)); // Max is 100
    }

    [Fact]
    public void NextPage_ReturnsCorrectPage()
    {
        // Arrange
        var page0 = PageRequest.Of(0, 20, "Name", true);

        // Act
        var page1 = page0.NextPage();

        // Assert
        Assert.Equal(1, page1.PageNumber);
        Assert.Equal(20, page1.PageSize);
        Assert.Equal("Name", page1.SortBy);
        Assert.True(page1.SortAscending);
    }

    [Fact]
    public void PreviousPage_FromFirstPage_ReturnsSamePage()
    {
        // Arrange
        var page0 = PageRequest.Of(0, 20);

        // Act
        var previous = page0.PreviousPage();

        // Assert
        Assert.Equal(0, previous.PageNumber);
    }

    [Fact]
    public void PreviousPage_FromSecondPage_ReturnsFirstPage()
    {
        // Arrange
        var page1 = PageRequest.Of(1, 20);

        // Act
        var page0 = page1.PreviousPage();

        // Assert
        Assert.Equal(0, page0.PageNumber);
    }
}
```

### 2. Tests de PagedResult

```csharp
using Xunit;
using AspNetProject.Domain.ValueObjects;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_CalculatesCorrectly()
    {
        // Arrange & Act
        var result = new PagedResult<string>
        {
            Items = new List<string> { "A", "B" },
            PageNumber = 0,
            PageSize = 20,
            TotalCount = 150
        };

        // Assert
        Assert.Equal(8, result.TotalPages); // Ceiling(150 / 20) = 8
    }

    [Fact]
    public void HasPreviousPage_OnFirstPage_ReturnsFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            PageNumber = 0,
            PageSize = 20,
            TotalCount = 100
        };

        // Assert
        Assert.False(result.HasPreviousPage);
        Assert.True(result.IsFirstPage);
    }

    [Fact]
    public void HasNextPage_OnLastPage_ReturnsFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            PageNumber = 4, // Last page (5 total pages)
            PageSize = 20,
            TotalCount = 100 // 100 / 20 = 5 pages
        };

        // Assert
        Assert.False(result.HasNextPage);
        Assert.True(result.IsLastPage);
    }

    [Fact]
    public void Map_TransformsItems()
    {
        // Arrange
        var result = new PagedResult<int>
        {
            Items = new List<int> { 1, 2, 3 },
            PageNumber = 0,
            PageSize = 20,
            TotalCount = 3
        };

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        Assert.Equal(new[] { "1", "2", "3" }, mapped.Items);
        Assert.Equal(0, mapped.PageNumber);
        Assert.Equal(20, mapped.PageSize);
        Assert.Equal(3, mapped.TotalCount);
    }

    [Fact]
    public void Empty_CreatesEmptyResult()
    {
        // Act
        var result = PagedResult<string>.Empty(0, 20);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.PageNumber);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }
}
```

### 3. Tests de CityQueryService

```csharp
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.Out;
using AspNetProject.Domain.ValueObjects;
using AspNetProject.Infrastructure.Data;
using AspNetProject.Infrastructure.Persistence;

public class CityQueryServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly CityQueryService _service;

    public CityQueryServiceTests()
    {
        // Configurar DbContext en memoria
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new CityQueryService(_context);

        // Seed data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var cities = new List<City>
        {
            new City { Id = 1, Name = "Bogotá", Country = "Colombia", 
                      Latitude = 4.7110, Longitude = -74.0721, 
                      CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new City { Id = 2, Name = "Medellín", Country = "Colombia", 
                      Latitude = 6.2442, Longitude = -75.5812, 
                      CreatedAt = DateTime.UtcNow.AddDays(-9) },
            new City { Id = 3, Name = "Cali", Country = "Colombia", 
                      Latitude = 3.4516, Longitude = -76.5320, 
                      CreatedAt = DateTime.UtcNow.AddDays(-8) },
            new City { Id = 4, Name = "Buenos Aires", Country = "Argentina", 
                      Latitude = -34.6037, Longitude = -58.3816, 
                      CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new City { Id = 5, Name = "Lima", Country = "Perú", 
                      Latitude = -12.0464, Longitude = -77.0428, 
                      CreatedAt = DateTime.UtcNow.AddDays(-6) }
        };

        _context.Cities.AddRange(cities);
        _context.SaveChanges();
    }

    [Fact]
    public async Task SearchAsync_WithNoFilters_ReturnsAllCities()
    {
        // Arrange
        var criteria = CitySearchCriteria.Empty();
        var pageRequest = PageRequest.Of(0, 10);

        // Act
        var result = await _service.SearchAsync(criteria, pageRequest);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task SearchAsync_WithCountryFilter_ReturnsFilteredCities()
    {
        // Arrange
        var criteria = new CitySearchCriteria { Country = "Colombia" };
        var pageRequest = PageRequest.Of(0, 10);

        // Act
        var result = await _service.SearchAsync(criteria, pageRequest);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
        Assert.All(result.Items, city => Assert.Equal("Colombia", city.Country));
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var criteria = CitySearchCriteria.Empty();
        var pageRequest = PageRequest.Of(0, 2); // 2 items per page

        // Act - First page
        var page0 = await _service.SearchAsync(criteria, pageRequest);

        // Assert - First page
        Assert.Equal(5, page0.TotalCount);
        Assert.Equal(2, page0.Items.Count);
        Assert.Equal(3, page0.TotalPages);
        Assert.True(page0.HasNextPage);
        Assert.False(page0.HasPreviousPage);

        // Act - Second page
        var page1 = await _service.SearchAsync(criteria, pageRequest.NextPage());

        // Assert - Second page
        Assert.Equal(5, page1.TotalCount);
        Assert.Equal(2, page1.Items.Count);
        Assert.True(page1.HasNextPage);
        Assert.True(page1.HasPreviousPage);
    }

    [Fact]
    public async Task SearchAsync_WithSorting_ReturnsSortedResults()
    {
        // Arrange
        var criteria = CitySearchCriteria.Empty();
        var pageRequest = PageRequest.Of(0, 10, "Name", true);

        // Act
        var result = await _service.SearchAsync(criteria, pageRequest);

        // Assert
        var names = result.Items.Select(c => c.Name).ToList();
        Assert.Equal(new[] { "Bogotá", "Buenos Aires", "Cali", "Lima", "Medellín" }, names);
    }

    [Fact]
    public async Task SearchAsync_WithDescendingSorting_ReturnsSortedResults()
    {
        // Arrange
        var criteria = CitySearchCriteria.Empty();
        var pageRequest = PageRequest.Of(0, 10, "Name", false);

        // Act
        var result = await _service.SearchAsync(criteria, pageRequest);

        // Assert
        var names = result.Items.Select(c => c.Name).ToList();
        Assert.Equal(new[] { "Medellín", "Lima", "Cali", "Buenos Aires", "Bogotá" }, names);
    }

    [Fact]
    public async Task GetAllAsync_WithFewRecords_ReturnsAll()
    {
        // Act
        var cities = await _service.GetAllAsync();

        // Assert
        Assert.Equal(5, cities.Count());
    }

    [Fact]
    public async Task GetByCountryAsync_ReturnsFilteredCities()
    {
        // Act
        var cities = await _service.GetByCountryAsync("Colombia");

        // Assert
        Assert.Equal(3, cities.Count());
        Assert.All(cities, city => Assert.Equal("Colombia", city.Country));
    }

    [Fact]
    public async Task SearchByNameAsync_ReturnsMatchingCities()
    {
        // Act
        var cities = await _service.SearchByNameAsync("Bog");

        // Assert
        Assert.Single(cities);
        Assert.Equal("Bogotá", cities.First().Name);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

## 🎯 Tests de Integración

### Controller Tests

```csharp
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using AspNetProject.Domain.ValueObjects;

public class CitiesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CitiesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Search_WithValidParameters_ReturnsPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/cities?page=0&size=20");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CityDto>>();
        
        Assert.NotNull(result);
        Assert.True(result.PageNumber >= 0);
        Assert.True(result.PageSize > 0);
        Assert.True(result.TotalCount >= 0);
    }

    [Fact]
    public async Task Search_WithCountryFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/cities?country=Colombia&page=0&size=20");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CityDto>>();
        
        Assert.NotNull(result);
        Assert.All(result.Items, city => Assert.Equal("Colombia", city.Country));
    }

    [Fact]
    public async Task Search_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/cities?page=0&size=0");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithSorting_ReturnsSortedResults()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/cities?page=0&size=10&sortBy=Name&ascending=true");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CityDto>>();
        
        Assert.NotNull(result);
        var names = result.Items.Select(c => c.Name).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }
}
```

## 📊 Tests de Performance

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class PaginationBenchmarks
{
    private ApplicationDbContext _context;
    private CityQueryService _service;

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "BenchmarkDb")
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new CityQueryService(_context);

        // Seed 10,000 cities
        var cities = Enumerable.Range(1, 10000)
            .Select(i => new City
            {
                Id = i,
                Name = $"City {i}",
                Country = $"Country {i % 100}",
                Latitude = i * 0.1,
                Longitude = i * 0.1,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        _context.Cities.AddRange(cities);
        _context.SaveChanges();
    }

    [Benchmark]
    public async Task<PagedResult<City>> SearchAsync_Page0_Size20()
    {
        var criteria = CitySearchCriteria.Empty();
        var pageRequest = PageRequest.Of(0, 20);
        return await _service.SearchAsync(criteria, pageRequest);
    }

    [Benchmark]
    public async Task<PagedResult<City>> SearchAsync_Page50_Size20()
    {
        var criteria = CitySearchCriteria.Empty();
        var pageRequest = PageRequest.Of(50, 20);
        return await _service.SearchAsync(criteria, pageRequest);
    }

    [Benchmark]
    public async Task<PagedResult<City>> SearchAsync_WithFilter_Page0_Size20()
    {
        var criteria = new CitySearchCriteria { Country = "Country 1" };
        var pageRequest = PageRequest.Of(0, 20);
        return await _service.SearchAsync(criteria, pageRequest);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

// Para ejecutar:
// dotnet run -c Release --project BenchmarkProject
```

## 🧪 Tests de Casos Edge

```csharp
public class PaginationEdgeCasesTests
{
    [Fact]
    public async Task SearchAsync_EmptyDatabase_ReturnsEmptyResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        var service = new CityQueryService(context);

        var criteria = CitySearchCriteria.Empty();
        var pageRequest = PageRequest.Of(0, 20);

        // Act
        var result = await service.SearchAsync(criteria, pageRequest);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task SearchAsync_RequestingPageBeyondTotal_ReturnsEmptyPage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        
        context.Cities.Add(new City 
        { 
            Id = 1, 
            Name = "Test", 
            Country = "Test",
            Latitude = 0,
            Longitude = 0,
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();

        var service = new CityQueryService(context);
        var criteria = CitySearchCriteria.Empty();
        var pageRequest = PageRequest.Of(10, 20); // Page 10 when only 1 item exists

        // Act
        var result = await service.SearchAsync(criteria, pageRequest);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetAllAsync_WithMoreThan1000Records_ThrowsException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        // Add 1001 cities
        var cities = Enumerable.Range(1, 1001)
            .Select(i => new City
            {
                Id = i,
                Name = $"City {i}",
                Country = "Test",
                Latitude = 0,
                Longitude = 0,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        context.Cities.AddRange(cities);
        context.SaveChanges();

        var service = new CityQueryService(context);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.GetAllAsync());
    }
}
```

## 🎯 Resumen de Testing

### Cobertura Recomendada

```
✅ Value Objects
   - PageRequest: Validaciones, cálculos, navegación
   - PagedResult: Metadatos, transformaciones

✅ Query Services
   - Filtrado correcto
   - Paginación correcta
   - Ordenamiento correcto
   - Conteo total correcto

✅ Controllers
   - Parámetros válidos
   - Parámetros inválidos
   - Mapeo a DTOs

✅ Integración
   - End-to-end con base de datos real
   - Casos edge (vacío, página inexistente)

✅ Performance
   - Benchmarks con datasets grandes
   - Verificar que no carga todo en memoria
```
