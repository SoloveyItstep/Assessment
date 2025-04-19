using log4net;
using Microsoft.EntityFrameworkCore;
using SessionMVC.Models;

namespace SessionMVC.Context;

public class AssessmentDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Summary> Summarys { get; set; }
    public DbSet<WeatherForecast> WeatherForecasts { get; set; }


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
