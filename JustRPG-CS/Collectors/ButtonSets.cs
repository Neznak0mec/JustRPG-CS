using Discord;
using JustRPG_CS.Classes;

namespace JustRPG_CS;

public static class ButtonSets
{
    public static MessageComponent ProfileButtonsSet(string finder, string toFind, string currentButton = "Profile")
    {

        var builder = new ComponentBuilder()
            .WithButton(label: "Профиль", customId: $"Profile_{finder}_{toFind}", disabled: currentButton == "Profile")
            .WithButton(label: "Экипировка", customId: $"Equipment_{finder}_{toFind}", disabled: currentButton == "Equipment")
            .WithButton(label: "Инвентарь", customId: $"Inventory_{finder}_{toFind}")
            .WithButton(label: "Прокачка навыков", customId: $"UpSkills_{finder}", disabled: finder!=toFind, row:1);
        
        return builder.Build();
    }

    public static MessageComponent UpUserSkills(string finder, User user)
    {
        int lvl = user.lvl;
        bool canUpSkills = user.skill_points > 0;
        var builder = new ComponentBuilder()
            .WithButton(label: $"{user.hp} - хп",customId:"nothing", row: 0, disabled: true)
            .WithButton(label: $"{user.defence} - броня",customId: $"UpSkill_{finder}_defence" ,row: 0, disabled: user.defence >= lvl || !canUpSkills)
            .WithButton(label: $"{user.damage} - урон",customId: $"UpSkill_{finder}_damage" , row: 1, disabled: user.damage >= lvl || !canUpSkills)
            .WithButton(label: $"{user.speed} - ловкость",customId: $"UpSkill_{finder}_speed" , row: 1, disabled: user.speed >= lvl || !canUpSkills)
            .WithButton(label: $"{user.luck} - удача",customId: $"UpSkill_{finder}_luck" , row: 2, disabled: user.luck >= lvl || !canUpSkills)
            .WithButton(label: $"{user.krit} - крит",customId: $"UpSkill_{finder}_krit" , row: 2, disabled: user.krit >= lvl || !canUpSkills)
            .WithButton(label: "К профилю",customId:$"Profile_{finder}_{finder}" ,row: 3, emote: Emoji.Parse("🔙"));

        return builder.Build();
    }
}