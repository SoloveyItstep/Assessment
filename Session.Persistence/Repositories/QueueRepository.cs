using log4net;
using RabbitMQ.Client;
using Session.Application.Repositories;
using Session.Persistence.Helpers;
using System.Text;

namespace Session.Persistence.Repositories;
public class QueueRepository(ILog logger) : IQueueRepository
{
    private const string RoutingKey = "test_queue";

    public async Task<List<string>> GetMessages()
    {
        var messages = new List<string>();
        try
        {
            using var chanel = await QueueHelper.CerateChanel();

            var count = await chanel.MessageCountAsync(RoutingKey);

            for (var i = 0; i < count; i++)
            {
                var result = await chanel.BasicGetAsync(RoutingKey, true);
                if (result != null)
                {
                    var message = Encoding.UTF8.GetString(result.Body.ToArray());
                    messages.Add(message);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("Error getting messages from queue", ex);
        }

        return messages;
    }

    public async Task SendMessage(string message)
    {
        try
        {
            using var chanel = await QueueHelper.CerateChanel();
            await chanel.QueueDeclareAsync(RoutingKey, false, false, false, null);
            var body = Encoding.UTF8.GetBytes(message);
            await chanel.BasicPublishAsync(exchange: string.Empty, routingKey: RoutingKey, body: body);
        }
        catch (Exception ex)
        {
            logger.Error("Error sending message to queue", ex);
        }
    }
}
