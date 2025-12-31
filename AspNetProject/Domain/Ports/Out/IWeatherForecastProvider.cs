using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Puerto de SALIDA (Outbound Port) - Define el contrato para obtener datos de pronósticos
/// Este puerto es implementado por la capa de Infrastructure (adaptadores de salida)
/// y usado por la capa de Application
/// </summary>
public interface IWeatherForecastProvider
{
    /// <summary>
    /// Obtiene pronósticos del tiempo para un número específico de días
    /// </summary>
    /// <param name="days">Número de días a pronosticar</param>
    /// <returns>Colección de pronósticos</returns>
    IEnumerable<WeatherForecast> GetForecasts(int days);
}
