using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;
using Serilog;

namespace JustRPG.Services.Collections;

public class InventoryDB: Collection
{
    private readonly IMongoCollection<Inventory> _collection;
    private readonly DataBase _dataBase;

    public InventoryDB(IMongoDatabase mongoDatabase, DataBase dataBase)
    {
        _dataBase = dataBase;
        _collection = mongoDatabase.GetCollection<Inventory>("interactions");
    }
    
    public object? Get(object val,string key="id")
    {
        var filterInventory = Builders<Inventory>.Filter.Eq(key, val);
        
        if (key != "id")
            return _collection.Find(filterInventory).FirstOrDefault();
        
        
        Inventory temp = new Inventory
        {
            id = val.ToString()
        };
        if (_collection.CountDocuments(x => x.id == temp.id) > 0)
            temp = _collection.Find(filterInventory).FirstOrDefault();
        else
            CreateObject(temp);
                
        TimeSpan aTimeSpan = new TimeSpan(0,0,5,0);
        if (temp.lastUsage < DateTimeOffset.Now.Subtract(aTimeSpan).ToUnixTimeSeconds())
        {
            CreateObject(temp);
        }
        
        return temp;
    }

    public object CreateObject(object id)
    {
        var temp = (Inventory)id;
        
        if (_collection.CountDocuments(x => x.id == temp.id) > 0)
            _collection.ReplaceOne(x => x.id == temp.id, temp);
        else
            _collection.InsertOne(temp);

        string userid = temp.id.Split('_')[2];
        User user = (User)_dataBase.UserDb.Get("id", userid)!;
        temp.Reload(user.inventory);
        
        return null;
    }

    public void Add(object where,string fieldKey, int value)
    {
        Inventory temp = (Inventory)where;
        var filterInventory =Builders<Inventory>.Filter.Eq("id", temp.id);
        var updateInventory = Builders<Inventory>.Update.Inc(fieldKey, value);
        _collection.UpdateOne(filterInventory,updateInventory);
    }

    public void Update(object obj)
    {
        Inventory temp = (Inventory)obj;
        var filterInventory =Builders<Inventory>.Filter.Eq("id", temp.id);
        _collection.ReplaceOne(filterInventory,temp);
    }
}