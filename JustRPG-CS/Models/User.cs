using JustRPG.Services;

namespace JustRPG.Models;

public class User
{

    public long id { get; set; }
    public int cash { get; set; }= 0;
    public string cooldown { get; set; } = "";
    public int lvl { get; set; }= 0;
    public double exp { get; set; }= 10;
    public double exp_to_lvl { get; set; }= 100;
    public int skill_points { get; set; }= 0;
    public int damage { get; set; }= 1;
    public int defence { get; set; }= 1;
    public int speed { get; set; }= 1;
    public int krit { get; set; }= 1;
    public int luck { get; set; }= 1;
    public Equipment? equipment { get; set; }
    public string[] inventory { get; set; }= {};
    public int hp { get; set; }= 100;
    
    public record Equipment
    {
        public string? helmet { get; set; } =null;
        public string? armor { get; set; }= null;
        public string? pants { get; set; }= null;
        public string? shoes { get; set; }= null;
        public string? gloves { get; set; }=null;
        public string? weapon { get; set; }=null;
        
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