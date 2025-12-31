namespace AspNetProject.Domain.Models;

/// <summary>
/// Entidad de dominio que representa un pronóstico del tiempo
/// Esta clase pertenece al núcleo del dominio (Domain Core)
/// </summary>
public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }
}


