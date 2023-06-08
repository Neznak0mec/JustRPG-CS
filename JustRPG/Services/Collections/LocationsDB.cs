using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class LocationsDB
{
    private readonly IMongoCollection<Location> _collection;

    public LocationsDB(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<Location>("info");
    }
    
    public async Task<List<Location>> GetDungeons()
    {
        var filterAction =Builders<Location>.Filter.Eq("type", "dungeon");
        return (await _collection.FindAsync(filterAction)).ToList();
    }
    
    public List<Location> GetAdventuresLocations()
    {
        var filterAction =Builders<Location>.Filter.Eq("type", "adventure");  
        return _collection.Find(filterAction).ToList();
    }
    
    public Location Get(string val,string key="_id")
    {
        var filterItem =Builders<Location>.Filter.Eq(key, val);  
        return _collection.Find(filterItem).FirstOrDefault();
    }
}