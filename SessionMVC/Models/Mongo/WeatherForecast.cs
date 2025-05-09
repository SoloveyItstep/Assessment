using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
namespace SessionMVC.Models;

[BsonIgnoreExtraElements]
public class WeatherForecastMongoDB
{
    [Key]
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public string? Summary { get; set; }
}
