using AutoMapper;
using SessionMVC.Models;
using SessionMVC.Models.DTOs;
using SessionMVC.Repositories;

namespace SessionMVC.Services;

/// <summary>
/// service
/// </summary>
public class WeatherService(IWeatherRepository repo, IMapper mapper) : IWeatherService
{


    public WeatherForecastDto GetForecast()
    {
        DateOnly date = DateOnly.FromDateTime(DateTime.Now);
        var forecast = repo.GetForecastByDate(date);

        if(forecast == null)
        {
            var summaries = repo.GetSummaries();
            forecast = new WeatherForecast
            {
                Date = date,
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Count)].State
            };

            repo.CreateForecast(forecast);
        }

        return mapper.Map<WeatherForecastDto>(forecast);
    }
}
