using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class BattlesDB
{
    private LocalCache<Battle> _collection;

    public BattlesDB()
    {
        _collection = new LocalCache<Battle>();
    }

    public Battle? Get(string val)
    {
        return _collection.Get(val);
    }

    public List<Battle?> GetAll()
    {
        return _collection.GetAll();
    }

    public Battle? CreateObject(Battle battle)
    {
        battle.lastActivity = DateTimeOffset.Now.ToUnixTimeSeconds();
        _collection.Add(battle.id,battle);
        return battle;
    }


    public Task Update(Battle battle)
    {
        battle.lastActivity = DateTimeOffset.Now.ToUnixTimeSeconds();
        _collection.Add(battle.id,battle);
        return Task.CompletedTask;
    }

    public Task Delete(object? obj)
    {
        Battle temp = (Battle)obj!;
        _collection.Remove(temp.id);
        return Task.CompletedTask;
    }
}