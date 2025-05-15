using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Session.Application.Repositories;
using Session.Domain.Models.SQL;
using Session.Persistence.Contexts;

namespace Session.Persistence.Repositories;

/// <summary>
/// weather repo
/// </summary>
public class WeatherSqlRepository(
    AssessmentDbContext context,
    ILogger<WeatherSqlRepository> logger) : IWeatherSqlRepository
{
    public async Task<int> CreateForecast(WeatherForecastSql forecast)
    {
        try
        {
            context.WeatherForecasts.Add(forecast);

            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "create error");
        }

        return 0;
    }

    public async Task<WeatherForecastSql?> GetForecastByDate(DateOnly date)
    {
        try
        {
            return await context.WeatherForecasts.SingleOrDefaultAsync(x => x.Date == date);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error");
        }

        return null;
    }

    public IQueryable<SummarySql> GetSummaries()
    {
        try
        {
            return context.Summarys.AsQueryable();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error message");
        }

        return Array.Empty<SummarySql>().AsQueryable();
    }
}
