using JustRPG.Models.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public record Guild
{
    [BsonElement("_id")] public string tag { get; set; }
    [BsonElement("name")] public string name { get; set; }
    [BsonElement("logo")] public string logo { get; set; } = "";

    [BsonElement("premium")] public bool premium { get; set; } = false;

    [BsonElement("members")] public  List<GuildMember> members { get; set; } = new();
    [BsonElement("join_type")] public JoinType join_type { get; set; } = JoinType.close;
    [BsonElement("want_join")] public List<long> wantJoin { get; set; } = new();

    [BsonElement("symbol")] public string symbol { get; set; } = "";
}