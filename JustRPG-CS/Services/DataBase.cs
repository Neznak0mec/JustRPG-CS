using JustRPG.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace JustRPG.Services
{

    public enum Bases
    {
        Users,
        Guilds,
        Items,
        Info,
        Interactions
    }

    public class DataBase
    {
        private readonly MongoClient _client;

        private readonly IMongoDatabase _database;

        private readonly IMongoCollection<User> _userbd;
        private readonly IMongoCollection<Guild> _guildsb;
        private readonly IMongoCollection<Item> _itemsbd;
        private readonly IMongoCollection<BsonDocument> _infodb;
        private readonly IMongoCollection<Inventory> _inventoryInteraction;


        public DataBase()
        {
            _client = new MongoClient(Environment.GetEnvironmentVariable("DataBaseURL"));
            _database = _client.GetDatabase("testMMORPG");

            _userbd =  _database.GetCollection<User>("users");
            _guildsb = _database.GetCollection<Guild>("servers");
            _itemsbd =  _database.GetCollection<Item>("items");
            _infodb = _database.GetCollection<BsonDocument>("info");
            _inventoryInteraction = _database.GetCollection<Inventory>("interactions");

        }

        public object? GetFromDataBase(Bases bas,string key ,object val)
        {
            
            switch (bas)
            {
                case Bases.Users:
                    var filterUser =Builders<User>.Filter.Eq(key, val);  
                    return _userbd.Find(filterUser).FirstOrDefault();
                case Bases.Guilds:
                    var filterGuild =Builders<Guild>.Filter.Eq(key, val);  
                    return _guildsb.Find(filterGuild).FirstOrDefault();
                case Bases.Items:
                    var filterItem =Builders<Item>.Filter.Eq(key, val);  
                    return _itemsbd.Find(filterItem).FirstOrDefault();
                case Bases.Interactions:
                    var filretInteraction = Builders<Inventory>.Filter.Eq(key, val);
                    return _inventoryInteraction.Find(filretInteraction).FirstOrDefault();
            }

            throw new InvalidOperationException();
        }

        public User CreateUser(long id)
        {
            User newUser = new User{id  = id};
            _userbd.InsertOne(newUser);
            return newUser;
        }

        public Inventory CreateInventory(User user,string finder)
        {
            Inventory newInventory = new Inventory()
            {
                id = $"Inventory_{user.id}_{finder}"
            };
            newInventory.Reload(user.inventory);
            
            if (_inventoryInteraction.CountDocuments(x => x.id == newInventory.id) > 0)
                _inventoryInteraction.ReplaceOne(x => x.id == newInventory.id, newInventory);
            else
                _inventoryInteraction.InsertOne(newInventory);
            
            return newInventory;
        }

        public object? Add(Bases bas,string key ,object val, string field, int add)
        {
            switch (bas)
            {
                case Bases.Users:
                    var filterUser =Builders<User>.Filter.Eq(key, val);
                    var updateUser = Builders<User>.Update.Inc(field, add);
                    return _userbd.UpdateOne(filterUser,updateUser);
                case Bases.Guilds:
                    var filterGuild =Builders<Guild>.Filter.Eq(key, val);  
                    var updateGuild = Builders<Guild>.Update.Inc(field, add);
                    return _guildsb.UpdateOne(filterGuild,updateGuild);
                case Bases.Items:
                    var filterItem =Builders<Item>.Filter.Eq(key, val);  
                    var updateItem = Builders<Item>.Update.Inc(field, add);
                    return _itemsbd.UpdateOne(filterItem,updateItem);
                default:
                    return null;
            }
        }

        public object? Set(Bases bas,string key ,object val, string field, int add)
        {
            switch (bas)
            {
                case Bases.Users:
                    var filterUser =Builders<User>.Filter.Eq(key, val);
                    var updateUser = Builders<User>.Update.Set(field, add);
                    return _userbd.UpdateOne(filterUser,updateUser);
                case Bases.Guilds:
                    var filterGuild =Builders<Guild>.Filter.Eq(key, val);  
                    var updateGuild = Builders<Guild>.Update.Set(field, add);
                    return _guildsb.UpdateOne(filterGuild,updateGuild);
                case Bases.Items:
                    var filterItem =Builders<Item>.Filter.Eq(key, val);  
                    var updateItem = Builders<Item>.Update.Set(field, add);
                    return _itemsbd.UpdateOne(filterItem,updateItem);
            }
            throw new InvalidOperationException();
        }
    }
}