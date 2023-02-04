using JustRPG.Models.SubClasses;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class Battle
{
    [BsonElement("_id")] public string id{ get; set; }
    [BsonElement("type")] public string type{ get; set; }
    [BsonElement("players")] public Warrior[] players{ get; set; }
    [BsonElement("enemies")] public Warrior[] enemies{ get; set; }
    [BsonElement("selected_enemy")] public short selectedEnemy{ get; set; } = 0;
    [BsonElement("current_user")] public short currentUser{ get; set; } = 0;
//    [BsonElement("timeout_on")] public long timeoutOn{ get; set; }
    [BsonElement("drop")] public Dictionary<string,int> drop{ get; set; }
    [BsonIgnore]public string log{ get; set; }
}

public class Warrior
{
    [BsonElement("name")] public string name{ get; set; }
    [BsonElement("lvl")]public int lvl{ get; set; }
    [BsonElement("stats")]public BattleStats stats{ get; set; }
    [BsonElement("inventory")]public List<Tuple<string, BattleStats>> inventory{ get; set; }
    [BsonElement("url")]public string url{ get; set; }

}