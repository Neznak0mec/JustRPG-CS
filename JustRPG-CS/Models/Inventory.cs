using JustRPG.Services;
using MongoDB.Bson.Serialization.Attributes;
using Serilog;

namespace JustRPG.Models;

public class Inventory
{
    [BsonElement("_id")]
    public string id { get; set; }
    
    [BsonElement("interactionType")]
    public string interactionType { get; set; } = "info";
    
    [BsonElement("currentPage")]
    public int currentPage { get; set; } = -1;
    
    [BsonElement("items")]
    public string[] items { get; set; } = Array.Empty<string>();
    
    [BsonElement("currentPageItems")]
    public string?[] currentPageItems { get; set; } = { null , null, null, null, null };
    
    [BsonElement("lastPage")]
    public int lastPage { get; set; } = 0;

    [BsonElement("lastUsage")]
    public long lastUsage { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();

    [BsonIgnore] 
    private DataBase _dataBase;
    
    public void NextPage()
    {
        if (currentPage >= lastPage)
        {
            currentPage = lastPage;
            return;
        }

        currentPage++;
        int lastItem = (currentPage + 1) * 5 > items.Length ? items.Length : (currentPage + 1) * 5-1;
        Array.Fill(currentPageItems, null);
        for (int i = 0,itemIndex = currentPage*5-1; itemIndex < lastItem; i++, itemIndex++)
        {
            currentPageItems[i] = items[itemIndex];
        }
        
    }

    public void PrewPage()
    {
        if (currentPage == 0)
            return;

        currentPage--;
        int lastItem = (currentPage + 1) * 5 > items.Length ? items.Length : (currentPage + 1) * 5-1;
        Array.Fill(currentPageItems, null);
        for (int i = 0,itemIndex = currentPage*5; itemIndex < lastItem; i++, itemIndex++)
        {
            currentPageItems[i] = items[itemIndex];
        }
    }

    public void Reload(string[] inventory)
    {
        interactionType = "info";
        currentPage = 0;
        items = inventory;
        lastPage = items.Length / 5 ;
        Log.Debug(lastPage.ToString());
        Array.Fill(currentPageItems, null);
        for (int i = 0; i < (items.Length > 5 ? 5 : items.Length); i++)
        {
            currentPageItems[i] = items[i];
        }
    }

    public Item?[] GetItems(DataBase dataBase)
    {
        _dataBase = dataBase;
        Item?[] getItems = {null, null, null, null, null };
        for (int i = 0; i < currentPageItems.Length; i++)
        {
            if (currentPageItems[i] == null)
                break;

            getItems[i] = (Item)_dataBase.GetFromDataBase(Bases.Items, "id", currentPageItems[i]!)!;
        }

        return getItems;
    }
    
    ~Inventory()
    {
        Log.Debug("Destructed");
        _dataBase.Update(Bases.Interactions,this);
    }
}
