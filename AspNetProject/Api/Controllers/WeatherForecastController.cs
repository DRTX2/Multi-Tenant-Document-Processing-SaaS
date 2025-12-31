using Microsoft.AspNetCore.Mvc;
using AspNetProject.Domain.Ports;
using AspNetProject.Domain.Models;

namespace AspNetProject.Api.Controllers;

/// <summary>
/// Controlador REST para pronósticos del tiempo
/// Esta clase pertenece a la capa de Adaptadores (Infrastructure/API)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastService _service;
    
    public WeatherForecastController(IWeatherForecastService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene pronósticos del tiempo para los próximos días
    /// </summary>
    /// <param name="days">Número de días (1-30)</param>
    /// <returns>Lista de pronósticos</returns>
    [HttpGet(Name = "GetWeatherForecast")]
    [ProducesResponseType(typeof(IEnumerable<WeatherForecast>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<WeatherForecast>> Get([FromQuery] int days = 5)
    {
        try
        {
            var forecasts = _service.GetForecasts(days);
            return Ok(forecasts);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

