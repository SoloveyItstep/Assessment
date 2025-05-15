using AutoMapper;
using Session.Domain.Models;
using Session.Domain.Models.Mongo;
using Session.Domain.Models.SQL;
using Session.Services.Models.DTOs;

namespace Session.Persistence.Mapping;

public class ForecastMappingProfile: Profile
{
    public ForecastMappingProfile()
    {
        CreateMap<WeatherForecast, WeatherForecastSql>().ReverseMap();
        CreateMap<Summary, SummarySql>().ReverseMap();
        CreateMap<WeatherForecast, WeatherForecastMongoDB>().ReverseMap();
        CreateMap<Summary, SummaryMongoDB>().ReverseMap();
        CreateMap<WeatherForecast, WeatherForecastDto>().ReverseMap();
    }
}
