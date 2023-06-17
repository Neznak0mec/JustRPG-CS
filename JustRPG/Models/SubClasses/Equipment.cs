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

    public string? GetByName(string name)
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
            case ("helmet"):
                helmet = value;
                break;
            case ("armor"):
                armor = value;
                break;
            case ("pants"):
                pants = value;
                break;
            case ("shoes"):
                shoes = value;
                break;
            case ("gloves"):
                gloves = value;
                break;
            case ("weapon"):
                weapon = value;
                break;
        }
    }
}