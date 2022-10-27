using System.Reflection;
using JustRPG_CS.Classes;
using MongoDB.Bson;
using MongoDB.Driver;


class MyType
{
    public IMongoCollection<User> UserDB{get; set;}
    public IMongoCollection<Guid> GuildDB{get; set;}
    public IMongoCollection<Item> ItemDB {get; set;}
    private readonly IMongoCollection<BsonDocument> InfoDB;
}


namespace JustRPG_CS
{
    using MongoDB.Bson;
    using MongoDB.Driver;

    public enum Bases
    {
        Users,
        Guilds,
        Items,
        Info
    }

    public class DataBase
    {
        private readonly MongoClient _client;

        private readonly IMongoDatabase _database;

        private readonly IMongoCollection<User> _userbd;
        private readonly IMongoCollection<Guild> _guildsb;
        private readonly IMongoCollection<Item> _itemsbd;
        private readonly IMongoCollection<BsonDocument> _infodb;


        public DataBase()
        {
            
            _client = new MongoClient(Environment.GetEnvironmentVariable("DataBaseURL"));
            _database = _client.GetDatabase("testMMORPG");

            _userbd =  _database.GetCollection<User>("users");
            _guildsb = _database.GetCollection<Guild>("servers");
            _itemsbd =  _database.GetCollection<Item>("items");
            _infodb = _database.GetCollection<BsonDocument>("info");

        }

        public object? GetFromDataBase<T>(T bas, object val)
        {
            switch (bas)
            {
                case Bases.Users:
                    return _userbd.Find(x => x.id == Convert.ToInt64( val)).FirstOrDefault();
                case Bases.Guilds:
                    return _guildsb.Find(x => x.id == Convert.ToInt64(val)).FirstOrDefault();
                case Bases.Items:
                    return _itemsbd.Find(x => x.id == val.ToString()).FirstOrDefault();
            }

            throw new InvalidOperationException();
        }

    }
}