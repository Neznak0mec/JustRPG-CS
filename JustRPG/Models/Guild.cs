using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public record Guild
{
    [BsonElement("_id")] public string tag { get; set; }
    [BsonElement("name")] public string name { get; set; } = null;
    [BsonElement("members")] public List<ulong> members { get; set; } = new List<ulong>();
    [BsonElement("leader")] public ulong leader { get; set; }
    [BsonElement("officers")] public List<ulong> officers { get; set; }  = new List<ulong>();
}