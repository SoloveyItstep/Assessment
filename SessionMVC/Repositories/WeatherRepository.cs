using SessionMVC.Context;
using SessionMVC.Models;

namespace SessionMVC.Repositories;

/// <summary>
/// weather repo
/// </summary>
public class WeatherRepository(AssessmentDbContext context, ILogger<WeatherRepository> logger) : IWeatherRepository
{
    public int CreateForecast(WeatherForecast forecast)
    {
        try
        {
            context.WeatherForecasts.Add(forecast);

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
            return context.WeatherForecasts.SingleOrDefault(x => x.Date == date);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error");
        }

        return null;
    }

    public List<Summary> GetSummaries()
    {
        List<Summary> result = [];
        try
        {
            result = [.. context.Summarys];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error message");
        }

        return result;
    }
}
