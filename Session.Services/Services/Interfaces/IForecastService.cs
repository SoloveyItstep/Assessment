using Session.Services.Models.DTOs;

namespace Session.Services.Services.Interfaces;

public interface IForecastService
{
    WeatherForecastDto GetForecast();
}
