using JustRPG.Models.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class SaleItem
{
    [BsonElement("_id")] public string id { get; set; }
    [BsonElement("user_id")] public ulong userId { get; set; }
    [BsonElement("item_id")] public string itemId { get; set; }
    [BsonElement("price")] public int price { get; set; }
    [BsonElement("date_listed")] public long dateListed { get; set; }
    [BsonElement("item_description")] public string itemDescription { get; set; }
    [BsonElement("is_visible")] public bool isVisible { get; set; }

    [BsonElement("item_name")] public string itemName { get; set; }
    [BsonElement("item_lvl")] public int itemLvl { get; set; }
    [BsonElement("item_rarity")] public Rarity itemRarity { get; set; }
    [BsonElement("item_type")] public string itemType { get; set; }
}