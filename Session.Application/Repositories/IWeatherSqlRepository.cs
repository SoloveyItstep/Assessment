using Session.Domain.Models.SQL;

namespace Session.Application.Repositories;

public interface IWeatherSqlRepository
{
    public IQueryable<SummarySql> GetSummaries();

    public Task<WeatherForecastSql?> GetForecastByDate(DateOnly date);

    public Task<int> CreateForecast(WeatherForecastSql forecast);
}
