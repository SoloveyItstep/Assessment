using System.ComponentModel.DataAnnotations;

namespace SessionMVC.Models;

/// <summary>
/// Summary
/// </summary>
public class Summary
{
    public Summary(string state)
    {
        this.State = state;
    }

    [Key]
    public int Id { get; set; }
    public string State { get; set; } = string.Empty;
}
