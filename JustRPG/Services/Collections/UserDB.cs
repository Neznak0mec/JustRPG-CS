using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class UserDb
{
    private readonly IMongoCollection<User?> _collection;
    private readonly List<ulong> _usersIdCache;

    public UserDb(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<User>("users")!;
        _usersIdCache = new List<ulong>();
    }

    public async Task<User?> Get(object val, string key = "id")
    {
        FilterDefinition<User> filterUser = Builders<User>.Filter.Eq(key, val);
        return await (await _collection!.FindAsync(filterUser)).FirstOrDefaultAsync();
    }

    public async Task<User?> CreateObject(object? id)
    {
        User newUser = new User { id = Convert.ToInt64(id) };
        await _collection.InsertOneAsync(newUser);
        return newUser;
    }

    public async Task Update(object? obj)
    {
        User temp = (User)obj!;

        if (temp.expToLvl < temp.exp)
        {
            temp.exp -= temp.expToLvl;
            temp.lvl++;
            temp.expToLvl += temp.expToLvl / 3;
        }

        if (temp.exp < 0)
            temp.exp = 0;
        
        if (temp.cash < 0)
            temp.cash = 0;

        FilterDefinition<User> filterUser = Builders<User>.Filter.Eq("id", temp.id);
        await _collection.ReplaceOneAsync(filterUser!, temp);
    }

    public async Task Cache(ulong userId)
    {
        if (_usersIdCache.Contains(userId))
            return;

        if (await Get(userId) == null)
            await CreateObject(userId);

        _usersIdCache.Add(userId);
    }

    public List<User> GetTopMMR()
    {
        var sortDefinition = Builders<User>.Sort.Descending(doc => doc.mmr);
        return _collection.Find(FilterDefinition<User>.Empty).Sort(sortDefinition).Limit(10).ToList();
    }
}