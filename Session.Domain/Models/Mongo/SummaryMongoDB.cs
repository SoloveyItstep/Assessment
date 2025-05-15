using MongoDB.Bson.Serialization.Attributes;

namespace Session.Domain.Models.Mongo;

/// <summary>
/// Summary
/// </summary>
[BsonIgnoreExtraElements]
public record SummaryMongoDB(string State, int Id)
{ }
