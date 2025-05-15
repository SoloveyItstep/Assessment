using Microsoft.AspNetCore.Mvc;
using Session.Services.Models.DTOs;
using Session.Services.Services.Interfaces;

namespace SessionMVC.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherMongoController(IWeatherMongoService service) : ControllerBase
{
    [HttpGet(Name = "GetWeatherMongoForecast")]
    public Task<WeatherForecastDto> Get()
    {
        return service.GetForecast();
    }
}
