using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Session.Services.Services.Interfaces;
using SessionMVC.Controllers;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Session.UnitTests.Controllers;

[ExcludeFromCodeCoverage]
public class QueueControllerTests
{
    [Fact]
    public async Task Get_ShouldReturnOkResult_WhenMessagesAreRetrieved()
    {
        // Arrange
        var fixture = new Fixture();
        var queueServiceMock = new Mock<IQueueService>();
        queueServiceMock.Setup(service => service.GetMessages())
            .ReturnsAsync([.. fixture.CreateMany<string>()]);
        
        var controller = new QueueController(queueServiceMock.Object);
        // Act
        var result = await controller.Get();
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var messages = Assert.IsType<IEnumerable<string>>(okResult.Value, exactMatch: false);
        Assert.Equal(3, messages.Count());
    }

    [Fact]
    public async Task Post_ShouldReturnOkResult_WhenMessageIsSent()
    {
        // Arrange
        var fixture = new Fixture();
        var message = fixture.Create<string>();
        var queueServiceMock = new Mock<IQueueService>();
        queueServiceMock.Setup(service => service.SendMessage(message))
            .Returns(Task.CompletedTask);
        
        var controller = new QueueController(queueServiceMock.Object);
        
        // Act
        var result = await controller.Post(message);
        
        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Post_ShouldReturnBadRequest_WhenMessageIsEmpty()
    {
        // Arrange
        var queueServiceMock = new Mock<IQueueService>();
        var controller = new QueueController(queueServiceMock.Object);
        
        // Act
        var result = await controller.Post(string.Empty);
        
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
