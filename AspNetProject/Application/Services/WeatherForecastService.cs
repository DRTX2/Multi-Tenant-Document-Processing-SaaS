using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.In;
using AspNetProject.Domain.Ports.Out;

namespace AspNetProject.Application.Services;

/// <summary>
/// Implementación del caso de uso de pronósticos del tiempo
/// Esta clase pertenece a la capa de Aplicación y orquesta la lógica de negocio
/// </summary>
public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastProvider _provider;
    
    public WeatherForecastService(IWeatherForecastProvider provider)
    {
        _provider = provider;
    }

    public IEnumerable<WeatherForecast> GetForecasts(int days)
    {
        // Validación de negocio
        if (days <= 0)
        {
            throw new ArgumentException("El número de días debe ser mayor a cero", nameof(days));
        }
        
        if (days > 30)
        {
            throw new ArgumentException("No se pueden obtener pronósticos para más de 30 días", nameof(days));
        }
        
        return _provider.GetForecasts(days);
    }
}

