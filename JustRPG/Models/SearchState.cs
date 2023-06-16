using JustRPG.Services;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class SearchState {
    [BsonElement("_id")]public string id {get;set;}
    [BsonElement("user_id")]public ulong userId{get;set;}
    [BsonElement("currentPage")]public int CurrentPage { get; set; } = 0;
    [BsonElement("currentItemIndex")]public int CurrentItemIndex { get; set; } = 0;
    [BsonElement("searchResults")]public List<SaleItem> SearchResults { get; set; } = new List<SaleItem>();

    [BsonElement("item_lvl")] public Tuple<int,int>? itemLvl {get;set;} = null;
    [BsonElement("item_rarity")] public string? itemRarity {get;set;} = null;
    [BsonElement("item_type")] public string? itemType {get;set;} = null;
    
    public List<SaleItem> GetItemsOnPage(int pageIndex)
    {
        List<SaleItem> res = SearchResults
            .Skip(5 * pageIndex).ToList();
        
        return res;
    }
    
    public void IncrementPage()
    {
        int nextPage = CurrentPage + 1;
        List<SaleItem> itemsOnNextPage = GetItemsOnPage(nextPage);
    
        if (itemsOnNextPage.Count > 0)
        {
            CurrentPage = nextPage;
            CurrentItemIndex = 0;
        }
    }
    
    public void DecrementPage()
    {
        if (CurrentPage > 0)
        {
            CurrentPage--;
            CurrentItemIndex = 0;
        }
    }
    
    public void IncrementItemIndex()
    {
        List<SaleItem> itemsOnCurrentPage = GetItemsOnPage(CurrentPage);
    
        if (CurrentItemIndex < itemsOnCurrentPage.Count - 1)
        {
            CurrentItemIndex++;
        }
        else if (CurrentItemIndex == 4)
            IncrementPage();
    }
    
    public void DecrementItemIndex()
    {
        if (CurrentItemIndex > 0)
        {
            CurrentItemIndex--;
        }
        else if (CurrentItemIndex == 0)
            DecrementPage();
    }
}