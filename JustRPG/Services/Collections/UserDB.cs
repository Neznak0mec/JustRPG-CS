using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class UserDb : ICollection
{
    private readonly IMongoCollection<User?> _collection;
    private readonly List<ulong> usersIdCache;

    public UserDb(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<User>("users")!;
        usersIdCache = new List<ulong>();
    }

    public async Task<object?> Get(object val, string key = "id")
    {
        FilterDefinition<User> filterUser = Builders<User>.Filter.Eq(key, val);
        return await (await _collection!.FindAsync(filterUser)).FirstOrDefaultAsync();
    }

    public async Task<object?> CreateObject(object? id)
    {
        User? newUser = new User { id = Convert.ToInt64(id) };
        await _collection.InsertOneAsync(newUser);
        return newUser;
    }

    public async Task Update(object? obj)
    {
        User temp = (User)obj!;
        FilterDefinition<User> filterUser = Builders<User>.Filter.Eq("id", temp.id);
        await _collection.ReplaceOneAsync(filterUser!, temp);
    }

    public async Task Cache(ulong userId)
    {
        if (usersIdCache.Contains(userId))
            return;

        var user = Get(userId);
        if (user == null)
            await CreateObject(userId);

        usersIdCache.Add(userId);
    }
}