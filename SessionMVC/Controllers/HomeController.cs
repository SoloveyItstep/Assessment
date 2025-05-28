using Microsoft.AspNetCore.Mvc;
using Session.Services.Services.Interfaces;
using SessionMVC.Models;
using System.Diagnostics;

namespace SessionMVC.Controllers;

public class HomeController(IHealthcheckSqlService healthcheckSqlService) : Controller
{
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

        var id = HttpContext.Session.Id;

        return Ok(new { id });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


    public async Task<IActionResult> DbCheck()
    {
        var result = await healthcheckSqlService.DatabaseCheck();

        return Ok(new { status = result });
    }
}
