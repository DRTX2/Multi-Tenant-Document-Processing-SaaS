using AspNetProject.Application.Services;
using AspNetProject.Domain.Ports.In;
using AspNetProject.Domain.Ports.Out;
using AspNetProject.Infrastructure.Providers;
using AspNetProject.Infrastructure.Data;
using AspNetProject.Infrastructure.Repositories;
using AspNetProject.Domain.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Weather Forecast API",
        Version = "v1",
        Description = "API de pronósticos del tiempo - Arquitectura Hexagonal"
    });
});

// Configuración de Entity Framework Core con PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=aspnetproject;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
        npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
    });
    
    // Habilitar logging sensible solo en desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Configuración de Arquitectura Hexagonal

// Repositorios genéricos - Reutilizables para cualquier entidad
builder.Services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));

// Repositorios específicos - Solo si necesitas métodos adicionales
builder.Services.AddScoped<CityRepository>();

// Puerto de salida (Adaptador de Infraestructura)
builder.Services.AddScoped<IWeatherForecastProvider, RandomWeatherForecastProvider>();

// Puerto de entrada (Caso de Uso)
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather Forecast API v1");
        options.RoutePrefix = string.Empty; // Swagger UI en la raíz
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();