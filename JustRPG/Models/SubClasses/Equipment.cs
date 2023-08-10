using JustRPG.Models.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace JustRPG.Models.SubClasses;

public record Equipment
{
    [BsonElement("helmet")] public string? helmet { get; set; } = null;
    [BsonElement("armor")] public string? armor { get; set; } = null;
    [BsonElement("pants")] public string? pants { get; set; } = null;
    [BsonElement("shoes")] public string? shoes { get; set; } = null;
    [BsonElement("gloves")] public string? gloves { get; set; } = null;
    [BsonElement("weapon")] public string? weapon { get; set; } = null;

    public string? GetByType(ItemType type)
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

    public void SetByType(ItemType type, string value)
    {
        switch (type)
        {
            case ItemType.helmet:
                helmet = value;
                break;
            case ItemType.armor:
                armor = value;
                break;
            case ItemType.pants:
                pants = value;
                break;
            case ItemType.shoes:
                shoes = value;
                break;
            case ItemType.gloves:
                gloves = value;
                break;
            case ItemType.weapon:
                weapon = value;
                break;
        }
    }
}