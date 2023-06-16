using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class MarketDB : ICollection
{
    private readonly IMongoCollection<SaleItem> _collection;
    private readonly IMongoCollection<SearchState> _interaction;
    private readonly IMongoCollection<MarketSettings> _settings;

    public MarketDB(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<SaleItem>("market");
        _interaction = mongoDatabase.GetCollection<SearchState>("interactions");
        _settings = mongoDatabase.GetCollection<MarketSettings>("interactions");
    }

    public async Task<object?> Get(object val,string key="id")
    {
        FilterDefinition<SaleItem> filterItem =Builders<SaleItem>.Filter.Eq(key, val);
        return await (await _collection.FindAsync(filterItem)).FirstOrDefaultAsync();
    }
    
    public async Task<object?> CreateObject(object? id)
    {
        await _collection.InsertOneAsync((SaleItem)id!);
        return id;
    }
    
    public async Task Update(object? obj)
    {
        SaleItem? temp = (SaleItem)obj!;
        FilterDefinition<SaleItem> filterItem =Builders<SaleItem>.Filter.Eq("id", temp.id);
        await _collection.ReplaceOneAsync(filterItem!,temp);
    }
    
    public async Task Delete(object? obj)
    {
        await _collection.DeleteOneAsync(x => x.id == ((SaleItem)obj!).id);
    }
    
    
    
    public async Task CreateSearch(SearchState search)
    {
        await _interaction.DeleteManyAsync(x=> x.userId == search.userId);
        await _interaction.InsertOneAsync(search);
    }
    
    public async Task<SearchState?> GetSearch(string userId)
    {
        return await (await _interaction.FindAsync(x=> x.userId == Convert.ToUInt64(userId))).FirstOrDefaultAsync();
    }
    
    public async Task SearchGetAndUpdate(SearchState searchState)
    {
        FilterDefinition<SaleItem> filter = Builders<SaleItem>.Filter.Empty;
    
    if (searchState.itemLvl != null)
    {
        var f1 = Builders<SaleItem>.Filter.Gt("item_lvl", searchState.itemLvl.Item1);
        var f2 = Builders<SaleItem>.Filter.Lt("item_lvl", searchState.itemLvl.Item2);
        filter = filter & f1 & f2;
    }
    if (searchState.itemRarity != null)
    {
        var f3 = Builders<SaleItem>.Filter.Eq("item_rarity", searchState.itemRarity);
        filter = filter & f3;
    }
    if (searchState.itemType != null)
    {
        var f4 = Builders<SaleItem>.Filter.Eq("item_type", searchState.itemType);
        filter = filter & f4;
    }
    
    var res = await _collection.FindAsync(filter);
    searchState.SearchResults = res.ToList();
    
    FilterDefinition<SearchState> filterItem =Builders<SearchState>.Filter.Eq("id", searchState.id);
    await _interaction.ReplaceOneAsync(filterItem,searchState);
    }
    
    
    
    public async Task CreateSettings(MarketSettings settings)
    {
        await _settings.DeleteManyAsync(x=> x.userId == settings.userId);
        await _settings.InsertOneAsync(settings);
    }
    
    public async Task<MarketSettings?> GetSettings(string userId)
    {
        return await (await _settings.FindAsync(x=> x.userId == Convert.ToUInt64(userId))).FirstOrDefaultAsync();
    }
    
    public async Task GetUserSlots(MarketSettings settings)
    {
        List<SaleItem> res = await (await _collection.FindAsync(x => x.userId == settings.userId)).ToListAsync();
        settings.SearchResults = res;
    
    FilterDefinition<MarketSettings> filterItem =Builders<MarketSettings>.Filter.Eq("id", settings.id);
    await _settings.ReplaceOneAsync(filterItem,settings);
    }
    
    public async Task<long> GetCountOfUserSlots(ulong userId){
        return (await _collection.CountDocumentsAsync(x=> x.userId == userId));
    }
}