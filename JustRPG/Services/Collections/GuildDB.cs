using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class GuildDB
{
    private readonly IMongoCollection<Guild> _collection;

    public GuildDB(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<Guild>("guilds");
    }

    public async Task<Guild?> Get(object val, string key = "_id")
    {
        FilterDefinition<Guild> filterAction = Builders<Guild>.Filter.Eq(key, val);
        return await (await _collection!.FindAsync(filterAction)).FirstOrDefaultAsync();
    }

    public async Task<Guild?> CreateObject(Guild? id)
    {
        await _collection.InsertOneAsync(id!);
        return id;
    }

    public async Task Update(Guild obj)
    {
        FilterDefinition<Guild> filterGuild = Builders<Guild>.Filter.Eq("_id", obj.tag);
        await _collection.ReplaceOneAsync(filterGuild!, obj);
    }
}