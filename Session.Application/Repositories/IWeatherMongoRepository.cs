using Session.Domain.Models;
using Session.Domain.Models.Mongo;

namespace Session.Application.Repositories;

public interface IWeatherMongoRepository
{
    public Task<List<SummaryMongoDB>> GetSummaries();

    public Task<WeatherForecastMongoDB?> GetForecastByDate(DateOnly date);

    public int CreateForecast(WeatherForecastMongoDB forecast);
}

