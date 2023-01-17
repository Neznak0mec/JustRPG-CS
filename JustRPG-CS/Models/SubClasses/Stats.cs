using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models.SubClasses;

public record Stats
{
    [BsonElement("hp")]public int hp { get; set; } = 0;
    [BsonElement("damage")]public int damage { get; set; } = 0;
    [BsonElement("defence")]public int defence { get; set; } = 0;
    [BsonElement("luck")]public int luck { get; set; } = 0;
    [BsonElement("speed")]public int speed { get; set; } = 0;
}