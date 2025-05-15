using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Session.Domain.Models.Mongo;
using Session.Domain.Models.SQL;
using Session.Persistence.Contexts;

namespace Session.Persistence.Helpers;

public static class DBHelper
{
    public static void InitDB(AssessmentDbContext context)
    {
        context.Database.Migrate();

        if (!context.Summarys.Any())
        {
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy",
                "Hot", "Sweltering", "Scorching"
            };
            
            int i = 0;
            foreach (var summary in summaries)
            {
                context.Summarys.Add(new SummarySql(summary));
            }

            context.SaveChanges();
        }
    }

    public static void InitMongoDb(IMongoDatabase database)
    {
        var collection = database.GetCollection<SummaryMongoDB>("Summarys");
        if (collection.CountDocuments(FilterDefinition<SummaryMongoDB>.Empty) == 0)
        {
            var summaries = new[]
            {
                new SummaryMongoDB("Bracing", 0),
                new SummaryMongoDB("Cool", 1),
                new SummaryMongoDB("Freezing", 2),
                new SummaryMongoDB("Chilly", 3),
                new SummaryMongoDB("Mild", 4),
                new SummaryMongoDB("Warm", 5),
                new SummaryMongoDB("Balmy", 6),
                new SummaryMongoDB("Hot", 7),
                new SummaryMongoDB("Sweltering", 8),
                new SummaryMongoDB("Scorching", 9)
            };
            collection.InsertMany(summaries);
        }
    }
}
