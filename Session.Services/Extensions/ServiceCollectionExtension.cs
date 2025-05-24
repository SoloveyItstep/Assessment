using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Session.Application.Repositories;
using Session.Persistence.Contexts;
using Session.Persistence.Repositories;
using Session.Services.Mapping;
using Session.Services.Middleware;
using Session.Services.Services;
using Session.Services.Services.Interfaces;

namespace Session.Services.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddSessionServices(this IServiceCollection services, string connectionString, string mongoConnectionString)
    {
        services.AddSingleton<IMongoClient>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("MongoConnectionString"); // Або configuration["MongoConnectionString"]
            if (string.IsNullOrEmpty(connectionString))
            {
                // Логування або викидання виключення, якщо рядок не знайдено
                throw new InvalidOperationException("MongoConnectionString not found in configuration.");
            }
            Console.WriteLine($"Using MongoDB connection string: {connectionString}"); // Для дебагу
            return new MongoClient(connectionString);
        });

        // Або якщо ви отримуєте IMongoDatabase напряму:
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("MongoConnectionString"); // Або configuration["MongoConnectionString"]
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("MongoConnectionString not found in configuration.");
            }
            Console.WriteLine($"DEBUG: Using MongoDB connection string from config: {connectionString}");
            var client = new MongoClient(connectionString);
            var databaseName = MongoUrl.Create(connectionString).DatabaseName; // Отримати ім'я БД з рядка
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new InvalidOperationException("Database name not found in MongoConnectionString.");
            }
            return client.GetDatabase(databaseName);
        });

        services.AddDbContext<AssessmentDbContext>(options => {
            options.UseSqlServer(connectionString);
        });

        services.AddHealthChecks()
            .AddCheck<HealthcheckSample>("SomeCheck")
            .AddDbContextCheck<AssessmentDbContext>(
                customTestQuery: async (context, cancellationToken) => {
                    return await context.CanConnectToDbAsync(services.BuildServiceProvider().GetRequiredService<ILog>());
                });

        services.AddTransient<IQueueRepository, QueueRepository>();
        services.AddTransient<IQueueService, QueueService>();

        services.AddTransient<IWeatherSqlRepository, WeatherSqlRepository>();
        services.AddTransient<IWeatherSqlService, WeatherSqlService>();

        services.AddTransient<IWeatherMongoRepository, WeatherMongoRepository>();
        services.AddTransient<IWeatherMongoService, WeatherMongoService>();

        services.AddTransient<IHealthcheckSqlRepository, HealthcheckSqlRepository>();
        services.AddTransient<IHealthcheckSqlService, HealthcheckSqlService>();

        services.AddAutoMapper(typeof(ForecastMappingProfile));

        services.AddLogging(options =>
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            options.Services.AddSingleton(LogManager.GetLogger(typeof(ServiceCollectionExtension)));
        });

        return services;
    }
}
