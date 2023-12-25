using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;
using Serilog;

namespace JustRPG.Services.Collections;
//todo: switch IMongoDatabase to local cache
public class InventoryDB : ICollection
{
    private readonly IMongoCollection<Inventory> _collection;
    private readonly DataBase _dataBase;

    public InventoryDB(IMongoDatabase mongoDatabase, DataBase dataBase)
    {
        _dataBase = dataBase;
        _collection = mongoDatabase.GetCollection<Inventory>("interactions");
    }

    public async Task<object?> Get(object val, string key = "id")
    {
        FilterDefinition<Inventory> filterInventory = Builders<Inventory>.Filter.Eq(key, val);

        if (key != "id")
        {
            var res = await _collection.FindAsync(filterInventory);
            return res.FirstOrDefault();
        }


        Inventory? temp = new Inventory
        {
            id = val.ToString()
        };
        if (await _collection.CountDocumentsAsync(x => x!.id == temp.id) > 0)
        {
            var res = await _collection.FindAsync(filterInventory);
            temp = res.FirstOrDefault();
        }
        else
            temp = (Inventory)(await CreateObject(temp))!;

        TimeSpan aTimeSpan = new TimeSpan(0, 0, 5, 0);
        if (((DateTimeOffset)temp!.lastUsage).ToUnixTimeSeconds() < DateTimeOffset.Now.Subtract(aTimeSpan).ToUnixTimeSeconds())
        {
            temp = (Inventory)(await CreateObject(temp))!;
        }

        return temp;
    }

    public async Task<object?> CreateObject(object? id)
    {
        var temp = (Inventory)id!;

        if (await _collection.CountDocumentsAsync(x => x!.id == temp.id) > 0)
            await _collection.ReplaceOneAsync(x => x!.id == temp.id, temp);
        else
            await _collection.InsertOneAsync(temp);

        string userid = temp.id!.Split('_')[2];
        User user = (User)(await _dataBase.UserDb.Get(userid))!;
        await temp.Reload(user.inventory, _dataBase);
        await temp.Save(_dataBase);

        return temp;
    }

    public async Task Update(object? obj)
    {
        Inventory? temp = (Inventory)obj!;
        FilterDefinition<Inventory> filterInventory = Builders<Inventory>.Filter.Eq("id", temp.id)!;
        await _collection.ReplaceOneAsync(filterInventory, temp);
    }
}