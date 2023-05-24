using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class ItemDB : ICollection
{
    private readonly IMongoCollection<Item> _collection;

    public ItemDB(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<Item>("items");
    }
    
    public object? Get(object val,string key="id")
    {
        var filterItem =Builders<Item>.Filter.Eq(key, val);  
        return _collection.Find(filterItem).FirstOrDefault();
    }

    public object CreateObject(object id)
    {
        Item newItem = new Item{id  = id.ToString()};
        _collection.InsertOne(newItem);
        return newItem;
    }

    public void Add(object where,string fieldKey, int value)
    {
        Item temp = (Item)where;
        var filterItem =Builders<Item>.Filter.Eq("id", temp.id);
        var updateItem = Builders<Item>.Update.Inc(fieldKey, value);
        _collection.UpdateOne(filterItem,updateItem);
    }

    public void Update(object obj)
    {
        Item temp = (Item)obj;
        var filterItem =Builders<Item>.Filter.Eq("id", temp.id);
        _collection.ReplaceOne(filterItem,temp);
    }
}