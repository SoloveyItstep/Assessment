using AutoMapper;
using Session.Application.Repositories;
using Session.Domain.Models;
using Session.Services.Models.DTOs;
using Session.Services.Resolvers;
using Session.Services.Services.Interfaces;

namespace Session.Services.Services;

public class ForecastMongoService(WeatherResolvers resolver, IMapper mapper) : IForecastService
{
    readonly IWeatherRepository repository = resolver("mongo");
    readonly IMapper mapper = mapper;

    public WeatherForecastDto GetForecast()
    {
        DateOnly date = DateOnly.FromDateTime(DateTime.Now);
        var forecast = repository.GetForecastByDate(date);

        if (forecast == null)
        {
            var summaries = repository.GetSummaries();
            forecast = new WeatherForecast
            {
                Date = date,
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Count)].State
            };
            repository.CreateForecast(forecast);
        }

        return mapper.Map<WeatherForecastDto>(forecast);
    }
}
