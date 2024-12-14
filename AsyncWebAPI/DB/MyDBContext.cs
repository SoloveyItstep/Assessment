using AsyncWebAPI.DB.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AsyncWebAPI.DB;

public class MyDBContext: DbContext
{
    public MyDBContext(DbContextOptions options)
        :base(options)
    { }
     
    public DbSet<User> Users { get; set; }

    public async Task<List<User>> UsersList()
    {
        await Task.Delay(2000);
        var users = await Users.AsNoTracking().ToListAsync();
        Debug.WriteLine($"=============================== Users method ======= Thread ID: {Environment.CurrentManagedThreadId}");

        return users;
    }

    public async Task InsertData()
    {
        for(int i = 0; i < 10000; ++i)
        {
            Users.Add(new()
            {
                Name = $"new User {i}",
                Email = $"mail{1}@mail.com"
            });
        }

        await SaveChangesAsync();
    }

    public async Task Clear()
    {
        await Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Users]");
    }
}
