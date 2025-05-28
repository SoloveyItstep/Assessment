using log4net;
using Moq;
using Session.Application.Repositories;
using Session.Services.Services;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Session.UnitTests.Services;
[ExcludeFromCodeCoverage]
public class HealthcheckSqlServiceTests
{
    [Fact]
    public async Task DatabaseCheck_ShouldReturnConnectionStatus()
    {
        // Arrange
        var repositoryMock = new Mock<IHealthcheckSqlRepository>();
        var loggerMock = new Mock<ILog>();
        var service = new HealthcheckSqlService(repositoryMock.Object, loggerMock.Object);
        
        repositoryMock.Setup(repo => repo.DatabaseCheck()).ReturnsAsync(true);
        // Act
        var result = await service.DatabaseCheck();
        // Assert
        Assert.Equal("Database connection status: True", result);
        loggerMock.Verify(logger => logger.Info(It.IsAny<string>()), Times.Once);
    }
}
