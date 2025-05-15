namespace Session.Services.Services.Interfaces;

public interface IHealthcheckSqlService
{
    Task<string> DatabaseCheck();
}
