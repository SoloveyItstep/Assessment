namespace SessionMVC.Services;

public interface IQueueService
{
    Task SendMessage(string message);

    Task<List<string>> GetMessages();
}
