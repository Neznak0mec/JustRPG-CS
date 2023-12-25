using JustRPG.Models.Enums;
using JustRPG.Models.SubClasses;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class Item
{
    [BsonElement("id")] public string id { get; set; }
    [BsonElement("name")] public string name { get; set; } = "";
    [BsonElement("lvl")] public int lvl { get; set; }
    [BsonElement("type")] public ItemType type { get; set; }
    [BsonElement("price")] public int price { get; set; } = 0;
    [BsonElement("description")] public string description { get; set; } = "";
    [BsonElement("rarity")] public Rarity rarity { get; set; } = Rarity.common;
    [BsonElement("give_stats")] public Stats? giveStats { get; set; }
    [BsonElement("generated")] public bool generated { get; set; } = false;

    [BsonElement("preset")] public string preset { get; set; } = "";

    public override string ToString()
    {
        if (generated)
            return
                $"<:health:997889169567260714>: {giveStats!.hp} | <:strength:997889205684420718>: {giveStats.damage} | <:armor:997889166673186987>: {giveStats.defence} \n" +
                $"<:dexterity:997889168216694854>: {giveStats.luck} | <:crit:997889163552628757>: {giveStats.speed}";
        else
            return description;
    }

    public string ToStringWithRarity()
    {
        string res = ToString();
        res += "| редкость: " +
               rarity switch
               {
                   Rarity.common => "⬜",
                   Rarity.uncommon => "🟦",
                   Rarity.rare => "🟪",
                   Rarity.epic => "🟨",
                   Rarity.legendary => "🟥",
                   Rarity.impossible => "👾",
                   Rarity.exotic => "🔳",
                   Rarity.prize => "🎁",
                   Rarity.eventt => "✨",
                   _ => "⬜"
               };
        return res;
    }

    public bool IsEquippable()
    {
        return type is
            ItemType.armor or
            ItemType.shoes or 
            ItemType.weapon or
            ItemType.gloves or 
            ItemType.pants or 
            ItemType.helmet;
    }
}