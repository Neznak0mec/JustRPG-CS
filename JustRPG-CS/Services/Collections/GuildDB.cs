using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class GuildDB : ICollection
{
    private readonly IMongoCollection<Guild> _collection;

    public GuildDB(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<Guild>("servers");
    }
    
    public async Task<object?> Get(object val, string key = "_id")
    {
        FilterDefinition<Guild> filterAction =Builders<Guild>.Filter.Eq(key, val);
        return await (await _collection!.FindAsync(filterAction)).FirstOrDefaultAsync();
    }

    public async Task<object?> CreateObject(object? id)
    {
        Guild? newGuild = new Guild{id  = Convert.ToInt64(id)};
        await _collection.InsertOneAsync(newGuild);
        return newGuild;
    }

    public async Task Add(object where,string fieldKey, int value)
    {
        Guild temp = (Guild)where;
        FilterDefinition<Guild> filterGuild =Builders<Guild>.Filter.Eq("id", temp.id);
        UpdateDefinition<Guild> updateGuild = Builders<Guild>.Update.Inc(fieldKey, value);
        await _collection.UpdateOneAsync(filterGuild!,updateGuild!);
    }

    public async Task Update(object? obj)
    {
        Guild? temp = (Guild)obj!;
        FilterDefinition<Guild> filterGuild =Builders<Guild>.Filter.Eq("id", temp.id);
        await _collection.ReplaceOneAsync(filterGuild!,temp);
    }
}