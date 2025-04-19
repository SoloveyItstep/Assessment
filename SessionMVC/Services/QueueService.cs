using log4net;
using RabbitMQ.Client;
using RabbitMQ.Stream.Client;
using RabbitMQ.Stream.Client.Reliable;
using System.Text;
using System.Text.Json;

namespace SessionMVC.Services;

public class QueueService(ILog logger) : IQueueService
{
    private const string RoutingKey = "test_queue";
    private const string Exchange = "DirectExchange";

    public async Task<List<string>> GetMessages()
    {
        var messages = new List<string>();
        try
        {
            var factory = new ConnectionFactory() { HostName = "host.docker.internal" };
            using var connection = await factory.CreateConnectionAsync();
            using var chanel = await connection.CreateChannelAsync();

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
            //var config = new StreamSystemConfig
            //{
            //    UserName = "guest",
            //    Password = "guest",
            //    VirtualHost = "/",
            //    HostName = "localhost",
            //};
            //var streamSystem = await StreamSystem.Create(config);

            //var stream = "stream-offset-tracking-dotnet";
            //await streamSystem.CreateStream(new StreamSpec(stream));
            //var messageCount = 100;
            //var confirmedCde = new CountdownEvent(messageCount);
            //var producer = await Producer.Create(new ProducerConfig(streamSystem, stream)
            //{
            //    ConfirmationHandler = async confirmation => {
            //        if (confirmation.Status == ConfirmationStatus.Confirmed)
            //        {
            //            confirmedCde.Signal();
            //        }
            //        await Task.CompletedTask.ConfigureAwait(false);
            //    }
            //});

            //await producer.Send(new Message(Encoding.UTF8.GetBytes(message)));


            //confirmedCde.Wait();
            //await producer.Close();
            //await streamSystem.Close();

            var factory = new ConnectionFactory() { HostName = "host.docker.internal" };
            using var connection = await factory.CreateConnectionAsync();
            using var chanel = await connection.CreateChannelAsync();

            await chanel.QueueDeclareAsync(RoutingKey, false, false, false, null);

            var body = Encoding.UTF8.GetBytes(message);

            await chanel.BasicPublishAsync(exchange: string.Empty, routingKey: RoutingKey, body: body);
            //await chanel.QueueDeclareAsync(RoutingKey, true, false, false, null);

            //var props = new BasicProperties
            //{
            //    ContentType = "text/plain",
            //    DeliveryMode = DeliveryModes.Persistent
            //};
            //await chanel.QueueDeclareAsync(QueueName, true, false, false, null);
            //await chanel.BasicPublishAsync(Exchange, QueueName, true, props, body);
        }
        catch (Exception ex)
        {
            logger.Error("Error sending message to queue", ex);
        }
    }
}
