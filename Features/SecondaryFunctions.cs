namespace JustRPG.Features;

public class SecondaryFunctions
{
    public static string ProgressBar(double current, double max = 100)
    {
        string res = "[";
        double proc = current / max * 100;
        int numOfBlack = (int)(proc / 10) - 1;
        res += new string('█', numOfBlack);
        res += (int)proc % 10 >= 5 ? "█" : "▒";
        int numOfWhite = 10 - numOfBlack - 1;
        res += new string('-', numOfWhite);
        return res + "]" + $"- {current:F2}/{max:F0}";
    }
}