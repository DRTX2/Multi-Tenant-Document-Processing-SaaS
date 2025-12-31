using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.Out;

namespace AspNetProject.Infrastructure.Providers;

/// <summary>
/// Adaptador de infraestructura que implementa el puerto de salida
/// Genera pronósticos del tiempo aleatorios para demostración
/// Esta clase pertenece a la capa de Infraestructura
/// </summary>
public class RandomWeatherForecastProvider : IWeatherForecastProvider
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public IEnumerable<WeatherForecast> GetForecasts(int days)
    {
        return Enumerable.Range(1, days).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        });
    }
}


