using Session.Services.Models.DTOs;

namespace Session.Services.Services.Interfaces;

public interface IWeatherMongoService
{
    Task<WeatherForecastDto> GetForecast();
}
