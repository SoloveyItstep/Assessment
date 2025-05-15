using RabbitMQ.Client;

namespace Session.Persistence.Helpers;
internal class QueueHelper
{
    private const string HostName = "host.docker.internal";

    public static async Task<IChannel> CerateChanel()
    {
        var factory = new ConnectionFactory() { HostName = HostName };
        using var connection = await factory.CreateConnectionAsync();
        var chanel = await connection.CreateChannelAsync();

        return chanel;
    }
}
