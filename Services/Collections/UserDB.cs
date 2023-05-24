using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;



public class UserDb : ICollection
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
        var filterUser = Builders<User>.Filter.Eq("id", temp.id);
        var updateUser = fieldKey == "exp" ? UpdateLvl(temp, value) : Builders<User>.Update.Inc(fieldKey, value);

        _collection.UpdateOne(filterUser, updateUser);
    }

    private UpdateDefinition<User> UpdateLvl(User user, int value)
    {
        UpdateDefinition<User> updateUser;
        if (user.exp + value >= user.expToLvl)
        {
            updateUser = Builders<User>.Update.Inc("lvl", 1);
            updateUser.Inc("skill_points", 3);
            updateUser.Inc("exp_to_lvl", user.expToLvl / 5);
            updateUser.Set("exp", 0);
        }
        else
            updateUser = Builders<User>.Update.Inc("exp", value);

        return updateUser;
    }

    public void Update(object obj)
    {
        User temp = (User)obj;
        var filterUser =Builders<User>.Filter.Eq("id", temp.id);
        _collection.ReplaceOne(filterUser,temp);
    }
}