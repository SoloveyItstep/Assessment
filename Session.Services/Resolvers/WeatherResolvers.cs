using Session.Application.Repositories;
using Session.Services.Services.Interfaces;

namespace Session.Services.Resolvers;

public delegate IWeatherRepository WeatherResolvers(string type);

public delegate IForecastService ForecastResolvers(string type);
