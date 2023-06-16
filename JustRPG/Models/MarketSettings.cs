using JustRPG.Services;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class MarketSettings {
    [BsonElement("_id")]public string id {get;set;}
    [BsonElement("user_id")]public ulong userId{get;set;}
    [BsonElement("current_item_index")]public int CurrentItemIndex { get; set; } = 0;
    [BsonElement("search_results")]public List<SaleItem> SearchResults { get; set; } = new List<SaleItem>();
    [BsonElement("start_page")]public string startPage {get;set;}
    
    
    public void IncrementItemIndex()
    {
    
        if (CurrentItemIndex < SearchResults.Count - 1)
        {
            CurrentItemIndex++;
        }
    }
    
    public void DecrementItemIndex()
    {
        if (CurrentItemIndex > 0)
        {
            CurrentItemIndex--;
        }
    }
}