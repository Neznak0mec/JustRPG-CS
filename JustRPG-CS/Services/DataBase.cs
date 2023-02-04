using System.Text.Json;
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

    public UserDb UserDb;
    public ItemDB ItemDb;
    public InventoryDB InventoryDb;
    public GuildDB GuildDb;
    public ActionDB ActionDb;
    public LocationsDB LocationsDb;
    public List<Work>? Works = new List<Work>();


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
        
        ParseWorks();
    }

    private void ParseWorks()
    {
        //using StreamReader r = new StreamReader("JustRPG-CS/Services/json/works.json");
        using StreamReader r = new StreamReader("json/works.json");
        string json = r.ReadToEnd();
        r.Close();
        Works = JsonSerializer.Deserialize<List<Work>>(json);
    }
}
