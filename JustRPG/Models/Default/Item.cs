using JustRPG.Features;
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
                $":heart: : {giveStats!.hp} | :dagger: : {giveStats.damage} | :shield: : {giveStats.defence} \n" +
                $":four_leaf_clover: : {giveStats.luck} | :zap: : {giveStats.speed} | :gem: : {SecondaryFunctions.GetRarityColoredEmoji(rarity)}";
        else
            return description;
               
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