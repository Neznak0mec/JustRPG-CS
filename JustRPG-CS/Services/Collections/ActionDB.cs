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
    
    public object? Get(object val, string key = "_id")
    {
        var filterAction =Builders<Action>.Filter.Eq(key, val);  
        return _collection.Find(filterAction).FirstOrDefault();
    }

    public object CreateObject(object id)
    {
        _collection.InsertOne((Action)id);
        return id;
    }

    public void Add(object where, string fieldKey, int value)
    {
        throw new NotImplementedException();
    }

    public void Update(object obj)
    {
        throw new NotImplementedException();
    }

    public void Remove(string id)
    {
        _collection.DeleteOne(x => x.id == id);
    }
}