using log4net;
using Microsoft.EntityFrameworkCore;
using Session.Domain.Models.SQL;

namespace Session.Persistence.Contexts;

public class AssessmentDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<SummarySql> Summarys { get; set; }
    public DbSet<WeatherForecastSql> WeatherForecasts { get; set; }


    public async Task<bool> CanConnectToDbAsync(ILog logger)
    {
        try
        {
            if (await Database.CanConnectAsync())
            {
                logger.Info("Database connection available");
                return true;
            }
            
            logger.Error("Database connection unavailable");
            return false;
        }
        catch(Exception ex) {
            logger.Error(ex);
        }

        return false;
    }
}
