using AutoMapper;
using Session.Domain.Models.Mongo;
using Session.Domain.Models.SQL;
using Session.Services.Models.DTOs;

namespace Session.Services.Mapping;

public class ForecastMappingProfile: Profile
{
    public ForecastMappingProfile()
    {
        CreateMap<WeatherForecastMongoDB, WeatherForecastDto>().ReverseMap();
        CreateMap<WeatherForecastSql, WeatherForecastDto>().ReverseMap();
    }
}
