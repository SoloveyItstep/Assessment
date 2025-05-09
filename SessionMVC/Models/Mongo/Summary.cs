using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SessionMVC.Models;

/// <summary>
/// Summary
/// </summary>
[BsonIgnoreExtraElements]
public class SummaryMongoDB
{
    public SummaryMongoDB(string state, int id)
    {
        this.State = state;
        this.Id = id;
    }

    [Key]
    //[BsonId]
    //[BsonRepresentation(BsonType.ObjectId)]
    public int Id { get; set; }

    public string State { get; set; }
}
