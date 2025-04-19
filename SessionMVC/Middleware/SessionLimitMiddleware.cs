using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace SessionMVC.Middleware;

public class SessionLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly int _maxSessions;
    private readonly IDistributedCache _distributedCache; // Для зберігання інформації про сесії

    public SessionLimitMiddleware(RequestDelegate next, int maxSessions, IDistributedCache distributedCache)
    {
        _next = next;
        _maxSessions = maxSessions;
        _distributedCache = distributedCache;
    }

    public async Task Invoke(HttpContext context)
    {
        // Отримуємо ідентифікатор користувача (наприклад, з ClaimsPrincipal)
        var sessionId = context.Session.Id;

        // Перевіряємо, чи користувач вже має активну сесію
        var existingSession = await _distributedCache.GetStringAsync($"session:{sessionId}");

        if (existingSession != null)
        {
            // Оновлюємо час останньої активності сесії
            await _distributedCache.SetStringAsync($"session:{sessionId}", DateTime.UtcNow.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20) // Сесія буде видалена через 20 хвилин бездіяльності
            });
        }
        else
        {
            // Перевіряємо загальну кількість активних сесій
            var currentSessionsCount = await _distributedCache.GetAsync("totalSessions");
            var sessionsCount = currentSessionsCount == null ? 0 : Convert.ToInt32(Encoding.UTF8.GetString(currentSessionsCount));

            if (sessionsCount >= _maxSessions)
            {
                // Ліміт сесій досягнуто, повертаємо відповідь про відмову
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsync("Too many active sessions.");
                return;
            }

            // Зберігаємо інформацію про нову сесію
            await _distributedCache.SetStringAsync($"session:{sessionId}", DateTime.UtcNow.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
            });
            await _distributedCache.SetAsync("totalSessions", Encoding.UTF8.GetBytes((sessionsCount + 1).ToString()));
        }

        await _next(context);
    }
}
