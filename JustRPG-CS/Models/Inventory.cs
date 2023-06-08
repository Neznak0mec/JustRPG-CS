using JustRPG.Services;
using MongoDB.Bson.Serialization.Attributes;
using Serilog;

namespace JustRPG.Models;

public class Inventory
{
    [BsonElement("_id")]public string? id { get; set; }
    
    [BsonElement("interactionType")]public string interactionType { get; set; } = "info";
    
    [BsonElement("currentPage")]public int currentPage { get; set; } = -1;
    
    [BsonElement("items")]public string[] items { get; set; } = Array.Empty<string>();
    
    [BsonElement("currentPageItems")]public string?[] currentPageItems { get; set; } = { null , null, null, null, null };
    
    [BsonElement("lastPage")]public int lastPage { get; set; } = 0;

    [BsonElement("lastUsage")]public long lastUsage { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();

    [BsonIgnore]public DataBase? DataBase = null;

    public void NextPage()
    {
        
        currentPage++;
        if (currentPage > lastPage)
            currentPage = lastPage;
        
        Save();
    }

    public void PreviousPage()
    {
        currentPage--;
        
        if (currentPage < 0)
            currentPage = 0;
        
        Save();
    }

    public void Reload(string[] inventory)
    {
        interactionType = "info";
        currentPage = 0;
        items = inventory;
        lastPage = items.Length / 5 ;
        lastUsage = DateTimeOffset.Now.ToUnixTimeSeconds();
        
        Save();
    }

    public async Task<Item?[]> GetItems(DataBase dataBase)
    {
        Item?[] getItems = {null, null, null, null, null };
        for (int i = 0; i < currentPageItems.Length; i++)
        {
            if (currentPageItems[i] == null)
                break;

            getItems[i] = (Item) (await dataBase.ItemDb.Get(currentPageItems[i]!))!;
        }

        return getItems;
    }

    private void UpdatePageItems()
    {
        int lastItemIndex = currentPage * 5 + 5;
        Array.Fill(currentPageItems, null);
        for (int i = 0,itemIndex = currentPage*5; itemIndex < lastItemIndex; i++, itemIndex++)
        {
            if (itemIndex >= items.Length)
                return;
            currentPageItems[i] = items[itemIndex];
        }
    }
    
    private void Save(){
        if (DataBase != null)
        {
            UpdatePageItems();
            DataBase.InventoryDb.Update(this);
        }
    }
}
