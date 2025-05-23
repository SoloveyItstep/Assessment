using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Session.Application.Repositories;
using Session.Persistence.Contexts;
using Session.Persistence.Repositories;
using Session.Services.Mapping;
using Session.Services.Middleware;
using Session.Services.Services;
using Session.Services.Services.Interfaces;
using SessionMVC.Middleware;

namespace Session.Services.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddSessionServices(this IServiceCollection services, string connectionString, string mongoConnectionString)
    {
        //var MongoDbUri = Environment.GetEnvironmentVariable("MongoConnectionString");
        services.AddSingleton(new MongoClient(mongoConnectionString).GetDatabase("Assessment"));

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

    public static async Task<IServiceCollection> ValidateDatabases(this IServiceCollection services)
    {
        var scope = services.BuildServiceProvider().CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILog>();

        await DbHealthcheck.CheckSqlDbHealthAsync(scope, logger);
        await DbHealthcheck.CheckMongoDbHealthAsync(scope, logger);

        return services;
    }
}
