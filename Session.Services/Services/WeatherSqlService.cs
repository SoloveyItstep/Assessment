using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Session.Application.Repositories;
using Session.Domain.Models.SQL;
using Session.Services.Models.DTOs;
using Session.Services.Services.Interfaces;

namespace Session.Services.Services;

public class WeatherSqlService(IWeatherSqlRepository repository, IMapper mapper) : IWeatherSqlService
{
    readonly IMapper mapper = mapper;

    public async Task<WeatherForecastDto> GetForecast()
    {
        DateOnly date = DateOnly.FromDateTime(DateTime.Now);
        var forecast = await repository.GetForecastByDate(date);

        if (forecast == null)
        {
            var summaries = await repository.GetSummaries().ToListAsync();
            forecast = new WeatherForecastSql
            {
                Date = date,
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Count)].State
            };
            await repository.CreateForecast(forecast);
        }

        return mapper.Map<WeatherForecastDto>(forecast);
    }
}
