using JustRPG.Models.Enums;

namespace JustRPG.Models.SubClasses;

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

    public Item? GetEquippedItemByType(ItemType type)
    {
        return type switch
        {
            ItemType.helmet => helmet,
            ItemType.armor => armor,
            ItemType.pants => pants,
            ItemType.shoes => shoes,
            ItemType.gloves => gloves,
            ItemType.weapon => weapon,
            _ => null
        };
    }
}