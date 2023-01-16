using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class Item
{
    [BsonElement("id")]public string id { get; set; } = "";
    [BsonElement("name")]public string name { get; set; } = "";
    [BsonElement("lvl")]public int lvl { get; set;  } = 0;
    [BsonElement("type")]public string type { get; set; } = "";
    [BsonElement("price")]public int price { get; set; } = 0;
    [BsonElement("description")]public string description { get; set; } = "";
    [BsonElement("rarity")]public string rarity { get; set; } = "";
    [BsonElement("give_stats")]public Stats? giveStats { get; set; }
    [BsonElement("generated")]public bool generated { get; set; } = false;
    
    [BsonElement("preset")]public string preset { get; set; } = "";

    public override string ToString()
    {
        if (generated)
            return
                $"<:health:997889169567260714>: {give_stats.hp} | <:strength:997889205684420718>: {give_stats.damage} | <:armor:997889166673186987>: {give_stats.defence} \n" +
                $"<:dexterity:997889168216694854>: {give_stats.luck} | <:crit:997889163552628757>: {give_stats.speed} | <:crit:997889163552628757>: {give_stats.krit}";
        else
            return description;
    }

    public bool IsEquippable()
    {
        return type is "helmet" or "armor" or "gloves" or "pants" or "weapon";
    }

}