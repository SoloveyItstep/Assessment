using log4net;
using Session.Application.Repositories;
using Session.Services.Services.Interfaces;

namespace Session.Services.Services;

public class HealthcheckSqlService(IHealthcheckSqlRepository repository, ILog logger) : IHealthcheckSqlService
{
    public async Task<string> DatabaseCheck()
    {
        var status = await repository.DatabaseCheck();
        var message = $"Database connection status: {status}";
        logger.Info(message);

        return message;
    }
}
