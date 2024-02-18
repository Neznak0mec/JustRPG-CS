using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class BattlesDB
{
    private LocalCache<Battle> _collection;
    private LocalCache<BattleResultDrop> _collectionDrop;

    public BattlesDB()
    {
        _collection = new LocalCache<Battle>();
        _collectionDrop = new LocalCache<BattleResultDrop>();
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
        _collection.Add(battle.id, battle);
        return battle;
    }


    public Task Update(Battle battle)
    {
        battle.lastActivity = DateTimeOffset.Now.ToUnixTimeSeconds();
        _collection.Add(battle.id, battle);
        return Task.CompletedTask;
    }

    public Task Delete(object? obj)
    {
        Battle temp = (Battle)obj!;
        _collection.Remove(temp.id);
        return Task.CompletedTask;
    }


    // BattleResultDrop
    public void AddDrop(BattleResultDrop drop)
    {
        _collectionDrop.Add(drop.id, drop);
    }

    public void UpdateDrop(BattleResultDrop drop)
    {
        _collectionDrop.Add(drop.id, drop);
    }

    public BattleResultDrop? GetDrop(string id)
    {
        return _collectionDrop.Get(id);
    }

    public BattleResultDrop? GetLastUserDrop(ulong userId)
    {
        List<BattleResultDrop?> temp = _collectionDrop.GetAll().Where(x => x!.userId == (long)userId).ToList();
        return temp.Count == 0 ? null : temp.First();
    }

    public void DeleteDrop(string id)
    {
        _collectionDrop.Remove(id);
    }

    public void ClearCache()
    {
        List<BattleResultDrop?> temp = _collectionDrop.GetAll();
        TimeSpan aTimeSpan = new TimeSpan(0, 0, 5, 0);
        foreach (var drop in temp)
        {
            if (drop == null)
                continue;
            if (((DateTimeOffset)drop.endTime).ToUnixTimeSeconds() <
                DateTimeOffset.Now.Subtract(aTimeSpan).ToUnixTimeSeconds())
            {
                _collectionDrop.Remove(drop.id);
            }
        }
    }
}