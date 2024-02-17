using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;
using Action = JustRPG.Models.Action;

namespace JustRPG.Services.Collections;

public class ActionDB
{
    private readonly LocalCache<Action?> _collection;

    public ActionDB(IMongoDatabase mongoDatabase)
    {
        _collection = new LocalCache<Action?>();
        
    }

    public Action? Get(string key)
    {
        
        return _collection.Get(key);

    }

    public Action CreateObject(Action action)
    {
        _collection.Add(action.id,action);
        return action;
    }

    public Action Update(Action obj)
    {
        _collection.Add(obj.id,obj);
        return obj;
    }

    public void Delete(string id)
    {
        _collection.Remove(id);
    }
    
    public void ClearCache()
    {
        TimeSpan aTimeSpan = new TimeSpan(0, 0, 5, 0);
        foreach (var action in _collection.GetAll())
        {
            if (((DateTimeOffset)action!.date).ToUnixTimeSeconds() < DateTimeOffset.Now.Subtract(aTimeSpan).ToUnixTimeSeconds())
            {
                _collection.Remove(action.id);
            }
        }
        
    }
}