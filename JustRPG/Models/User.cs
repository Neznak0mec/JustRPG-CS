using JustRPG.Models.SubClasses;
using JustRPG.Services;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class User
{
    [BsonElement("_id")] public long id { get; init; }
    [BsonElement("cash")] public int cash { get; set; } = 0;
    [BsonElement("lvl")] public int lvl { get; set; } = 0;
    [BsonElement("exp")] public double exp { get; set; } = 0;
    [BsonElement("mmr")] public int mmr { get; set; } = 0;
    [BsonElement("exp_to_lvl")] public double expToLvl { get; set; } = 100;
    [BsonElement("stats")] public Stats stats { get; set; } = new();
    [BsonElement("inventory")] public List<string> inventory { get; set; } = new();
    [BsonElement("equipment")] public Equipment? equipment { get; set; } = new();
    [BsonElement("guild_tag")] public string? guildTag { get; set; } = null;

    public async Task<UserEquipment> GetEquipmentAsItems(DataBase dataBase)
    {
        UserEquipment res = new UserEquipment(
            helmet: equipment?.helmet == null ? null : (Item)(await dataBase.ItemDb.Get(equipment.helmet))!,
            armor: equipment?.armor == null ? null : (Item)(await dataBase.ItemDb.Get(equipment.armor))!,
            pants: equipment?.pants == null ? null : (Item)(await dataBase.ItemDb.Get(equipment.pants))!,
            shoes: equipment?.shoes == null ? null : (Item)(await dataBase.ItemDb.Get(equipment.shoes))!,
            gloves: equipment?.gloves == null ? null : (Item)(await dataBase.ItemDb.Get(equipment.gloves))!,
            weapon: equipment?.weapon == null ? null : (Item)(await dataBase.ItemDb.Get(equipment.weapon))!
        );

        return res;
    }
}