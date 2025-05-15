using Session.Application.Repositories;
using Session.Services.Services.Interfaces;

namespace Session.Services.Services;

public class QueueService(IQueueRepository queueRepository) : IQueueService
{
    public Task<List<string>> GetMessages()
    {
        return queueRepository.GetMessages();
    }

    public Task SendMessage(string message)
    {
        return queueRepository.SendMessage(message);
    }
}
