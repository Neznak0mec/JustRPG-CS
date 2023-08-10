using Discord;
using Discord.Rest;
using Discord.WebSocket;
using JustRPG.Models.Enums;
using JustRPG.Models.SubClasses;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class Battle
{
    public string id { get; set; }
    public BattleType type { get; set; }

    public BattleStatus status { get; set; } = BattleStatus.going;
    public Warrior[] players { get; set; }
    public Warrior[] enemies { get; set; }
    public short selectedEnemy { get; set; } = 0;
    public short currentUser { get; set; } = 0;
    public long lastActivity { get; set; }
    public Dictionary<string, string> drop { get; set; }
    public List<object> originalInteraction {get;set;}
    public string log { get; set; } = "";
}

public class Warrior
{
    [BsonElement("userId")] public long? id { get; set; }
    [BsonElement("name")] public string name { get; set; }
    [BsonElement("lvl")] public int lvl { get; set; }
    [BsonElement("stats")] public BattleStats stats { get; set; }
    [BsonElement("inventory")] public List<Tuple<string, BattleStats>> inventory { get; set; }
    [BsonElement("url")] public string url { get; set; }


    public double GetDamage(int startDamage)
    {

        int proc = startDamage / 5 * 100;
        double damage = startDamage + Random.Shared.Next(-proc, proc) / 100.0;

        if (Random.Shared.Next(1, 100) < (stats.speed > 75 ? 75 : stats.speed))
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

    public void Attack(Battle battle, Warrior enemy)
    {
        double damage;
        string msg;
        if (Random.Shared.Next(1, 100) < 1 + (stats.luck > 75 ? 75 : stats.luck))
        {
            damage = enemy.GetDamage(stats.damage * 2);
            msg = $"{name} нанес критический удар {enemy.name}, тем самым нанеся {damage:f2} урона\n";
        }
        else
        {
            damage = enemy.GetDamage(stats.damage);
            msg = $"{name} нанёс {damage:f2} урона по {enemy.name}\n";
        }

        if (damage == -1)
            msg = $"{enemy.name} удалось уклониться\n";


        battle.log += msg;
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