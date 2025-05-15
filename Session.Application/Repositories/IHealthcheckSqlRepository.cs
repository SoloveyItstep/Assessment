namespace Session.Application.Repositories;

public interface IHealthcheckSqlRepository
{
    public Task<bool> DatabaseCheck();
}
