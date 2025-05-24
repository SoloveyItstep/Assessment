using SessionMVC.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Session.Services.Extensions;
using log4net;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Session.Services.Middleware;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    // Слухаємо HTTP на порту, вказаному в ASPNETCORE_HTTP_PORTS (за замовчуванням 5000 з Dockerfile)
//    // або на порту 5000, якщо змінна не встановлена.
//    // Program.cs також намагається слухати на 80 та 8080, але ENV ASPNETCORE_HTTP_PORTS=5000 
//    // у Dockerfile має бути пріоритетним для HTTP.
//    // Для простоти, ми покладатимемося на ENV ASPNETCORE_HTTP_PORTS для HTTP.

//    // Налаштовуємо Kestrel слухати на портах, визначених змінними середовища,
//    // або на стандартних портах, якщо змінні не встановлені.
//    // Змінна ASPNETCORE_HTTP_PORTS=5000 встановлена у вашому Dockerfile.
    
//    // Ви можете залишити явне прослуховування портів, якщо це потрібно для специфічних сценаріїв,
//    // але переконайтеся, що вони не конфліктують і що HTTPS налаштовується умовно.

//    // serverOptions.Listen(System.Net.IPAddress.Any, 5000, listenOptions =>
//    // {
//    //     listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
//    // });

//    // serverOptions.Listen(System.Net.IPAddress.Any, 80, listenOptions =>
//    // {
//    //     listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
//    // });

//    if (builder.Environment.IsDevelopment())
//    {
//        // У середовищі розробки (локально з VS) можна спробувати налаштувати HTTPS,
//        // але в контейнері для CI/CD це може викликати проблеми без сертифіката.
//        // Якщо ASPNETCORE_ENVIRONMENT=Development встановлено для контейнера,
//        // і немає сертифіката, цей блок все одно може викликати помилку.
//        // Краще не намагатися налаштувати HTTPS в контейнері, якщо немає сертифіката.
//        Console.WriteLine("Development environment detected. Skipping explicit HTTPS Kestrel configuration if no cert is present.");
//    }
//    else // Для Production або інших середовищ, де HTTPS має бути налаштований
//    {
//        serverOptions.Listen(System.Net.IPAddress.Any, 443, listenOptions =>
//        {
//            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
//            // Цей рядок вимагає наявності сертифіката.
//            // Для CI/CD, де ASPNETCORE_ENVIRONMENT=Development, ми не хочемо, щоб це виконувалося,
//            // якщо тільки ми не надаємо сертифікат контейнеру.
//            // Оскільки ASPNETCORE_HTTP_PORTS=5000 встановлено, Kestrel має слухати на HTTP.
//            // Якщо ви хочете HTTPS, вам потрібно буде налаштувати сертифікати.
//            // Для простоти, ми не будемо викликати UseHttps() тут, якщо середовище Development.
//            // listenOptions.UseHttps(); 
//        });
//    }
    
//    // Ваш Dockerfile встановлює ENV ASPNETCORE_HTTP_PORTS=5000,
//    // тому Kestrel автоматично слухатиме на цьому HTTP порту.
//    // Явне прослуховування тут може бути зайвим або конфліктувати.
//    // serverOptions.Listen(System.Net.IPAddress.Any, 8080, listenOptions =>
//    // {
//    //    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
//    // });
//});

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

// app.UseHttpsRedirection(); // Закоментуйте це для середовищ без налаштованого HTTPS
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

// Перемістимо валідацію баз даних після app.Build(), щоб переконатися, що сервіси зареєстровані
// і щоб уникнути проблем з порядком запуску, якщо бази даних ще не готові.
// Краще, щоб додаток запустився, а потім health checks перевірили бази.
// Для CI/CD, де бази можуть запускатися паралельно, це може бути проблемою при старті.
// Розгляньте можливість зробити цю валідацию менш блокуючою або умовною.
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
    // Вирішіть, чи має додаток падати, якщо валідація не вдалася.
    // Для CI це може бути бажано, щоб побачити проблему.
    // throw; 
}


app.MapRazorPages();

await app.RunAsync();
