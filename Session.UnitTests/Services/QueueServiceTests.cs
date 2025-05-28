using Moq;
using Session.Application.Repositories;
using Session.Services.Services;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Session.UnitTests.Services;

[ExcludeFromCodeCoverage]
public class QueueServiceTests
{
    [Fact]
    public void QueueService_ShouldInitializeCorrectly()
    {
        // Arrange
        var repositoryMock = new Mock<IQueueRepository>();
        var queueService = new QueueService(repositoryMock.Object);
        
        // Act & Assert
        Assert.NotNull(queueService);
        Assert.IsType<QueueService>(queueService);
    }

    [Fact]
    public async Task GetMessages_ShouldReturnMessages_WhenRepositoryReturnsMessages()
    {
        // Arrange
        var repositoryMock = new Mock<IQueueRepository>();
        var expectedMessages = new List<string> { "Message1", "Message2", "Message3" };
        repositoryMock.Setup(repo => repo.GetMessages())
            .ReturnsAsync(expectedMessages);
        
        var queueService = new QueueService(repositoryMock.Object);
        
        // Act
        var messages = await queueService.GetMessages();
        
        // Assert
        Assert.Equal(expectedMessages, messages);
    }

    [Fact]
    public async Task SendMessage_ShouldCallRepository_WhenMessageIsSent()
    {
        // Arrange
        var repositoryMock = new Mock<IQueueRepository>();
        var message = "TestMessage";
        
        var queueService = new QueueService(repositoryMock.Object);
        
        // Act
        await queueService.SendMessage(message);
        
        // Assert
        repositoryMock.Verify(repo => repo.SendMessage(message), Times.Once);
    }

    [Fact]
    public async Task SendMessage_ShouldThrowException_WhenMessageIsNullOrEmpty()
    {
        // Arrange
        var repositoryMock = new Mock<IQueueRepository>();
        var queueService = new QueueService(repositoryMock.Object);
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => queueService.SendMessage(string.Empty));
        await Assert.ThrowsAsync<ArgumentException>(() => queueService.SendMessage(null!));
    }
}
