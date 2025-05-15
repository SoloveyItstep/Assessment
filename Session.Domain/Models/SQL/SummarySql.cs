using System.ComponentModel.DataAnnotations;

namespace Session.Domain.Models.SQL;

/// <summary>
/// Summary
/// </summary>
public record SummarySql(string State)
{
    [Key]
    public int Id { get; set; }
}
