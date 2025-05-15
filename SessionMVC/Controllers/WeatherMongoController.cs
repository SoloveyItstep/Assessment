using Microsoft.AspNetCore.Mvc;
using Session.Services.Models.DTOs;
using Session.Services.Resolvers;
using Session.Services.Services.Interfaces;

namespace SessionMVC.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherMongoController(ForecastResolvers forecastResolvers) : ControllerBase
{
    readonly IForecastService service = forecastResolvers("mongo");

    [HttpGet(Name = "GetWeatherMongoForecast")]
    public WeatherForecastDto Get()
    {
        return service.GetForecast();
    }
}
