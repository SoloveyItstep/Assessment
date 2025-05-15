using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Session.Domain.Models.Mongo;

[BsonIgnoreExtraElements]
public record WeatherForecastMongoDB
{
    [Key]
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public string? Summary { get; set; }
}
