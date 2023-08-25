using JustRPG.Models.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class GuildMember
{
    [BsonElement("user")] public long user { get; set; }
    [BsonElement("rank")] public GuildRank rank { get; set;}
}