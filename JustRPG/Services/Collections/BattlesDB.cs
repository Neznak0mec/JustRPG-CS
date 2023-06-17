using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class BattlesDB : ICollection
{
    private readonly IMongoCollection<Battle> _collection;

    public BattlesDB(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<Battle>("battles");
    }

    public async Task<object?> Get(object val, string key = "_id")
    {
        var filterAction = Builders<Battle>.Filter.Eq(key, val);
        var res = await _collection.FindAsync(filterAction);
        return res.FirstOrDefault();
    }

    public async Task<object?> CreateObject(object? id)
    {
        Battle temp = (Battle)id!;
        temp.lastActivity = DateTimeOffset.Now.ToUnixTimeSeconds();
        await _collection.InsertOneAsync(temp);
        return id;
    }


    public async Task Update(object? obj)
    {
        Battle temp = (Battle)obj!;
        temp.lastActivity = DateTimeOffset.Now.ToUnixTimeSeconds();
        var filterInventory = Builders<Battle>.Filter.Eq("id", temp.id);
        await _collection.ReplaceOneAsync(filterInventory, temp);
    }

    public async Task Delete(object? obj)
    {
        Battle battle = (Battle)obj!;
        await _collection.DeleteOneAsync(x => x.id == battle.id);
    }
}