using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Session.Application.Repositories;
using Session.Domain.Models;
using Session.Domain.Models.SQL;
using Session.Persistence.Contexts;

namespace Session.Persistence.Repositories;

/// <summary>
/// weather repo
/// </summary>
public class WeatherSqlRepository(
    AssessmentDbContext context,
    IMapper mapper,
    ILogger<WeatherSqlRepository> logger) : IWeatherRepository
{
    public int CreateForecast(WeatherForecast forecast)
    {
        try
        {
            var model = mapper.Map<WeatherForecastSql>(forecast);
            context.WeatherForecasts.Add(model);

            return context.SaveChanges();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "create error");
        }

        return 0;
    }

    public WeatherForecast? GetForecastByDate(DateOnly date)
    {
        try
        {
            var forecast = context.WeatherForecasts.SingleOrDefault(x => x.Date == date);

            return mapper.Map<WeatherForecast>(forecast);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error");
        }

        return null;
    }

    public List<Summary> GetSummaries()
    {
        try
        {
            var summaries = mapper.Map<List<Summary>>(context.Summarys);

            return [.. summaries];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error message");
        }

        return [];
    }
}
