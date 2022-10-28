using Discord;

namespace JustRPG_CS;

public class ButtonSets
{
    public static MessageComponent ProfileButtonsSet(string finder, string toFind, bool fromButton = false)
    {

        var builder = new ComponentBuilder()
            .WithButton(label: "Профиль", customId: $"Profile_{finder}_{toFind}", disabled: !fromButton)
            .WithButton(label: "Экипировка", customId: $"Equipment_{finder}_{toFind}")
            .WithButton(label: "Инвентарь", customId: $"Inventory_{finder}_{toFind}")
            .WithButton(label: "Прокачка навыков", customId: $"UpSkills_{finder}", disabled: finder!=toFind, row:1);
        
        return builder.Build();
    }
}