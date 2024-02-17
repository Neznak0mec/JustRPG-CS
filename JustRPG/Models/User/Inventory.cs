using JustRPG.Models.Enums;
using JustRPG.Services;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class Inventory : PagedResult<Item>
{
    public string? id { get; set; }
    public string interactionType { get; set; } = "info";
    public string[] userItems { get; set; } = Array.Empty<string>();
    public DateTime lastUsage { get; set; } = DateTime.Now;
    
    public long userId { get; set; }
    
    // sorting
    public bool showSortSelections { get; set; } = false;
    public Tuple<int, int>? itemLvl { get; set; } = null;
    public Rarity? itemRarity { get; set; } = null;
    public ItemType? itemType { get; set; } = null;
    

    public async Task Reload(DataBase db)
    {
        interactionType = "info";
        userItems = (await db.UserDb.Get(userId))!.inventory.ToArray();
        
        Items.Clear();
        foreach (var item in userItems)
        {
            var itemFromDb = await db.ItemDb.Get(item);
            if (itemFromDb != null)
            {
                Items.Add((Item)itemFromDb);
            }
        }
        
        itemLvl = null;
        itemRarity = null;
        itemType = null;
        
        if (CurrentPage > GetCountOfPages() - 1)
            CurrentPage = GetCountOfPages() - 1;

        await Save(db);
    }
    
    public Item?[] GetItems()
    {
        Item?[] res = [null,null,null,null,null];
        List<Item> itemsOnCurrentPage = GetItemsOnPage(CurrentPage);
        for (int i = 0; i < itemsOnCurrentPage.Count; i++)
        {
            res[i] = itemsOnCurrentPage[i];
        }

        return res;
    }

    public async Task Sort(DataBase? db)
    {
        if (db == null) return;

        userItems = (await db.UserDb.Get(userId))!.inventory.ToArray();
        
        Items.Clear();
        foreach (var item in userItems)
        {
            var itemFromDb = db.ItemDb.Get(item).Result;
            if (itemFromDb != null)
            {
                Items.Add((Item)itemFromDb);
            }
        }

        
        if (itemLvl != null)
        {
            Items = Items
                .Where(item => item.lvl >= itemLvl.Item1 && item.lvl <= itemLvl.Item2)
                .ToList();
        }
        if (itemRarity != null)
        {
            Items = Items
                .Where(item => item.rarity == itemRarity)
                .ToList();
        }
        if (itemType != null)
        {
            Items = Items
                .Where(item => item.type == itemType)
                .ToList();
        }
    }
    
    public async Task Save(DataBase? db = null)
    {
        if (db == null) return;
        lastUsage = DateTime.Now;
        db.InventoryDb.Update(this);
    }
}