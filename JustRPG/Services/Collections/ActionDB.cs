using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;
using Action = JustRPG.Models.Action;

namespace JustRPG.Services.Collections;

public class ActionDB
{
    private readonly List<Action?> _collection;

    public ActionDB(IMongoDatabase mongoDatabase)
    {
        _collection = new List<Action?>();
        
    }

    public Action? Get(string key)
    {
        return _collection.FirstOrDefault(x=> x.id == key);

    }

    public Action CreateObject(Action id)
    {
        _collection.Add(id);
        return id;
    }

    public Action Update(Action obj)
    {
        var index = _collection.FindIndex(x => x!.id == obj.id);
        _collection[index] = obj;
        return obj;
    }

    public void Delete(string id)
    {
        _collection.RemoveAll(x => x!.id == id);
    }
    
    //autoremove old actions
    public void RemoveOldActions()
    {
        _collection.RemoveAll(x => DateTimeOffset.Now - x!.date > TimeSpan.FromMinutes(5));
    }
}