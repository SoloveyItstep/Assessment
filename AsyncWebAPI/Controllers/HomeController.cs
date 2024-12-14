using AsyncWebAPI.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text;

namespace AsyncWebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private readonly MyDBContext _dbContext;

    public HomeController(MyDBContext context)
    {
        _dbContext = context;
    }

    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Debug.WriteLine($"=============================== Start Method ====== Thread ID: {Environment.CurrentManagedThreadId}");

        var url = "https://www.google.com";
        HttpClient client = new();
        var response = await await client.GetAsync(url)
            .ContinueWith(async (x) => 
            { 
                Debug.WriteLine($"=============================== Client continue with ======= Thread ID: {Environment.CurrentManagedThreadId}");
                var res = await x;
                return res;
            });

        Debug.WriteLine($"=============================== Client got ======= Thread ID: {Environment.CurrentManagedThreadId}");

        var usersTask = _dbContext.UsersList();
        var users = await usersTask;

        Debug.WriteLine($"=============================== got users ======== Thread ID: {Environment.CurrentManagedThreadId}");

        var content = await response.Content.ReadAsStringAsync();

        Debug.WriteLine($"=============================== Client content ======= Thread ID: {Environment.CurrentManagedThreadId}");

        await GetText();

        Debug.WriteLine($"=============================== File readed ======= Thread ID: {Environment.CurrentManagedThreadId}");

        await _dbContext.InsertData();

        Debug.WriteLine($"=============================== After Inseart ======= Thread ID: {Environment.CurrentManagedThreadId}");

        users = await _dbContext.UsersList();

        Debug.WriteLine($"=============================== Get users After Inseart ======= Thread ID: {Environment.CurrentManagedThreadId}");

        await _dbContext.Clear();

        Debug.WriteLine($"=============================== After Clear ======= Thread ID: {Environment.CurrentManagedThreadId}");

        Debug.WriteLine($"=============================== Time spent: {stopwatch.Elapsed.Seconds}");

        stopwatch.Stop();

        return Content(content);
    }

    [HttpGet("Sync")]
    public IActionResult Sync()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Debug.WriteLine($"=============================== Start Method ====== Thread ID: {Environment.CurrentManagedThreadId}");

        var url = "https://www.google.com";
        HttpClient client = new();
        var response = client.GetAsync(url).GetAwaiter().GetResult();

        Debug.WriteLine($"=============================== Client got ======= Thread ID: {Environment.CurrentManagedThreadId}");

        var users = _dbContext.UsersList().GetAwaiter().GetResult();

        Debug.WriteLine($"=============================== got users ======== Thread ID: {Environment.CurrentManagedThreadId}");

        var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        Debug.WriteLine($"=============================== Client content ======= Thread ID: {Environment.CurrentManagedThreadId}");

        GetText().GetAwaiter().GetResult();

        Debug.WriteLine($"=============================== File readed ======= Thread ID: {Environment.CurrentManagedThreadId}");

        _dbContext.InsertData().GetAwaiter().GetResult();

        Debug.WriteLine($"=============================== After Inseart ======= Thread ID: {Environment.CurrentManagedThreadId}");

        users = _dbContext.UsersList().GetAwaiter().GetResult();

        Debug.WriteLine($"=============================== Get users After Inseart ======= Thread ID: {Environment.CurrentManagedThreadId}");

        _dbContext.Clear().GetAwaiter().GetResult();

        Debug.WriteLine($"=============================== After Clear ======= Thread ID: {Environment.CurrentManagedThreadId}");

        Debug.WriteLine($"=============================== Time spent: {stopwatch.Elapsed.Seconds}");

        stopwatch.Stop();

        return Content(content);
    }

    [HttpGet("All")]
    public async Task<IActionResult> All()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Debug.WriteLine($"=============================== Start Method ====== Thread ID: {Environment.CurrentManagedThreadId}");

        var url = "https://www.google.com";
        HttpClient client = new();
        var responseTask = client.GetAsync(url);
        var usersTask = _dbContext.UsersList();
        var textTask = GetText();
        var inseartTask = _dbContext.InsertData();

        Debug.WriteLine($"=============================== Before wait all ====== Thread ID: {Environment.CurrentManagedThreadId}");
        Task.WaitAll(responseTask, textTask, inseartTask, usersTask);
        Debug.WriteLine($"=============================== After wait all ====== Thread ID: {Environment.CurrentManagedThreadId}");

        await inseartTask;
        Debug.WriteLine($"=============================== After users inseart ====== Thread ID: {Environment.CurrentManagedThreadId}");

        var users = await usersTask;
        Debug.WriteLine($"=============================== After get users ====== Thread ID: {Environment.CurrentManagedThreadId}   Users:{users.Count}");
        
        var response = await responseTask;
        var content = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"=============================== Afterget content ====== Thread ID: {Environment.CurrentManagedThreadId}");

        var text = await textTask;
        Debug.WriteLine($"=============================== After text read ====== Thread ID: {Environment.CurrentManagedThreadId}");

        await _dbContext.Clear();

        Debug.WriteLine($"=============================== After Clear ======= Thread ID: {Environment.CurrentManagedThreadId}");

        Debug.WriteLine($"=============================== Time spent: {stopwatch.Elapsed.Seconds}");

        stopwatch.Stop();

        return Content(content);
    }

    private static async Task<List<string>> GetText()
    {
        List<string> text = new ();
        const int BufferSize = 128;
        string? line = string.Empty;
        using var fileStream = System.IO.File.OpenRead("App_Data/text.txt");
        using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize);
        while ((line = await streamReader.ReadLineAsync()) != null)
        {
            text.Add(line);
        }

        return text;
    }
}
