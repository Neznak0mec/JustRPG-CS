using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class Info
{
    [BsonElement("_id")] public string id = "info";
    [BsonElement("dungeons")] private Dungeon[] dungeons { get; set; }
}

class Dungeon
{
    [BsonElement("name")] public string name { get; set; }
    [BsonElement("description")] public string description { get; set; }
    [BsonElement("lvl")]public int lvl { get; set; }
    
    
}