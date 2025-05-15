using System.ComponentModel.DataAnnotations;

namespace Session.Domain.Models.SQL;

public record WeatherForecastSql
{
    [Key]
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public string? Summary { get; set; }
}
