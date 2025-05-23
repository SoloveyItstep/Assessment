using SessionMVC.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Session.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

//var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
//var dbName = Environment.GetEnvironmentVariable("DB_NAME");
//var dbPass = Environment.GetEnvironmentVariable("DB_SA_PASSWORD");

//var connectionString = $"Data Source={dbHost};Initial Catalog={dbName}; Integrated Security=True;Encrypt=False;Password={dbPass}";
//"Data Source=VSOLOVEI-NOUT2;Initial Catalog=Assessment; Integrated Security=True;Encrypt=False";
// builder.Configuration.GetConnectionString("AssessmentDbConnectionString");
//var MongoDbUri = Environment.GetEnvironmentVariable("MongoConnectionString");

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(System.Net.IPAddress.Any, 5000, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });

    serverOptions.Listen(System.Net.IPAddress.Any, 80, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });

    serverOptions.Listen(System.Net.IPAddress.Any, 443, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps(); // Якщо у вас є сертифікат
    });

    serverOptions.Listen(System.Net.IPAddress.Any, 8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});



builder.Services.AddRazorPages();

//builder.WebHost.UseUrls("http://*:5000;https://*:5001;http://*:80;https://*:443;http://*:8080");
var connectionString = builder.Configuration.GetConnectionString("AssessmentDbConnectionString") ?? string.Empty;
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoConnectionString") ?? string.Empty;

Console.WriteLine($"Connection string: {connectionString}");
builder.Services.AddSessionServices(connectionString, mongoConnectionString);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



builder.Services.AddSwaggerGen();

//builder.Services.AddLogging(options =>
//{
//    XmlConfigurator.Configure(new FileInfo("log4net.config"));
//    options.Services.AddSingleton(LogManager.GetLogger(typeof(Program)));
//});

foreach (var envVar in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>())
{
    Console.WriteLine($"Key: {envVar.Key}, Value: {envVar.Value}");
}

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapHealthChecks("/health"); 

//app.UseHttpsRedirection();
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

await builder.Services.ValidateDatabases();

app.MapRazorPages();

await app.RunAsync();
