using Microsoft.EntityFrameworkCore;
using SessionMVC.Context;
using SessionMVC.Models;

namespace SessionMVC.Helpers;

public static class DBHelper
{
    public static void InitDB(AssessmentDbContext context)
    {
        context.Database.Migrate();

        if (!context.Summarys.Any())
        {
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy",
                "Hot", "Sweltering", "Scorching"
            };

            foreach (var summary in summaries)
            {
                context.Summarys.Add(new Summary(summary));
            }

            context.SaveChanges();
        }
    }
}
