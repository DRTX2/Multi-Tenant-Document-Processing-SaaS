# 🚀 Inicio Rápido - AspNetProject

## Requisitos Previos

- ✅ .NET 10 SDK
- ✅ Docker y Docker Compose
- ✅ Editor de código (Visual Studio, Rider, VS Code)

---

## ⚡ Inicio en 3 Pasos

### 1️⃣ Iniciar PostgreSQL

```bash
docker-compose up -d
```

Esto iniciará:
- PostgreSQL en `localhost:5432`
- pgAdmin en `http://localhost:5050`

### 2️⃣ Aplicar Migraciones

```bash
dotnet ef migrations add InitialCreate --project AspNetProject/AspNetProject.csproj
dotnet ef database update --project AspNetProject/AspNetProject.csproj
```

### 3️⃣ Ejecutar la Aplicación

```bash
dotnet run --project AspNetProject/AspNetProject.csproj
```

Acceder a Swagger: **https://localhost:5001**

---

## 📝 Agregar una Nueva Entidad (5 minutos)

### 1. Crear Entidad (Domain/Models/Product.cs)

```csharp
public class Product : IEntity<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 2. Crear Configuración (Infrastructure/Data/Configurations/ProductConfiguration.cs)

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Price).HasPrecision(18, 2);
    }
}
```

### 3. Agregar DbSet (Infrastructure/Data/ApplicationDbContext.cs)

```csharp
public DbSet<Product> Products => Set<Product>();
```

### 4. Migración

```bash
dotnet ef migrations add AddProductTable --project AspNetProject/AspNetProject.csproj
dotnet ef database update --project AspNetProject/AspNetProject.csproj
```

### 5. Usar en Servicio

```csharp
public class ProductService
{
    private readonly IRepository<Product, int> _repo;
    
    public ProductService(IRepository<Product, int> repo) => _repo = repo;
    
    public Task<Product?> GetAsync(int id) => _repo.GetByIdAsync(id);
    public Task<IEnumerable<Product>> GetAllAsync() => _repo.GetAllAsync();
    public Task<Product> CreateAsync(Product p) => _repo.AddAsync(p);
}
```

**¡Listo!** No necesitas crear repositorio, todo está en `IRepository<Product, int>`.

---

## 🔧 Comandos Útiles

### Docker

```bash
# Iniciar
docker-compose up -d

# Ver logs
docker-compose logs -f postgres

# Detener
docker-compose down

# Detener y eliminar datos
docker-compose down -v
```

### Migraciones

```bash
# Crear migración
dotnet ef migrations add NombreMigracion --project AspNetProject/AspNetProject.csproj

# Aplicar
dotnet ef database update --project AspNetProject/AspNetProject.csproj

# Ver SQL
dotnet ef migrations script --project AspNetProject/AspNetProject.csproj

# Revertir
dotnet ef database update MigracionAnterior --project AspNetProject/AspNetProject.csproj

# Eliminar última (si no se aplicó)
dotnet ef migrations remove --project AspNetProject/AspNetProject.csproj
```

### Desarrollo

```bash
# Compilar
dotnet build

# Ejecutar
dotnet run --project AspNetProject/AspNetProject.csproj

# Ejecutar con recarga automática
dotnet watch run --project AspNetProject/AspNetProject.csproj

# Limpiar
dotnet clean
```

### Base de Datos

```bash
# Conectar a PostgreSQL
docker exec -it aspnetproject-postgres psql -U postgres -d aspnetproject

# Comandos en psql:
\dt              # Listar tablas
\d table_name    # Describir tabla
\l               # Listar bases de datos
\q               # Salir
```

---

## 📚 Documentación

| Archivo | Descripción |
|---------|-------------|
| [RESUMEN_FINAL.md](RESUMEN_FINAL.md) | 📋 Resumen ejecutivo completo |
| [README.md](README.md) | 📖 Guía principal del proyecto |
| [DATABASE_GUIDE.md](DATABASE_GUIDE.md) | 🗄️ Guía completa de EF Core |
| [EXTENSION_GUIDE.md](EXTENSION_GUIDE.md) | 🔧 Cómo extender el proyecto |
| [ARCHITECTURE.md](ARCHITECTURE.md) | 🏗️ Diagramas y principios |
| [EF_CORE_SUMMARY.md](EF_CORE_SUMMARY.md) | 📊 Resumen de EF Core |

---

## 🌐 URLs Importantes

| Servicio | URL | Credenciales |
|----------|-----|--------------|
| **Swagger UI** | https://localhost:5001 | - |
| **API** | https://localhost:5001/api | - |
| **pgAdmin** | http://localhost:5050 | admin@aspnetproject.com / admin |
| **PostgreSQL** | localhost:5432 | postgres / postgres |

---

## 💡 Ejemplos Rápidos

### Obtener todos los registros

```csharp
var cities = await _repository.GetAllAsync();
```

### Buscar por ID

```csharp
var city = await _repository.GetByIdAsync(1);
```

### Búsqueda con filtro

```csharp
var cities = await _repository.FindAsync(c => c.Country == "España");
```

### Crear

```csharp
var city = new City { Name = "Madrid", Country = "España" };
await _repository.AddAsync(city);
```

### Actualizar

```csharp
city.Name = "Madrid Capital";
await _repository.UpdateAsync(city);
```

### Eliminar

```csharp
await _repository.DeleteAsync(1);
```

---

## ❓ Problemas Comunes

### Error: "No se puede conectar a PostgreSQL"

```bash
# Verificar que Docker esté corriendo
docker-compose ps

# Reiniciar servicios
docker-compose restart
```

### Error: "Tabla no existe"

```bash
# Aplicar migraciones
dotnet ef database update --project AspNetProject/AspNetProject.csproj
```

### Error: "Puerto 5432 en uso"

```bash
# Cambiar puerto en docker-compose.yml
ports:
  - "5433:5432"  # Usar 5433 en lugar de 5432

# Actualizar appsettings.json
"DefaultConnection": "Host=localhost;Port=5433;..."
```

---

## 🎯 Próximos Pasos

1. ✅ **Leer [RESUMEN_FINAL.md](RESUMEN_FINAL.md)** - Entender qué se implementó
2. ✅ **Probar la API** - Usar Swagger UI
3. ✅ **Agregar tu primera entidad** - Seguir los 5 pasos arriba
4. ✅ **Leer [DATABASE_GUIDE.md](DATABASE_GUIDE.md)** - Aprender más sobre EF Core
5. ✅ **Extender el proyecto** - Ver [EXTENSION_GUIDE.md](EXTENSION_GUIDE.md)

---

**¡Listo para empezar! 🚀**
