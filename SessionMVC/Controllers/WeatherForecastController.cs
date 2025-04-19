using Microsoft.AspNetCore.Mvc;
using SessionMVC.Models.DTOs;
using SessionMVC.Services;

namespace SessionMVC.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(IWeatherService service) : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public WeatherForecastDto Get()
    {
        return service.GetForecast();
    }
}
