using AutoMapper;
using Session.Application.Repositories;
using Session.Domain.Models.Mongo;
using Session.Services.Models.DTOs;
using Session.Services.Services.Interfaces;

namespace Session.Services.Services;

public class WeatherMongoService(IWeatherMongoRepository repository, IMapper mapper) : IWeatherMongoService
{
    readonly IMapper mapper = mapper;

    public async Task<WeatherForecastDto> GetForecast()
    {
        DateOnly date = DateOnly.FromDateTime(DateTime.Now);
        var forecast = await repository.GetForecastByDate(date);

        if (forecast == null)
        {
            var summaries = await repository.GetSummaries();
            forecast = new WeatherForecastMongoDB
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
