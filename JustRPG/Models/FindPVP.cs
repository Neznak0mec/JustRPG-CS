using Discord.WebSocket;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class FindPVP
{
    [BsonElement("type")] public string type = "findPvp";
    [BsonElement("userId")] public long userId { get; set; }
    [BsonElement("mmr")] public int mmr { get; set; }
    [BsonElement("strat_time")] public long stratTime { get; set; }
    [BsonElement("msg")] public SocketInteraction msgLocation { get; set; }
}