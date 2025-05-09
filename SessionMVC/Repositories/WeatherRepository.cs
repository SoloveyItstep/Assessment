using MongoDB.Bson;
using MongoDB.Driver;
using SessionMVC.Context;
using SessionMVC.Models;

namespace SessionMVC.Repositories;

/// <summary>
/// weather repo
/// </summary>
public class WeatherRepository(
    AssessmentDbContext context, 
    ILogger<WeatherRepository> logger,
    IMongoDatabase mongoDatabase
    ) : IWeatherRepository
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
        try
        {
            return [.. context.Summarys];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error message");
        }

        return [];
    }

    public List<SummaryMongoDB> GetSummariesMongo()
    {
        try
        {
            var collection = mongoDatabase.GetCollection<SummaryMongoDB>("Summarys");
            var summaries = collection.Find(new BsonDocument()).ToList();
            
            return summaries;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error message");
        }
        return [];
    }

    public WeatherForecastMongoDB? GetForecastMongoByDate(DateOnly date)
    {
        try
        {
            var collection = mongoDatabase.GetCollection<WeatherForecastMongoDB>("WeatherForecasts");
            var filter = Builders<WeatherForecastMongoDB>.Filter.Eq("Date", date);
            var forecast = collection.Find(filter).FirstOrDefault();
            
            return forecast;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error");
        }

        return null;
    }

    public int CreateForecastMongo(WeatherForecastMongoDB forecast)
    {
        try
        {
            var collection = mongoDatabase.GetCollection<WeatherForecastMongoDB>("WeatherForecasts");
            collection.InsertOne(forecast);

            return 1;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "create error");
        }
        return 0;
    }
}
