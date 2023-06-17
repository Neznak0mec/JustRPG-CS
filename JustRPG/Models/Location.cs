using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class Location
{
    [BsonElement("_id")] public string id { get; set; }
    [BsonElement("name")] public string name { get; set; }
    [BsonElement("description")] public string description { get; set; }
    [BsonElement("lvl")]public int lvl { get; set; }
    [BsonElement("type")] public string type { get; set; }
    [BsonElement("monsters")] public Dictionary<string,string> monsters { get; set;}
    [BsonElement("drops")] public Dictionary<string, string> drops { get; set; } = new Dictionary<string, string>();
}