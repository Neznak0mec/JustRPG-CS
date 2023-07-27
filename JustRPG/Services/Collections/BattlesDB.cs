using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class BattlesDB : ICollection
{
    private List<Battle> _collection;

    public BattlesDB(IMongoDatabase mongoDatabase)
    {
        _collection = new List<Battle>();
    }

    public Task<object?> Get(object val, string key = "_id")
    {
        Battle temp = _collection.First(x => x.id == (string)val);
        temp.log = "";
        return Task.FromResult<object?>(temp);
    }

    public Task<object?> GetAll()
    {
        return Task.FromResult<object?>(_collection);
    }

    public Task<object?> CreateObject(object? id)
    {
        Battle temp = (Battle)id!;
        temp.lastActivity = DateTimeOffset.Now.ToUnixTimeSeconds();
        _collection.Add(temp);
        return Task.FromResult(id);
    }


    public Task Update(object? obj)
    {
        Battle temp = (Battle)obj!;
        temp.lastActivity = DateTimeOffset.Now.ToUnixTimeSeconds();
        _collection.Remove(_collection.First(x => x.id == temp.id));
        _collection.Add(temp);
        return Task.CompletedTask;
    }

    public Task Delete(object? obj)
    {
        Battle temp = (Battle)obj!;
        temp.lastActivity = DateTimeOffset.Now.ToUnixTimeSeconds();
        _collection.Remove(_collection.First(x => x.id == temp.id));
        return Task.CompletedTask;
    }
}