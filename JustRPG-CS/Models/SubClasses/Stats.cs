using JustRPG.Services;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models.SubClasses;

public class Stats
{
    [BsonElement("hp")]public double hp { get; set; }
    [BsonElement("damage")]public int damage { get; set; }
    [BsonElement("defence")]public double defence { get; set; }
    [BsonElement("luck")]public int luck { get; set; }
    [BsonElement("speed")]public int speed { get; set; }
}

public class BattleStats : Stats
{
    public BattleStats(Stats stats)
    {
            hp = stats.hp;
            damage = stats.damage;
            defence = stats.defence;
            luck = stats.luck;
            MaxDef = stats.defence;
            MaxHP = stats.hp;
            speed = stats.speed;
    }

    public BattleStats(User user, DataBase dataBase)
    {
        hp = user.stats.hp;
        damage = user.stats.damage;
        defence = user.stats.defence;
        luck = user.stats.luck;
        MaxDef = user.stats.defence;
        MaxHP = user.stats.hp;
        speed = user.stats.speed;

        UserEquipment eq = user.GetEquipmentAsItems(dataBase);
        Item?[] items = {eq.armor, eq.pants,eq.shoes,eq.gloves,eq.helmet,eq.weapon};
        foreach (var i in items)
        {
            if (i == null) continue;



            hp += i.giveStats.hp;
            damage += i.giveStats.damage;
            defence += i.giveStats.defence;
            luck += i.giveStats.luck;
            MaxDef += i.giveStats.defence;
            MaxHP += i.giveStats.hp;
            speed += i.giveStats.speed;
        }
    }

    public BattleStats(){}

    [BsonElement("maxHP")] public double MaxHP { get; set; }
    [BsonElement("maxDef")]  public double MaxDef { get; set; }
}