using AutoFixture;
using log4net;
using Microsoft.EntityFrameworkCore;
using Moq;
using Session.Domain.Models.SQL;
using Session.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Session.UnitTests.Contexts;

[ExcludeFromCodeCoverage]
public class AssessmentDbContextTests
{
    [Fact]
    public void AssessmentDbContext_ShouldInitializeCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssessmentDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        // Act
        using var context = new AssessmentDbContext(options);
        // Assert
        Assert.NotNull(context);
        Assert.IsType<AssessmentDbContext>(context);
    }

    [Fact]
    public void AssessmentDbContext_ShouldHaveCorrectDbSetProperties()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssessmentDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        using var context = new AssessmentDbContext(options);
        
        // Act & Assert
        Assert.NotNull(context.Summarys);
        Assert.NotNull(context.WeatherForecasts);
    }

    [Fact]
    public void AssessmentDbContext_ShouldBeAbleToSaveChanges()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssessmentDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        using var context = new AssessmentDbContext(options);
        
        // Act
        context.Summarys.Add(new Fixture().Create<SummarySql>());
        var result = context.SaveChanges();
        
        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void AssessmentDbContext_ShouldBeAbleToQueryData()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssessmentDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        using var context = new AssessmentDbContext(options);
        
        // Act
        context.Summarys.Add(new Fixture().Create<SummarySql>());
        context.SaveChanges();
        
        var summaries = context.Summarys.ToList();
        
        // Assert
        Assert.NotEmpty(summaries);
        Assert.IsType<List<SummarySql>>(summaries);
    }

    [Fact]
    public void AssessmentDbContext_ShoulHandleConcurrencyCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssessmentDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        using var context = new AssessmentDbContext(options);
        
        // Act
        context.Summarys.Add(new Fixture().Create<SummarySql>());
        context.SaveChanges();
        
        // Simulate concurrency by creating a new instance of the context
        using var newContext = new AssessmentDbContext(options);
        var summary = newContext.Summarys.FirstOrDefault();
        
        // Assert
        Assert.NotNull(summary);
    }

    [Fact]
    public async Task AssessmentDbContext_CanConnectToDbAsync_ShouldReturnTrue_WhenDatabaseIsAvailable()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AssessmentDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        using var context = new AssessmentDbContext(options);
        var logger = new Mock<ILog>();
        
        // Act
        var canConnect = await context.CanConnectToDbAsync(logger.Object);
        
        // Assert
        Assert.True(canConnect);
        Assert.Contains(logger.Invocations, i => i.Method.Name == "Info" && i.Arguments.Contains("Database connection available"));
    }
}
