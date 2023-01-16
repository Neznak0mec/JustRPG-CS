using JustRPG.Services;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models;

public class User
{

    [BsonElement("_id")] public long id { get; set; }
    [BsonElement("cash")]public int cash { get; set; }= 0;
    [BsonElement("lvl")]public int lvl { get; set; }= 0;
    [BsonElement("exp")]public double exp { get; set; }= 10;
    [BsonElement("exp_to_lvl")]public double expToLvl { get; set; }= 100;
    [BsonElement("skill_points")]public int skillPoints { get; set; }= 0;
    [BsonElement("stats")]public Stats stats { get; set; }
    [BsonElement("inventory")]public string[] inventory { get; set; }= Array.Empty<string>();
    [BsonElement("equipment")]public Equipment? equipment { get; set; }
    

    public UserEquipment GetEquipmentAsItems(DataBase dataBase)
    {
        
        UserEquipment res = new UserEquipment(
            helmet: equipment?.helmet == null ? null :(Item)dataBase.ItemDb.Get(equipment.helmet)!,
            armor:  equipment?.armor == null ?  null :(Item)dataBase.ItemDb.Get(equipment.armor)!,
            pants:  equipment?.pants == null ?  null :(Item)dataBase.ItemDb.Get(equipment.pants)!,
            shoes:  equipment?.shoes == null ?  null :(Item)dataBase.ItemDb.Get(equipment.shoes)!,
            gloves: equipment?.gloves == null ? null :(Item)dataBase.ItemDb.Get(equipment.gloves)!,
            weapon: equipment?.weapon == null ? null :(Item)dataBase.ItemDb.Get(equipment.weapon)!
            );

        return res;
    }
    
}
public record Equipment
{
    [BsonElement("helmet")]public string? helmet { get; set; } =null;
    [BsonElement("armor")]public string? armor { get; set; }= null;
    [BsonElement("pants")]public string? pants { get; set; }= null;
    [BsonElement("shoes")]public string? shoes { get; set; }= null;
    [BsonElement("gloves")]public string? gloves { get; set; }=null;
    [BsonElement("weapon")]public string? weapon { get; set; }=null;
        
    public string? GetByName ( string name )
    {
        return name switch
        {
            ("helmet") => helmet,
            ("armor") => armor,
            ("pants") => pants,
            ("shoes") => shoes,
            ("gloves") => gloves,
            ("weapon") => weapon,
            _ => null
        };
    }

    public void SetByName(string name, string value)
    {
        switch (name)
        {
            case("helmet"):
                helmet= value;
                break;
            case("armor"):
                armor= value;
                break;
            case("pants"):
                pants= value;
                break;
            case("shoes"):
                shoes= value;
                break;
            case("gloves"):
                gloves= value;
                break;
            case("weapon"):
                weapon= value;
                break;
        }
    }
}

public class UserEquipment
{
    public Item? helmet = null;
    public Item? armor = null;
    public Item? pants = null;
    public Item? shoes = null;
    public Item? gloves = null;
    public Item? weapon = null;

    public UserEquipment(Item? helmet, Item? armor, Item? pants, Item? shoes, Item? gloves, Item? weapon)
    {
        this.helmet = helmet;
        this.armor = armor;
        this.pants = pants;
        this.shoes = shoes;
        this.gloves = gloves;
        this.weapon = weapon;
    }
    
}