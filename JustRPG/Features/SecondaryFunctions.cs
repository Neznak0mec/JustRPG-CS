using JustRPG.Models;

namespace JustRPG.Features;

public static class SecondaryFunctions
{
    private static Random rng = new Random();  
    
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

    public static Tuple<string, string> WarriorToStatusString(Warrior warrior, bool current)
    {
        string emoji;
        if (current)
            emoji = "⚔️";
        else if (warrior.stats.hp > 0)
            emoji = "❤️";
        else
            emoji = "💀";

        return new Tuple<string, string>(emoji, $"{warrior.stats.hp / warrior.stats.MaxHP * 100}%");
    }

    public static Tuple<string, string>? GetRandomKeyValuePair(Dictionary<string, string> dictionary)
    {
        if (dictionary.Count == 0)
        {
            return null;
        }

        var random = new Random();
        int randomIndex = random.Next(0, dictionary.Count);
        KeyValuePair<string, string> randomPair = new KeyValuePair<string, string>();

        int currentIndex = 0;
        foreach (var kv in dictionary)
        {
            if (currentIndex == randomIndex)
            {
                randomPair = kv;
                break;
            }

            currentIndex++;
        }

        return new Tuple<string, string>(randomPair.Key, randomPair.Value);
    }
    
    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            (list[k], list[n]) = (list[n], list[k]);
        }  
    }
}