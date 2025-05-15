using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Session.Application.Repositories;
using Session.Domain.Models;
using Session.Domain.Models.Mongo;

namespace Session.Persistence.Repositories;
public class WeatherMongoRepository(IMongoDatabase mongoDatabase, ILogger<WeatherMongoRepository> logger, IMapper mapper) : IWeatherRepository
{
    public int CreateForecast(WeatherForecast forecast)
    {
        try
        {
            var forecastMongoDB = mapper.Map<WeatherForecastMongoDB>(forecast);
            var collection = mongoDatabase.GetCollection<WeatherForecastMongoDB>("WeatherForecasts");
            collection.InsertOne(forecastMongoDB);

            return 1;
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
            var collection = mongoDatabase.GetCollection<WeatherForecastMongoDB>("WeatherForecasts");
            var filter = Builders<WeatherForecastMongoDB>.Filter.Eq("Date", date);
            var forecast = collection.Find(filter).FirstOrDefault();
            var result = mapper.Map<WeatherForecast>(forecast);

            return result;
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
            var collection = mongoDatabase.GetCollection<SummaryMongoDB>("Summarys");
            var summaries = collection.Find(new BsonDocument()).ToList();
            var result = mapper.Map<List<Summary>>(summaries);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error message");
        }

        return [];
    }
}
