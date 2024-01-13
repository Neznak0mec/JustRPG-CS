using JustRPG.Models.SubClasses;
using MongoDB.Bson.Serialization.Attributes;
using Serilog;

namespace JustRPG.Models;

public class Warrior
{
    [BsonElement("userId")] public long? id { get; set; }
    [BsonElement("name")] public string name { get; set; }
    [BsonElement("full_name")] public string fullName { get; set; }
    [BsonElement("lvl")] public int lvl { get; set; }
    [BsonElement("stats")] public BattleStats stats { get; set; }
    [BsonElement("inventory")] public List<Tuple<string, BattleStats>> inventory { get; set; }
    [BsonElement("url")] public string url { get; set; }


    public double GetDamage(int startDamage, int enemySpeed)
    {

        int proc = startDamage / 5 * 100;
        double damage = startDamage + Random.Shared.Next(-proc, proc) / 100.0;

        float chance = (float)stats.speed / (stats.speed + enemySpeed) * 100;
        if (chance > 50)
            chance = 50;
        if (Random.Shared.Next(1, 100) < chance)
        {
            return -1;
        }


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
        if (Random.Shared.Next(1, 100) < (stats.luck > 60 ? 60 : stats.luck))
        {
            damage = enemy.GetDamage(stats.damage * 2, stats.speed);
            msg = damage < 0 ?
                $"{enemy.name} удалось уклониться\n" :
                $"{name} нанес критический удар {enemy.name}, тем самым нанеся {damage:f2} урона\n";
        }
        else
        {
            damage = enemy.GetDamage(stats.damage, stats.speed);
            msg = damage < 0 ?
                $"{enemy.name} удалось уклониться\n" :
                $"{name} нанёс {damage:f2} урона по {enemy.name}\n";
        }

       


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