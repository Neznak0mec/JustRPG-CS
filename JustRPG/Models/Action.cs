using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class Action
{
    [BsonElement("_id")] public string id { get; set; }
    [BsonElement("args")] public string[] args { get; set; }
    [BsonElement("type")] public string type { get; set; }
    [BsonElement("userId")] public long userId { get; set; }
    [BsonElement("date")] public long date { get; set; }
}