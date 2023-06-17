using System.Text.Json;
using JustRPG.Services.Collections;
using JustRPG.Models;
using JustRPG.Services.Collections;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace JustRPG.Services;

public class DataBase
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;

    public readonly UserDb UserDb;
    public readonly ItemDB ItemDb;
    public readonly InventoryDB InventoryDb;
    public readonly GuildDB GuildDb;
    public readonly ActionDB ActionDb;
    public readonly LocationsDB LocationsDb;
    public readonly BattlesDB BattlesDb;
    public readonly ArenaDB ArenaDb;
    public readonly MarketDB MarketDb;
    public List<Work>? works;


    public DataBase()
    {
        _client = new MongoClient(Environment.GetEnvironmentVariable("DataBaseURL"));
        _database = _client.GetDatabase("testMMORPG");

        UserDb = new UserDb(_database);
        ItemDb = new ItemDB(_database);
        InventoryDb = new InventoryDB(_database, this);
        GuildDb = new GuildDB(_database);
        ActionDb = new ActionDB(_database);
        LocationsDb = new LocationsDB(_database);
        BattlesDb = new BattlesDB(_database);
        ArenaDb = new ArenaDB(_database);
        MarketDb = new MarketDB(_database);

        ParseWorks();
    }

    private void ParseWorks()
    {
        using StreamReader r = new StreamReader("json/works.json");
        string json = r.ReadToEnd();
        r.Close();
        works = JsonSerializer.Deserialize<List<Work>>(json);
    }
}