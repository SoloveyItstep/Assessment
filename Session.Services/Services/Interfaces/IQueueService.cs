namespace Session.Services.Services.Interfaces;

public interface IQueueService
{
    Task SendMessage(string message);

    Task<List<string>> GetMessages();
}
