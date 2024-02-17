using JustRPG.Interfaces;
using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class MarketDB
{
    private readonly IMongoCollection<SaleItem> _collection;
    private readonly IMongoCollection<MarketSearchState> _interaction;
    private readonly IMongoCollection<MarketSlotsSettings> _settings;

    public MarketDB(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<SaleItem>("market");
        _interaction = mongoDatabase.GetCollection<MarketSearchState>("interactions");
        _settings = mongoDatabase.GetCollection<MarketSlotsSettings>("interactions");
    }

    public async Task<SaleItem?> Get(object val, string key = "id")
    {
        FilterDefinition<SaleItem> filterItem = Builders<SaleItem>.Filter.Eq(key, val);
        return await (await _collection.FindAsync(filterItem)).FirstOrDefaultAsync();
    }

    public async Task<SaleItem?> CreateObject(SaleItem? id)
    {
        await _collection.InsertOneAsync((SaleItem)id!);
        return id;
    }

    public async Task Update(object? obj)
    {
        SaleItem? temp = (SaleItem)obj!;
        FilterDefinition<SaleItem> filterItem = Builders<SaleItem>.Filter.Eq("id", temp.id);
        await _collection.ReplaceOneAsync(filterItem!, temp);
    }

    public async Task Delete(object? obj)
    {
        await _collection.DeleteOneAsync(x => x.id == ((SaleItem)obj!).id);
    }


    public async Task CreateSearch(MarketSearchState search)
    {
        var temp = await GetSearch(search.userId.ToString());
        if(temp != null)
        {
            search.id = temp.id;
            await _interaction.ReplaceOneAsync(x => x.userId == search.userId, search);
        }
        else
            await _interaction.InsertOneAsync(search);
        
    }

    public async Task<MarketSearchState?> GetSearch(string userId)
    {
        return await (await _interaction.FindAsync(x => x.userId == Convert.ToUInt64(userId))).FirstOrDefaultAsync();
    }

    public async Task SearchGetAndUpdate(MarketSearchState searchState)
    {
        FilterDefinition<SaleItem> filter = Builders<SaleItem>.Filter.Empty;

        if (searchState.itemLvl != null)
        {
            var f1 = Builders<SaleItem>.Filter.Gte(x=> x.itemLvl, searchState.itemLvl.Item1);
            var f2 = Builders<SaleItem>.Filter.Lte(x=> x.itemLvl, searchState.itemLvl.Item2);
            filter &= f1 & f2;
        }
        else
        {
            var f1 = Builders<SaleItem>.Filter.Gte(x=> x.itemLvl, 0);
            filter &= f1;
        }

        if (searchState.itemRarity != null)
        {
            var f3 = Builders<SaleItem>.Filter.Eq("item_rarity", searchState.itemRarity);
            filter &= f3;
        }
        
        if (searchState.itemType != null)
        {
            var f4 = Builders<SaleItem>.Filter.Eq("item_type", searchState.itemType);
            filter &= f4;
        }

        var f5 = Builders<SaleItem>.Filter.Eq(x => x.isVisible, true);
        filter &= f5;

        var res = await _collection.FindAsync(filter);
        searchState.Items = res.ToList();

        FilterDefinition<MarketSearchState> filterItem = Builders<MarketSearchState>.Filter.Eq("id", searchState.id);
        await _interaction.ReplaceOneAsync(filterItem, searchState);
    }


    public async Task CreateSettings(MarketSlotsSettings settings)
    {
        await _settings.DeleteManyAsync(x => x.userId == settings.userId);
        await _settings.InsertOneAsync(settings);
    }

    public async Task<MarketSlotsSettings?> GetSettings(string userId)
    {
        return await (await _settings.FindAsync(x => x.userId == Convert.ToUInt64(userId))).FirstOrDefaultAsync();
    }

    public async Task GetUserSlots(MarketSlotsSettings settings)
    {
        List<SaleItem> res = await (await _collection.FindAsync(x => x.userId == settings.userId)).ToListAsync();
        settings.searchResults = res;

        FilterDefinition<MarketSlotsSettings> filterItem = Builders<MarketSlotsSettings>.Filter.Eq("id", settings.id);
        await _settings.ReplaceOneAsync(filterItem, settings);
    }

    public async Task<long> GetCountOfUserSlots(ulong userId)
    {
        return (await _collection.CountDocumentsAsync(x => x.userId == userId));
    }
}