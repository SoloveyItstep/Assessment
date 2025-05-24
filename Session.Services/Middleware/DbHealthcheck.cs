using log4net;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Session.Domain.Models.Mongo;
using Session.Persistence.Contexts;
using Session.Persistence.Helpers;

namespace Session.Services.Middleware;
public static class DbHealthcheck
{
    private static bool SqlDbChecked = false;
    private static bool MongoDbChecked = false;

    public static async Task CheckSqlDbHealthAsync(IServiceScope scope, ILog logger)
    {
        Console.WriteLine("Checking database health...");

        if (!SqlDbChecked)
        {

            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AssessmentDbContext>();
                DBHelper.InitDB(dbContext);
                SqlDbChecked = await dbContext.CanConnectToDbAsync(logger);
            }
            catch (Exception ex)
            {
                logger.Error("========== Database connection failed", ex);
            }
            finally
            {
                SqlDbChecked = true;
            }
        }
    }

    public static async Task CheckMongoDbHealthAsync(IServiceScope scope, ILog logger)
    {
        Console.WriteLine("Checking MongoDB health...");

        if (!MongoDbChecked)
        {
            try
            {

                var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
                DBHelper.InitMongoDb(database);
                var collection = database.GetCollection<WeatherForecastMongoDB>("WeatherForecasts");
                var filter = Builders<WeatherForecastMongoDB>.Filter.Empty;
                _ = await collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.Error("========== MongoDB connection failed", ex);
            }
            finally
            {
                MongoDbChecked = true;
            }
        }
    }
}
