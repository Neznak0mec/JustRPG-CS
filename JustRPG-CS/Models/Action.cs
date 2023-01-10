namespace JustRPG.Models;

public class Action
{
    public string id { get; set; }
    public string[] args { get; set; }
    
    public string type { get; set; }
    public long userId { get; set; }
    public long date { get; set; }
}