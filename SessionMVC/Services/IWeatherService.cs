using SessionMVC.Models.DTOs;

namespace SessionMVC.Services;

/// <summary>
/// Weather service
/// </summary>
public interface IWeatherService
{
    WeatherForecastDto GetForecast();
}
