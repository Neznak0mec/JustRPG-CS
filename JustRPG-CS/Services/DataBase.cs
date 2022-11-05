using JustRPG_CS.Classes;
using MongoDB.Bson;
using MongoDB.Driver;


namespace JustRPG_CS
{

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

        public object? GetFromDataBase<T>(T bas,string key ,object val)
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
            }

            throw new InvalidOperationException();
        }

        public User CreateUser(long id)
        {
            User newUser = new User();
            newUser.id = id;
            _userbd.InsertOne(newUser);
            return newUser;
        }

    }
}