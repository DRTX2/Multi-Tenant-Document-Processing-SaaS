using Microsoft.EntityFrameworkCore;
using AspNetProject.Domain.Models;

namespace AspNetProject.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos principal de la aplicación
/// Configurado para PostgreSQL
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets - Agregar aquí conforme se creen nuevas entidades
    public DbSet<City> Cities => Set<City>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones de entidades del ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configuraciones globales para PostgreSQL
        ConfigurePostgreSqlConventions(modelBuilder);
    }

    /// <summary>
    /// Configura convenciones específicas de PostgreSQL
    /// </summary>
    private void ConfigurePostgreSqlConventions(ModelBuilder modelBuilder)
    {
        // PostgreSQL usa snake_case por convención
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Convertir nombres de tablas a snake_case
            entity.SetTableName(ToSnakeCase(entity.GetTableName() ?? entity.DisplayName()));

            // Convertir nombres de columnas a snake_case
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            // Convertir nombres de claves a snake_case
            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName() ?? $"pk_{entity.GetTableName()}"));
            }

            // Convertir nombres de índices a snake_case
            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? $"ix_{entity.GetTableName()}"));
            }

            // Convertir nombres de claves foráneas a snake_case
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName() ?? $"fk_{entity.GetTableName()}"));
            }
        }
    }

    /// <summary>
    /// Convierte un string de PascalCase a snake_case
    /// </summary>
    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new System.Text.StringBuilder();
        result.Append(char.ToLowerInvariant(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                result.Append('_');
                result.Append(char.ToLowerInvariant(input[i]));
            }
            else
            {
                result.Append(input[i]);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Sobrescribe SaveChanges para agregar auditoría automática si es necesario
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Aquí puedes agregar lógica de auditoría automática
        // Por ejemplo, establecer CreatedAt, UpdatedAt, etc.
        
        return base.SaveChangesAsync(cancellationToken);
    }
}
