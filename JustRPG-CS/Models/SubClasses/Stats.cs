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

    public BattleStats(){}

    [BsonElement("maxHP")] public double MaxHP { get; set; }
    [BsonElement("maxDef")]  public double MaxDef { get; set; }
}