namespace JustRPG.Classes;

public record Guild
{
    public long? id { get; set; }
    public bool m_scroll { get; set; }
    public string? prefix { get; set; } = null;
    public string? language { get; set; }
}