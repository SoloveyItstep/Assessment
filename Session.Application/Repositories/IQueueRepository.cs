namespace Session.Application.Repositories;

public interface IQueueRepository
{
    Task SendMessage(string message);

    Task<List<string>> GetMessages();
}
