using Session.Domain.Models;

namespace Session.Application.Repositories;

public interface IWeatherRepository
{
    public List<Summary> GetSummaries();

    public WeatherForecast? GetForecastByDate(DateOnly date);

    public int CreateForecast(WeatherForecast forecast);
}

