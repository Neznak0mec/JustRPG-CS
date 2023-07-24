using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class GuildDB : ICollection
{
    private readonly IMongoCollection<Guild> _collection;

    public GuildDB(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<Guild>("guilds");
    }

    public async Task<object?> Get(object val, string key = "_id")
    {
        FilterDefinition<Guild> filterAction = Builders<Guild>.Filter.Eq(key, val);
        return await (await _collection!.FindAsync(filterAction)).FirstOrDefaultAsync();
    }

    public async Task<object?> CreateObject(object? id)
    {
        await _collection.InsertOneAsync((Guild)id!);
        return id;
    }

    public async Task Update(object? obj)
    {
        Guild? temp = (Guild)obj!;
        FilterDefinition<Guild> filterGuild = Builders<Guild>.Filter.Eq("id", temp.tag);
        await _collection.ReplaceOneAsync(filterGuild!, temp);
    }
}