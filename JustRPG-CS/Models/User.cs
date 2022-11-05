
namespace JustRPG_CS.Classes;

public class User
{

    public long id { get; set; } = 0;
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
    public List<string> inventory { get; set; }= new List<string>();
    public int hp { get; set; }= 100;
    
    public record Equipment
    {
        public string? helmet { get; set; } =null;
        public string? armor { get; set; }= null;
        public string? pants { get; set; }= null;
        public string? shoes { get; set; }= null;
        public string? gloves { get; set; }=null;
        public string? weapon { get; set; }=null;
    }

    public UserEquipment GetEquipmentAsItems(DataBase dataBase)
    {
        
        UserEquipment res = new UserEquipment(
            helmet_: equipment?.helmet == null ? null :(Item)dataBase.GetFromDataBase(Bases.Items,"id" ,equipment.helmet)!,
            armor_:  equipment?.armor == null ?  null :(Item)dataBase.GetFromDataBase(Bases.Items,"id" ,equipment.armor)!,
            pants_:  equipment?.pants == null ?  null :(Item)dataBase.GetFromDataBase(Bases.Items,"id" ,equipment.pants)!,
            shoes_:  equipment?.shoes == null ?  null :(Item)dataBase.GetFromDataBase(Bases.Items,"id" ,equipment.shoes)!,
            gloves_: equipment?.gloves == null ? null :(Item)dataBase.GetFromDataBase(Bases.Items,"id" ,equipment.gloves)!,
            weapon_: equipment?.weapon == null ? null :(Item)dataBase.GetFromDataBase(Bases.Items,"id" ,equipment.weapon)!
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

    public UserEquipment(Item? helmet_, Item? armor_, Item? pants_, Item? shoes_, Item? gloves_, Item? weapon_)
    {
        helmet = helmet_;
        armor = armor_;
        pants = pants_;
        shoes = shoes_;
        gloves = gloves_;
        weapon = weapon_;
    }
    
}