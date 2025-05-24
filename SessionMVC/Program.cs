using SessionMVC.Middleware;
using Microsoft.EntityFrameworkCore;
using Session.Services.Extensions;
using log4net;
using Session.Services.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("AssessmentDbConnectionString") ?? string.Empty;
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoConnectionString") ?? string.Empty;

Console.WriteLine($"Connection string: {connectionString}");
Console.WriteLine($"Mongo Connection string: {mongoConnectionString}"); // Додано для відладки
builder.Services.AddSessionServices(connectionString, mongoConnectionString);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSwaggerGen();

foreach (var envVar in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>())
{
    Console.WriteLine($"ENV_VAR - Key: {envVar.Key}, Value: {envVar.Value}");
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.MapHealthChecks("/health"); 

app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.UseSwagger();
app.UseSwaggerUI();
app.UseSession();
app.UseMiddleware<SessionLimitMiddleware>(30);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

try
{
    Console.WriteLine("Attempting to validate databases...");
    using var scope = app.Services.CreateScope();;
    var logger = scope.ServiceProvider.GetRequiredService<ILog>();

    await DbHealthcheck.CheckSqlDbHealthAsync(scope, logger);
    await DbHealthcheck.CheckMongoDbHealthAsync(scope, logger); 
    Console.WriteLine("Database validation completed (or skipped if not applicable).");
}
catch (Exception ex)
{
    Console.WriteLine($"Error during database validation: {ex.Message}");
}

app.MapRazorPages();

await app.RunAsync();
