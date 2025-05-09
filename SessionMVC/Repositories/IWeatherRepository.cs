using SessionMVC.Models;

namespace SessionMVC.Repositories;

/// <summary>
/// weather repo
/// </summary>
public interface IWeatherRepository
{
    public List<Summary> GetSummaries();

    public WeatherForecast? GetForecastByDate(DateOnly date);

    public int CreateForecast(WeatherForecast forecast);

    public List<SummaryMongoDB> GetSummariesMongo();

    public WeatherForecastMongoDB? GetForecastMongoByDate(DateOnly date);

    public int CreateForecastMongo(WeatherForecastMongoDB forecast);
}
