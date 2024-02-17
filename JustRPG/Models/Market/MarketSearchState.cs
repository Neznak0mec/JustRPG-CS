using JustRPG.Models.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class MarketSearchState : PagedResult<SaleItem>
{
    [BsonElement("_id")] public string id { get; set; }
    [BsonElement("user_id")] public ulong userId { get; set; }
    
    [BsonElement("item_lvl")] public Tuple<int, int>? itemLvl { get; set; } = null;
    [BsonElement("item_rarity")] public Rarity? itemRarity { get; set; } = null;
    [BsonElement("item_type")] public ItemType? itemType { get; set; } = null;
    
    [BsonElement("start_page")] public string? startPage { get; set; }
}