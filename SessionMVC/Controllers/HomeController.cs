using log4net;
using Microsoft.AspNetCore.Mvc;
using SessionMVC.Context;
using SessionMVC.Models;
using System.Diagnostics;

namespace SessionMVC.Controllers;
public class HomeController : Controller
{
    private readonly ILog _logger;
    private readonly AssessmentDbContext _context;

    public HomeController(ILog logger, AssessmentDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Queue()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        var key = ".AspNetCore.Session";

        var value = HttpContext.Session.GetString(key);

        if (value == null)
        {
            HttpContext.Session.SetString(key, HttpContext.Session.Id);
            HttpContext.Session.CommitAsync().GetAwaiter().GetResult();
        }

        var id = base.HttpContext.Session.Id;
        return Ok(new { id });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


    public async Task<IActionResult> DbCheck()
    {
        var result = await _context.Database.CanConnectAsync();
        var message = $"Database connection status: {result}";
        _logger.Info(message);

        return Ok(new { status = result });
    }
}
