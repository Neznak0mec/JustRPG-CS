using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class MarketSearchState
{
    [BsonElement("_id")] public string id { get; set; }
    [BsonElement("user_id")] public ulong userId { get; set; }
    [BsonElement("current_page")] public int currentPage { get; set; } = 0;
    [BsonElement("current_item_index")] public int currentItemIndex { get; set; } = 0;
    [BsonElement("search_results")] public List<SaleItem> searchResults { get; set; } = new List<SaleItem>();

    [BsonElement("item_lvl")] public Tuple<int, int>? itemLvl { get; set; } = null;
    [BsonElement("item_rarity")] public string? itemRarity { get; set; } = null;
    [BsonElement("item_type")] public string? itemType { get; set; } = null;

    public List<SaleItem> GetItemsOnPage(int pageIndex)
    {
        List<SaleItem> res = searchResults
            .Skip(5 * pageIndex).ToList();

        return res;
    }

    public void IncrementPage()
    {
        int nextPage = currentPage + 1;
        List<SaleItem> itemsOnNextPage = GetItemsOnPage(nextPage);

        if (itemsOnNextPage.Count > 0)
        {
            currentPage = nextPage;
            currentItemIndex = 0;
        }
    }

    public void DecrementPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            currentItemIndex = 0;
        }
    }

    public void IncrementItemIndex()
    {
        List<SaleItem> itemsOnCurrentPage = GetItemsOnPage(currentPage);

        if (currentItemIndex < itemsOnCurrentPage.Count - 1)
        {
            currentItemIndex++;
        }
        else if (currentItemIndex == 4)
            IncrementPage();
    }

    public void DecrementItemIndex()
    {
        if (currentItemIndex > 0)
        {
            currentItemIndex--;
        }
        else if (currentItemIndex == 0)
            DecrementPage();
    }
}