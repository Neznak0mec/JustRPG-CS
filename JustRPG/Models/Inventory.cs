using JustRPG.Services;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class Inventory
{
    [BsonElement("_id")] public string? id { get; set; }
    [BsonElement("interactionType")] public string interactionType { get; set; } = "info";
    [BsonElement("currentPage")] public int currentPage { get; set; } = -1;
    [BsonElement("items")] public List<string> items { get; set; } = new List<string>();

    [BsonElement("currentPageItems")]
    public string?[] currentPageItems { get; set; } = { null, null, null, null, null };

    [BsonElement("lastPage")] public int lastPage { get; set; } = 0;
    [BsonElement("lastUsage")] public long lastUsage { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
    [BsonIgnore] public DataBase? DataBase = null;

    public async Task NextPage()
    {
        currentPage++;
        if (currentPage > lastPage)
            currentPage = lastPage;

        await Save();
    }

    public async Task PreviousPage()
    {
        currentPage--;

        if (currentPage < 0)
            currentPage = 0;

        await Save();
    }

    public async Task Reload(List<string> inventory)
    {
        interactionType = "info";
        currentPage = 0;
        items = inventory;
        lastPage = items.Count / 5;
        lastUsage = DateTimeOffset.Now.ToUnixTimeSeconds();

        await Save();
    }

    public async Task<Item?[]> GetItems(DataBase dataBase)
    {
        Item?[] getItems = { null, null, null, null, null };
        int counter = 0;
        foreach (var i in currentPageItems)
        {
            if (i == null)
                continue;

            var a = await dataBase.ItemDb.Get(i);

            getItems[counter] = (Item)a!;
            counter++;
        }

        return getItems;
    }

    private void UpdatePageItems()
    {
        int lastItemIndex = currentPage * 5 + 5;
        Array.Fill(currentPageItems, null);
        for (int i = 0, itemIndex = currentPage * 5; itemIndex < lastItemIndex; i++, itemIndex++)
        {
            if (itemIndex >= items.Count)
                return;
            currentPageItems[i] = items[itemIndex];
        }
    }

    private async Task Save()
    {
        if (DataBase != null)
        {
            UpdatePageItems();
            await DataBase.InventoryDb.Update(this);
        }
    }
}