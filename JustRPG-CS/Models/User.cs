namespace JustRPG_CS.Classes;

public record User
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
    public Equipment? equipment { get; set; }= null;
    public List<string> inventory { get; set; }= new List<string>();
    public int hp { get; set; }= 100;
    
    
    public class Equipment
    {
        public string helmet { get; set; }="";
        public string armor { get; set; }= "";
        public string pants { get; set; }= "";
        public string shoes { get; set; }= "";
        public string gloves { get; set; }="";
        public string weapon { get; set; }="";
    }
}