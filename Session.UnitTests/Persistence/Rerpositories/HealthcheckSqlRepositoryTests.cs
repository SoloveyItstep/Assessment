using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Session.Application.Repositories;
using Session.Persistence.Contexts;
using Session.Persistence.Repositories;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Session.UnitTests.Persistence.Rerpositories;

[ExcludeFromCodeCoverage]
public class HealthcheckSqlRepositoryTests
{
    private readonly IHealthcheckSqlRepository _healthcheckSqlRepository;
    private readonly Mock<AssessmentDbContext> _assessmentDbContextMock;

    public HealthcheckSqlRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AssessmentDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _assessmentDbContextMock = new Mock<AssessmentDbContext>(options);
        var databaseFacadeMock = new Mock<DatabaseFacade>(_assessmentDbContextMock.Object);
        databaseFacadeMock.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _assessmentDbContextMock.Setup(x => x.Database).Returns(databaseFacadeMock.Object);
        _healthcheckSqlRepository = new HealthcheckSqlRepository(_assessmentDbContextMock.Object);
    }

    [Fact]
    public async Task DatabaseCheck_ShouldReturnTrue_WhenDatabaseIsAvailable()
    {
        // Arrange
        _assessmentDbContextMock.Setup(x => x.Database.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        // Act
        var result = await _healthcheckSqlRepository.DatabaseCheck();
        // Assert
        Assert.True(result);
    }
}
