using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using Moq;
using Session.Services.Services.Interfaces;
using SessionMVC.Controllers;
using SessionMVC.Models;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Session.UnitTests.Controllers;

[ExcludeFromCodeCoverage]
public class HomeControllerTests
{
    private readonly Mock<IHealthcheckSqlService> _healthcheckSqlServiceMock;
    private readonly HomeController _homeController;

    public HomeControllerTests()
    {
        _healthcheckSqlServiceMock = new Mock<IHealthcheckSqlService>();
        _homeController = new HomeController(_healthcheckSqlServiceMock.Object);
    }

    [Fact]
    public void Index_ShouldReturnViewResult()
    {
        // Arrange

        // Act
        var result = _homeController.Index();
        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Queue_ShouldReturnViewResult()
    {
        // Arrange

        // Act
        var result = _homeController.Queue();
        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_ShouldReturnOkResultWithSessionId()
    {
        // Arrange  
        var mockSession = new Mock<ISession>();
        mockSession.Setup(s => s.Id).Returns("test-session-id");

        var context = new DefaultHttpContext
        {
            Session = mockSession.Object
        };
        _homeController.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };

        // Act  
        var result = _homeController.Privacy();

        // Assert  
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("{ id = test-session-id }", okResult.Value?.ToString());
    }

    [Fact]
    public async Task DbCheck_ShouldReturnOkResultWithDatabaseStatus()
    {
        // Arrange
        _healthcheckSqlServiceMock.Setup(s => s.DatabaseCheck()).ReturnsAsync("Database connection status: True");
        // Act
        var result = await _homeController.DbCheck();
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("{ status = Database connection status: True }", okResult.Value?.ToString());
    }

    [Fact]
    public void Error_ShouldReturnViewResultWithErrorViewModel()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "test-trace-id"
        };
        _homeController.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };
        // Act
        var result = _homeController.Error();
        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
        Assert.Equal("test-trace-id", model.RequestId);
    }
}
