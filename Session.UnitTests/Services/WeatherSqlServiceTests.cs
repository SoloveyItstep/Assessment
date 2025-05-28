using AutoFixture;
using AutoMapper;
using Moq;
using Session.Application.Repositories;
using Session.Domain.Models.SQL;
using Session.Services.Mapping;
using Session.Services.Services;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Session.UnitTests.Services;

[ExcludeFromCodeCoverage]
public class WeatherSqlServiceTests
{
    [Fact]
    public void WeatherSqlService_ShouldInitializeCorrectly()
    {
        // Arrange
        var repository = new Mock<IWeatherSqlRepository>();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new ForecastMappingProfile());
        });
        var mapper = config.CreateMapper();


        // Act
        var weatherSqlService = new WeatherSqlService(repository.Object, mapper);
        // Assert
        Assert.NotNull(weatherSqlService);
        Assert.IsType<WeatherSqlService>(weatherSqlService);
    }

    [Fact]
    public async Task GetForecast_ShouldReturnForecast_WhenRepositoryReturnsForecast()
    {
        // Arrange
        var repository = new Mock<IWeatherSqlRepository>();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new ForecastMappingProfile());
        });
        var mapper = config.CreateMapper();
        var expectedForecast = new WeatherForecastSql
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = 25,
            Summary = "Warm"
        };
        repository.Setup(repo => repo.GetForecastByDate(It.IsAny<DateOnly>()))
            .ReturnsAsync(expectedForecast);
        var weatherSqlService = new WeatherSqlService(repository.Object, mapper);
        // Act
        var forecastDto = await weatherSqlService.GetForecast();
        // Assert
        Assert.NotNull(forecastDto);
        Assert.Equal(expectedForecast.Date, forecastDto.Date);
        Assert.Equal(expectedForecast.TemperatureC, forecastDto.TemperatureC);
        Assert.Equal(expectedForecast.Summary, forecastDto.Summary);
    }

    [Fact]
    public async Task GetForecast_ShouldThrowException_WhenRepositoryThrowsException()
    {
        // Arrange
        var repository = new Mock<IWeatherSqlRepository>();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new ForecastMappingProfile());
        });
        var mapper = config.CreateMapper();
        repository.Setup(repo => repo.GetForecastByDate(It.IsAny<DateOnly>()))
            .ThrowsAsync(new Exception("Database error"));
        
        var weatherSqlService = new WeatherSqlService(repository.Object, mapper);
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => weatherSqlService.GetForecast());
    }
}
