# Guía de Entity Framework Core con PostgreSQL

Esta guía explica cómo usar Entity Framework Core con PostgreSQL en este proyecto, siguiendo la arquitectura hexagonal y evitando duplicación de código.

## 📋 Tabla de Contenidos

1. [Arquitectura de Datos](#arquitectura-de-datos)
2. [Configuración Inicial](#configuración-inicial)
3. [Crear una Nueva Entidad](#crear-una-nueva-entidad)
4. [Migraciones](#migraciones)
5. [Uso de Repositorios](#uso-de-repositorios)
6. [Mejores Prácticas](#mejores-prácticas)

---

## Arquitectura de Datos

### Patrón Repository Genérico

Este proyecto implementa un **repositorio genérico** que evita duplicación de código:

```
┌─────────────────────────────────────────────────────────────┐
│                    Domain Layer                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ IEntity<TId>          (Interfaz base de entidades)   │   │
│  │ IRepository<T, TId>   (Puerto de salida genérico)    │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ▲
                            │ implementa
                            │
┌─────────────────────────────────────────────────────────────┐
│                Infrastructure Layer                         │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ EfRepository<T, TId>  (Implementación genérica)      │   │
│  │   ├─ GetByIdAsync()                                  │   │
│  │   ├─ GetAllAsync()                                   │   │
│  │   ├─ FindAsync()                                     │   │
│  │   ├─ AddAsync()                                      │   │
│  │   ├─ UpdateAsync()                                   │   │
│  │   └─ DeleteAsync()                                   │   │
│  └──────────────────────────────────────────────────────┘   │
│                            ▲                                 │
│                            │ hereda                          │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ CityRepository        (Repositorio específico)       │   │
│  │   └─ GetByCountryAsync() (Métodos adicionales)      │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### Ventajas de este Enfoque

✅ **Sin duplicación de código**: CRUD básico implementado una sola vez  
✅ **Extensible**: Fácil agregar métodos específicos cuando sea necesario  
✅ **Type-safe**: Fuertemente tipado con genéricos  
✅ **Testeable**: Fácil crear mocks de `IRepository<T, TId>`  
✅ **Consistente**: Todas las entidades usan la misma interfaz  

---

## Configuración Inicial

### 1. Dependencias Instaladas

```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0"/>
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.1"/>
```

### 2. Cadena de Conexión

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

### 3. Configuración en Program.cs

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });
});

// Registro del repositorio genérico
builder.Services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
```

---

## Crear una Nueva Entidad

### Paso 1: Crear la Entidad en Domain/Models

```csharp
// Domain/Models/Product.cs
namespace AspNetProject.Domain.Models;

public class Product : IEntity<int>  // ⬅️ Implementa IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Importante**: 
- ✅ Implementa `IEntity<TId>` para usar el repositorio genérico
- ✅ Usa tipos de .NET estándar (no tipos de PostgreSQL)
- ✅ No uses atributos de EF aquí (mantén el dominio limpio)

### Paso 2: Crear la Configuración de EF

```csharp
// Infrastructure/Data/Configurations/ProductConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AspNetProject.Domain.Models;

namespace AspNetProject.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Tabla
        builder.ToTable("products");

        // Clave primaria
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        // Propiedades
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasPrecision(18, 2); // Para dinero

        builder.Property(p => p.Stock)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt);

        // Índices
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.Price);

        // Restricciones
        builder.HasCheckConstraint("CK_Product_Price", "price >= 0");
        builder.HasCheckConstraint("CK_Product_Stock", "stock >= 0");
    }
}
```

### Paso 3: Agregar DbSet al Contexto

```csharp
// Infrastructure/Data/ApplicationDbContext.cs
public DbSet<Product> Products => Set<Product>();
```

### Paso 4: (Opcional) Crear Repositorio Específico

**Solo si necesitas métodos adicionales**. Si solo necesitas CRUD básico, usa directamente `IRepository<Product, int>`.

```csharp
// Infrastructure/Repositories/ProductRepository.cs
using Microsoft.EntityFrameworkCore;
using AspNetProject.Domain.Models;
using AspNetProject.Infrastructure.Data;

namespace AspNetProject.Infrastructure.Repositories;

public class ProductRepository : EfRepository<Product, int>
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Métodos específicos de negocio
    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(
        decimal minPrice, 
        decimal maxPrice, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .OrderBy(p => p.Price)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockAsync(
        int threshold = 10, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.Stock < threshold)
            .OrderBy(p => p.Stock)
            .ToListAsync(cancellationToken);
    }
}
```

### Paso 5: Registrar en Program.cs (si creaste repositorio específico)

```csharp
// Solo si creaste ProductRepository
builder.Services.AddScoped<ProductRepository>();
```

---

## Migraciones

### Crear la Primera Migración

```bash
# Desde la raíz del proyecto
dotnet ef migrations add InitialCreate --project AspNetProject/AspNetProject.csproj
```

### Aplicar Migraciones

```bash
# Actualizar la base de datos
dotnet ef database update --project AspNetProject/AspNetProject.csproj
```

### Ver el SQL Generado

```bash
# Ver el script SQL sin aplicarlo
dotnet ef migrations script --project AspNetProject/AspNetProject.csproj
```

### Agregar Nueva Migración

```bash
# Después de modificar entidades
dotnet ef migrations add AddProductTable --project AspNetProject/AspNetProject.csproj
dotnet ef database update --project AspNetProject/AspNetProject.csproj
```

### Revertir Migración

```bash
# Revertir a una migración específica
dotnet ef database update PreviousMigrationName --project AspNetProject/AspNetProject.csproj

# Eliminar la última migración (si no se aplicó)
dotnet ef migrations remove --project AspNetProject/AspNetProject.csproj
```

---

## Uso de Repositorios

### Opción 1: Usar el Repositorio Genérico Directamente

**Recomendado para CRUD simple**

```csharp
// En tu servicio o controlador
public class ProductService
{
    private readonly IRepository<Product, int> _repository;

    public ProductService(IRepository<Product, int> repository)
    {
        _repository = repository;
    }

    public async Task<Product?> GetProductAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        return await _repository.AddAsync(product);
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(product);
    }

    public async Task DeleteProductAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    // Búsquedas personalizadas usando FindAsync
    public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
    {
        return await _repository.FindAsync(p => p.Name.Contains(name));
    }

    public async Task<IEnumerable<Product>> GetExpensiveProductsAsync()
    {
        return await _repository.FindAsync(p => p.Price > 1000);
    }
}
```

### Opción 2: Usar Repositorio Específico

**Cuando necesitas métodos de negocio complejos**

```csharp
public class ProductService
{
    private readonly ProductRepository _repository;

