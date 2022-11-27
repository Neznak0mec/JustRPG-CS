using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class GuildDB : Collection
{
    private readonly IMongoCollection<Guild> _collection;

    public GuildDB(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<Guild>("servers");
    }
    
    public object? Get(object val,string key="id")
    {
        var filterGuild =Builders<Guild>.Filter.Eq(key, val);  
        return _collection.Find(filterGuild).FirstOrDefault();
    }

    public object CreateObject(object id)
    {
        Guild newGuild = new Guild{id  = Convert.ToInt64(id)};
        _collection.InsertOne(newGuild);
        return newGuild;
    }

    public void Add(object where,string fieldKey, int value)
    {
        Guild temp = (Guild)where;
        var filterGuild =Builders<Guild>.Filter.Eq("id", temp.id);
        var updateGuild = Builders<Guild>.Update.Inc(fieldKey, value);
        _collection.UpdateOne(filterGuild,updateGuild);
    }

    public void Update(object obj)
    {
        Guild temp = (Guild)obj;
        var filterGuild =Builders<Guild>.Filter.Eq("id", temp.id);
        _collection.ReplaceOne(filterGuild,temp);
    }
}