using Discord;

namespace JustRPG_CS;

public class ButtonSets
{
    public static MessageComponent ProfileButtonsSet()
    {

        var builder = new ComponentBuilder()
            .WithButton(label: "Профиль",customId: "Profile", disabled: true);
        
        return builder.Build();
    }
}