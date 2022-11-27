using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;



public class UserDb : Collection
{
    private readonly IMongoCollection<User> _collection;

    public UserDb(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<User>("users");
    }
    
    public object? Get(object val,string key="id")
    {
        var filterUser =Builders<User>.Filter.Eq(key, val);  
        return _collection.Find(filterUser).FirstOrDefault();
    }

    public object CreateObject(object id)
    {
        User newUser = new User{id  = Convert.ToInt64(id)};
        _collection.InsertOne(newUser);
        return newUser;
    }

    public void Add(object where,string fieldKey, int value)
    {
        User temp = (User)where;
        var filterUser =Builders<User>.Filter.Eq("id", temp.id);
        var updateUser = Builders<User>.Update.Inc(fieldKey, value);
        _collection.UpdateOne(filterUser,updateUser);
    }

    public void Update(object obj)
    {
        User temp = (User)obj;
        var filterUser =Builders<User>.Filter.Eq("id", temp.id);
        _collection.ReplaceOne(filterUser,temp);
    }
}