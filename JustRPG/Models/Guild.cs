using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public record Guild
{
    [BsonElement("_id")] public long id { get; set; }
    [BsonElement("m_scroll")] public bool scroll { get; set; }
    [BsonElement("prefix")] public string? prefix { get; set; } = null;
    [BsonElement("language")] public string? language { get; set; }
}