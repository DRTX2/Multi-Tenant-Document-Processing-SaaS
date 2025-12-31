using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports;

/// <summary>
/// Puerto de entrada (Use Case) - Define la lógica de negocio para pronósticos del tiempo
/// </summary>
public interface IWeatherForecastService
{
    IEnumerable<WeatherForecast> GetForecasts(int days);
}
