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
    [BsonElement("last_activity")] public long lastActivity{ get; set; }
    [BsonElement("drop")] public Dictionary<string,string> drop{ get; set; }
    [BsonIgnore]public string log{ get; set; } = "";
}

public class Warrior
{
    [BsonElement("userId")] public long? id {get;set;}
    [BsonElement("name")] public string name{ get; set; }
    [BsonElement("lvl")]public int lvl{ get; set; }
    [BsonElement("stats")]public BattleStats stats{ get; set; }
    [BsonElement("inventory")]public List<Tuple<string, BattleStats>> inventory{ get; set; }
    [BsonElement("url")]public string url{ get; set; }


    public double GetDamage(int startDamage)
    {
        int proc = startDamage / 5 * 100;
        double damage = startDamage + Random.Shared.Next(-proc,proc)/100.0;

        if (Random.Shared.Next(1, 100) < 1 + stats.luck)
            return -1;


        if (stats.defence > 0)
        {
            stats.defence -= damage;
            if (stats.defence < 0)
            {
                stats.hp += stats.defence;
                stats.defence = 0;
            }
        }
        else
            stats.hp -= damage;

        return damage;
    }

    public void Attack(Battle? battle,Warrior enemy)
    {
        double damage;
        string msg;
        if (Random.Shared.Next(1, 100) < 1 + stats.luck){
            damage = enemy.GetDamage(stats.damage*2);
            msg = $"{name} нанес критический удар {enemy.name}, тем самым нанеся {damage:f2} урона\n";
        }
        else{
            damage = enemy.GetDamage(stats.damage);
            msg = $"{name} нанес {damage:f2} урона по {enemy.name}\n";
        }

        if (damage == -1)
            msg = $"{enemy.name} удалось укланиться\n";


        battle!.log+=msg;
    }

    public double Heal()
    {
        double procents = stats.MaxHP / 4;
        stats.hp += procents;
        if (stats.hp > stats.MaxHP)
            stats.hp = stats.MaxHP;
        return procents;
    }

}