using JustRPG.Services;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class MarketSlotsSettings
{
    [BsonElement("_id")] public string id { get; set; }
    [BsonElement("user_id")] public ulong userId { get; set; }
    [BsonElement("current_item_index")] public int currentItemIndex { get; set; } = 0;
    [BsonElement("search_results")] public List<SaleItem> searchResults { get; set; } = new List<SaleItem>();
    [BsonElement("start_page")] public string startPage { get; set; }


    public void IncrementItemIndex()
    {
        if (currentItemIndex < searchResults.Count - 1)
        {
            currentItemIndex++;
        }
    }

    public void DecrementItemIndex()
    {
        if (currentItemIndex > 0)
        {
            currentItemIndex--;
        }
    }
}