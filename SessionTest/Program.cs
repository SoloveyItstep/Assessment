using log4net.Config;
using log4net;
using Microsoft.AspNetCore.DataProtection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
        options => builder.Configuration.Bind("JwtSettings", options))
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
        options => builder.Configuration.Bind("CookieSettings", options));

builder.Services.AddCors(options =>
options.AddPolicy("AllowSetOrigins", opt =>
{
    opt.AllowAnyHeader();
    opt.AllowAnyMethod();
    opt.WithOrigins("http://localhost:4200");
    opt.SetIsOriginAllowed(orign => true);
    opt.AllowCredentials();
}));

builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".AspNetCore.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging(options =>
{
    XmlConfigurator.Configure(new FileInfo("log4net.config"));
    options.Services.AddSingleton(LogManager.GetLogger(typeof(Program)));
});

builder.Services.AddDataProtection()
    .SetDefaultKeyLifetime(TimeSpan.FromDays(14));

var rateLimitName = "some-limit";

//builder.Services.AddRateLimiter(options => {
//    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
//    //options.AddPolicy("user-limit", opt => { 
        
//    //});

//    //options.AddFixedWindowLimiter("fixed-window", opt =>
//    //{
//    //    opt.Window = TimeSpan.FromSeconds(5);
//    //    opt.PermitLimit = 5;
//    //    opt.QueueLimit = 10;
//    //    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//    //});
//});



builder.Services.AddRateLimiter(options => {
    options.OnRejected = (context, cancelationTocken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }
        var userEndpoint = GetUserEndPoint(context.HttpContext);
        var logger = context.HttpContext.RequestServices.GetService<ILog>();
        var message = $"User endpoint: {userEndpoint}, Status Code: 429, SessionID: {context.HttpContext.Session.Id}";
        logger?.Warn(message);

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.RequestServices.GetService<ILoggerFactory>()?
            .CreateLogger("Microsoft.AspNetCore.RateLimitingMiddleware")
            .LogWarning("OnRejected: {GetUserEndPoint}", userEndpoint);

        return new ValueTask();
    };

    
    options.AddPolicy(policyName: rateLimitName, partitioner: httpContext =>
    {
        var sessionId = httpContext.Session.Id;

        //var count = httpContext.Session.

        //var key = ".AspNetCore.Session";

        //var value = httpContext.Session.GetString(key);

        //if (value == null)
        //{
        //    httpContext.Session.SetString(key, sessionId);
        //    httpContext.Session.CommitAsync().GetAwaiter().GetResult();
        //}

        return RateLimitPartition.GetTokenBucketLimiter(sessionId, _ =>
        {
            return new TokenBucketRateLimiterOptions
            {
                TokenLimit = 1,
                AutoReplenishment = true,
                QueueLimit = 3,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                ReplenishmentPeriod = TimeSpan.FromMinutes(10),
                TokensPerPeriod = 1
            };
        });

    }).RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

app.UseCors("AllowSetOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.None
});
app.UseSession();

app.UseRateLimiter();
//app.Use(async (context, next) => 
//{
//    //var value = context.Request.Query["idvalue"];

//    var sessionId = context.Session?.Id;
//    if (context.Session?.GetInt32(sessionId) == null)
//    {
//        await Task.Delay(30000);
//        context.Session.SetInt32(sessionId, 1);
//    }
//    else
//    {
//        var num = context.Session.GetInt32(sessionId) ?? 0;
//        context.Session.SetInt32(sessionId, num + 1);
//    }

//    await context.Response.WriteAsync($"Session ID: {sessionId}, sessions: {context.Session?.GetInt32(sessionId)}");

//    await next(context);
//});

app.MapControllers()
    .RequireRateLimiting(rateLimitName);

static string GetUserEndPoint(HttpContext context) =>
   $"User {context.User.Identity?.Name ?? "Anonymous"} endpoint:{context.Request.Path}"
   + $" {context.Connection.RemoteIpAddress}";

await app.RunAsync();
