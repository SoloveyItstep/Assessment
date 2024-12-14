using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SessionTest.Controllers;

[ApiController]
//[EnableRateLimiting("fixed-window")]
[Route("[controller]")]
public class WeatherForecastController(ILog logger) : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILog _logger = logger;

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        _logger.Debug("test log info ===================");
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("getid")]
    [ProducesResponseType(typeof(HttpResponseMessage), 200)]
    public IActionResult GetId()
    {
        return Ok(new { Id = HttpContext.Session.Id });
    }
}
