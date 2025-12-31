using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports;

/// <summary>
/// Puerto de salida (Output Port) - Define el contrato para obtener datos de pronósticos
/// Las implementaciones de esta interfaz pertenecen a la capa de Infraestructura
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


