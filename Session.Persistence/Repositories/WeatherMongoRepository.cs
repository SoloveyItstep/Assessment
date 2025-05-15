using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Session.Application.Repositories;
using Session.Domain.Models.Mongo;

namespace Session.Persistence.Repositories;
public class WeatherMongoRepository(IMongoDatabase mongoDatabase, ILogger<WeatherMongoRepository> logger) : IWeatherMongoRepository
{
    public int CreateForecast(WeatherForecastMongoDB forecast)
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

    public async Task<WeatherForecastMongoDB?> GetForecastByDate(DateOnly date)
    {
        try
        {
            var collection = mongoDatabase.GetCollection<WeatherForecastMongoDB>("WeatherForecasts");
            var filter = Builders<WeatherForecastMongoDB>.Filter.Eq("Date", date);
            var filterResult = await collection.FindAsync(filter);
            
            return await filterResult.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error");
        }

        return null;
    }

    public async Task<List<SummaryMongoDB>> GetSummaries()
    {
        try
        {
            var collection = mongoDatabase.GetCollection<SummaryMongoDB>("Summarys");
            var filter = new BsonDocument();
            var sort = Builders<SummaryMongoDB>.Sort.Ascending("state");
            var options = new FindOptions<SummaryMongoDB>
            {
                Sort = sort
            };
            var summariesCursor = await collection.FindAsync(filter, options);

            return await summariesCursor.ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error message");
        }

        return [];
    }
}
