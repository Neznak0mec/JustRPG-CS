using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG_CS.Services.Collections;

public class BattlesDB: ICollection{
    
    private readonly IMongoCollection<Battle> _collection;

    public BattlesDB(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<Battle>("battles");
    }
    
    public object? Get(object val, string key = "id")
    {
        var filterBattle =Builders<Battle>.Filter.Eq(key, val);  
        return _collection.Find(filterBattle).FirstOrDefault();
    }

    public object CreateObject(object id)
    {
        Battle temp = (Battle)id;
        temp.lastActivity = DateTimeOffset.Now.ToUnixTimeSeconds();
        _collection.InsertOne(temp);
        return id;
    }

    public void Add(object where, string fieldKey, int value)
    {
        throw new NotImplementedException();
    }

    public void Update(object obj)
    {
        Battle temp = (Battle)obj;
        temp.lastActivity = DateTimeOffset.Now.ToUnixTimeSeconds();
        var filterInventory =Builders<Battle>.Filter.Eq("id", temp.id);
        _collection.ReplaceOne(filterInventory,temp);
    }

    public void Delete(object obj)
    {
        Battle battle = (Battle)obj;
        _collection.DeleteOne(x => x.id == battle.id);
    }
}
