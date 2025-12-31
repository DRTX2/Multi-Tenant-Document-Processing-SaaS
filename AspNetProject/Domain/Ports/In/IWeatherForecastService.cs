using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports.In;

/// <summary>
/// Puerto de ENTRADA (Inbound Port) - Define la lógica de negocio para pronósticos del tiempo
/// Este puerto es implementado por la capa de Application y usado por adaptadores de entrada (Adapters/In)
/// </summary>
public interface IWeatherForecastService
{
    IEnumerable<WeatherForecast> GetForecasts(int days);
}
