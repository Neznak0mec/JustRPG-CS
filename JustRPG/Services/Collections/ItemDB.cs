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
    
    public async Task<object?> Get(object val,string key="id")
    {
        FilterDefinition<Item> filterItem =Builders<Item>.Filter.Eq(key, val);
        return await (await _collection.FindAsync(filterItem)).FirstOrDefaultAsync();
    }

    public async Task<object?> CreateObject(object? id)
    {
        Item temp = (Item)id;
        await _collection.InsertOneAsync(temp);
        return temp;
    }

    public async Task Add(object where,string fieldKey, int value)
    {
        Item temp = (Item)where;
        FilterDefinition<Item> filterItem =Builders<Item>.Filter.Eq("id", temp.id);
        UpdateDefinition<Item> updateItem = Builders<Item>.Update.Inc(fieldKey, value);
        await _collection.UpdateOneAsync(filterItem,updateItem);
    }

    public async Task Update(object? obj)
    {
        Item? temp = (Item)obj!;
        FilterDefinition<Item> filterItem =Builders<Item>.Filter.Eq("id", temp.id);
        await _collection.ReplaceOneAsync(filterItem!,temp);
    }
}