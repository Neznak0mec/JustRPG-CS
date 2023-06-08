using Discord.WebSocket;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG_CS.Models;

public class PVP {
    [BsonElement("last_interaction")] public long lastInteraction{get;set;}
    [BsonElement("battle_id")]public string battleId {get;set;}
    [BsonElement("messages")]public List<SocketInteraction> msgLocations {get;set;}
}