using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;
using Action = JustRPG.Models.Action;

namespace JustRPG.Services.Collections;

public class ActionDB : ICollection
{
    private readonly IMongoCollection<Action> _collection;

    public ActionDB(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<Action>("interactions");
    }

    public async Task<object?> Get(object val, string key = "_id")
    {
        var filterAction = Builders<Action>.Filter.Eq(key, val);
        var res = await _collection.FindAsync(filterAction);
        return await res.FirstOrDefaultAsync();
    }

    public async Task<object?> CreateObject(object? id)
    {
        await _collection.InsertOneAsync((Action)id!);
        return id;
    }

    public async Task Update(object? obj)
    {
        Action? temp = (Action)obj!;
        FilterDefinition<Action> filterAction = Builders<Action>.Filter.Eq("id", temp.id)!;
        await _collection.ReplaceOneAsync(filterAction, temp);
    }

    public async Task Delete(string id)
    {
        await _collection.DeleteOneAsync(x => x.id == id);
    }
}