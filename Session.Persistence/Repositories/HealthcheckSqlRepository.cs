using Session.Application.Repositories;
using Session.Persistence.Contexts;

namespace Session.Persistence.Repositories;
public class HealthcheckSqlRepository(AssessmentDbContext context) : IHealthcheckSqlRepository
{
    public async Task<bool> DatabaseCheck()
    {
        return await context.Database.CanConnectAsync();
    }
}
