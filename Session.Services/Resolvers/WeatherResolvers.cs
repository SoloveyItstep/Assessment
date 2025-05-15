using Session.Application.Repositories;
using Session.Services.Services.Interfaces;

namespace Session.Services.Resolvers;

public delegate IWeatherMongoRepository WeatherResolvers(string type);

public delegate IWeatherMongoService ForecastResolvers(string type);
