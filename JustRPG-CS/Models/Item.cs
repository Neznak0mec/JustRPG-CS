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
                $"<:health:997889169567260714>: {giveStats.hp} | <:strength:997889205684420718>: {giveStats.damage} | <:armor:997889166673186987>: {giveStats.defence} \n" +
                $"<:dexterity:997889168216694854>: {giveStats.luck} | <:crit:997889163552628757>: {giveStats.speed}";
        else
            return description;
    }

    public bool IsEquippable()
    {
        return type is "helmet" or "armor" or "gloves" or "pants" or "weapon";
    }

}
public class Stats
{
    [BsonElement("hp")]public int hp { get; set; } = 0;
    [BsonElement("damage")]public int damage { get; set; } = 0;
    [BsonElement("defence")]public int defence { get; set; } = 0;
    [BsonElement("luck")]public int luck { get; set; } = 0;
    [BsonElement("speed")]public int speed { get; set; } = 0;
}