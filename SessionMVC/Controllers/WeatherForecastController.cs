using Microsoft.AspNetCore.Mvc;
using Session.Services.Models.DTOs;
using Session.Services.Services.Interfaces;

namespace SessionMVC.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(IWeatherMongoService service) : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<WeatherForecastDto> Get()
    {
        return await service.GetForecast();
    }
}