    public ProductService(ProductRepository repository)
    {
        _repository = repository;
    }

    // Métodos CRUD heredados de EfRepository
    public async Task<Product?> GetProductAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    // Métodos específicos de ProductRepository
    public async Task<IEnumerable<Product>> GetAffordableProductsAsync()
    {
        return await _repository.GetByPriceRangeAsync(0, 100);
    }

    public async Task<IEnumerable<Product>> GetProductsNeedingRestockAsync()
    {
        return await _repository.GetLowStockAsync(threshold: 5);
    }
}
```

### Registro en Program.cs

```csharp
// Opción 1: Solo repositorio genérico
builder.Services.AddScoped<IRepository<Product, int>, EfRepository<Product, int>>();
// Ya está cubierto por: builder.Services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));

// Opción 2: Repositorio específico
builder.Services.AddScoped<ProductRepository>();
```

---

## Mejores Prácticas

### 1. Separación de Responsabilidades

✅ **Dominio**: Solo entidades y lógica de negocio  
✅ **Infraestructura**: Configuraciones de EF, repositorios  
✅ **Aplicación**: Servicios que usan repositorios  

### 2. Convenciones de PostgreSQL

El proyecto usa **snake_case** automáticamente:

```csharp
// Clase C#
public class ProductCategory { }

// Tabla PostgreSQL
// product_category
```

### 3. Auditoría Automática

Puedes sobrescribir `SaveChangesAsync` para auditoría:

```csharp
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.Entity is IEntity<int> && 
                   (e.State == EntityState.Added || e.State == EntityState.Modified));

    foreach (var entry in entries)
    {
        if (entry.State == EntityState.Added)
        {
            ((dynamic)entry.Entity).CreatedAt = DateTime.UtcNow;
        }
        else if (entry.State == EntityState.Modified)
        {
            ((dynamic)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }

    return base.SaveChangesAsync(cancellationToken);
}
```

### 4. Transacciones

```csharp
public async Task TransferStockAsync(int fromProductId, int toProductId, int quantity)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        var fromProduct = await _repository.GetByIdAsync(fromProductId);
        var toProduct = await _repository.GetByIdAsync(toProductId);

        fromProduct.Stock -= quantity;
        toProduct.Stock += quantity;

        await _repository.UpdateAsync(fromProduct);
        await _repository.UpdateAsync(toProduct);

        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### 5. Paginación

```csharp
public async Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(
    int page, 
    int pageSize)
{
    var query = _context.Products.AsQueryable();
    
    var total = await query.CountAsync();
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (items, total);
}
```

### 6. Eager Loading (Relaciones)

```csharp
// Si Product tiene Category
public async Task<Product?> GetProductWithCategoryAsync(int id)
{
    return await _context.Products
        .Include(p => p.Category)
        .FirstOrDefaultAsync(p => p.Id == id);
}
```

---

## Docker Compose para PostgreSQL

Crea `docker-compose.yml` en la raíz:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    container_name: aspnetproject-postgres
    environment:
      POSTGRES_DB: aspnetproject
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:
```

Ejecutar:

```bash
docker-compose up -d
dotnet ef database update --project AspNetProject/AspNetProject.csproj
```

---

## Resumen de Archivos Creados

```
AspNetProject/
├── Domain/
│   ├── Models/
│   │   ├── IEntity.cs                    ✅ Interfaz base
│   │   └── City.cs                       ✅ Entidad ejemplo
│   └── Ports/
│       └── IRepository.cs                ✅ Puerto genérico
│
├── Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs       ✅ DbContext PostgreSQL
│   │   └── Configurations/
│   │       └── CityConfiguration.cs      ✅ Configuración EF
│   └── Repositories/
│       ├── EfRepository.cs               ✅ Repositorio genérico
│       └── CityRepository.cs             ✅ Repositorio específico
│
├── appsettings.json                      ✅ Cadena de conexión
└── .env.example                          ✅ Ejemplo de configuración
```

---

## Próximos Pasos

1. **Crear más entidades** siguiendo el patrón mostrado
2. **Agregar relaciones** entre entidades (1:N, N:M)
3. **Implementar Unit of Work** si necesitas transacciones complejas
4. **Agregar caché** con Redis para consultas frecuentes
5. **Implementar CQRS** si la aplicación crece en complejidad

---

**¡Listo!** Ahora tienes una implementación robusta y reutilizable de Entity Framework Core con PostgreSQL. 🚀
