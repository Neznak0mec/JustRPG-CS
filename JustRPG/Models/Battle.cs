using JustRPG.Models.Enums;

namespace JustRPG.Models;

public class Battle
{
    public string id { get; set; }
    public BattleType type { get; set; }

    public BattleStatus status { get; set; } = BattleStatus.going;
    public Warrior[] players { get; set; }
    public Warrior[] enemies { get; set; }
    public short selectedEnemy { get; set; } = 0;
    public short currentUser { get; set; } = 0;
    public long lastActivity { get; set; }
    public Dictionary<string, string> drop { get; set; }
    public List<object> originalInteraction {get;set;}
    public string log { get; set; } = "";
}

