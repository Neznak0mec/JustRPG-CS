namespace JustRPG_CS.Classes;

public record Item
{
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public int lvl { get; set;  } = 0;
    public string type { get; set; } = "";
    public int price { get; set; } = 0;
    public string description { get; set; } = "";
    public string rarity { get; set; } = "";
    public GiveStats? give_stats { get; set; } = null;
    public bool generated { get; set; } = false;
    public string preset { get; set; } = "";
    
    public class GiveStats
    {
            public int hp { get; set; } = 0;
            public int damage { get; set; } = 0;
            public int defence { get; set; } = 0;
            public int luck { get; set; } = 0;
            public int speed { get; set; } = 0;
            public int krit { get; set; } = 0;
    }
}