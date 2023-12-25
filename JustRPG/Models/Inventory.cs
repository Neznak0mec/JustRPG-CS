using JustRPG.Services;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class Inventory
{
    [BsonElement("_id")] public string? id { get; set; }
    [BsonElement("interactionType")] public string interactionType { get; set; } = "info";
    [BsonElement("current_page")] public int currentPage { get; set; } = 0;
    
    [BsonElement("current_item_index")] public int currentItemIndex { get; set; } = 0;

    [BsonElement("user_items_as_str")] public string[] userItems { get; set; } = Array.Empty<string>();

    [BsonElement("items")] public List<Item> items { get; set; } = new();

    [BsonElement("lastUsage")] public DateTime lastUsage { get; set; } = DateTime.Now;

    public int GetCountOfPages()
    {
        return (int)Math.Ceiling((double)items.Count / 5);
    }
    
    public bool IsLastPage()
    {
        return currentPage == GetCountOfPages() - 1;
    }
    
    public void IncrementPage()
    {
        int nextPage = currentPage + 1;
        List<Item> itemsOnNextPage = GetItemsOnPage(nextPage);

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
        List<Item> itemsOnCurrentPage = GetItemsOnPage(currentPage);

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

    public async Task Reload(List<string> inventory,DataBase db)
    {
        interactionType = "info";
        currentPage = 0;
        userItems = inventory.ToArray();

        items.Clear();
        foreach (var item in userItems)
        {
            var itemFromDb = await db.ItemDb.Get(item);
            if (itemFromDb != null)
            {
                items.Add((Item)itemFromDb);
            }
        }
        
    }
    
    public List<Item> GetItemsOnPage(int pageIndex)
    {
        List<Item> res = items
            .Skip(5 * pageIndex).Take(5).ToList();
       return res;
    }

    public Item?[] GetItems()
    {
        Item?[] res = {null,null,null,null,null};
        List<Item> itemsOnCurrentPage = GetItemsOnPage(currentPage);
        for (int i = 0; i < itemsOnCurrentPage.Count; i++)
        {
            res[i] = itemsOnCurrentPage[i];
        }

        return res;
    }

    public async Task Save(DataBase? db = null)
    {
        if (db != null)
        {
            lastUsage = DateTime.Now;
            await db.InventoryDb.Update(this);
        }
    }
}