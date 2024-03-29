using System.Text.Json;
using DotNetEnv;
using JustRPG.Services.Collections;
using JustRPG.Models;
using MongoDB.Driver;

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
        _client = new MongoClient(Env.GetString("DataBaseURL"));
        # if DEBUG
        _database = _client.GetDatabase("testMMORPG");
        #else
        _database = _client.GetDatabase("MMORPG");
        #endif

        UserDb = new UserDb(_database);
        ItemDb = new ItemDB(_database);
        InventoryDb = new InventoryDB(this);
        GuildDb = new GuildDB(_database);
        ActionDb = new ActionDB(_database);
        LocationsDb = new LocationsDB(_database);
        BattlesDb = new BattlesDB();
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