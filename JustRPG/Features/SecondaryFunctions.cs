using JustRPG.Models;

namespace JustRPG.Features;

public class SecondaryFunctions
{
    public static string ProgressBar(double current, double max = 100)
    {
        int percent = (int)(current / max * 100);
        int filledBlocks = percent / 10;
        int unfilledBlocks = 9 - filledBlocks;
        string progress = "["
                      + (filledBlocks > 0 ? new string('█', filledBlocks) : "")
                      + (percent % 10 >= 5 ? '█' : '▒')
                      + (unfilledBlocks > 0 ? new string('-', unfilledBlocks) : "")
                      + $"] {current:F2}/{max:F0}";

    return progress;
    }

    public static Tuple<string,string> WarriorToStatusString(Warrior warrior, bool current)
    {
        string emoji;
        if (current)
            emoji = "⚔️";
        else if (warrior.stats.hp>0)
            emoji = "❤️";
        else
            emoji = "💀";

        return new Tuple<string, string>(emoji,$"{warrior.stats.hp/warrior.stats.MaxHP*100}%");
    }
}