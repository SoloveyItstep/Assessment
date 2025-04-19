using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SessionMVC.Models;

public class QueueModel: PageModel
{
    public List<string> Messages { get; set; } = [];
}
