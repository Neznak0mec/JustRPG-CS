using Discord;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Generators;

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

    public static MessageComponent InventoryButtonsSet(string finder, User user, Inventory inventory,Item?[] items)
    {
        
        
        var select = new SelectMenuBuilder()
            .WithPlaceholder(
                $"Тип взаимодействия: {(inventory.interactionType == "info" ? "информация" : inventory.interactionType == "sell" ? "продажа" : "экипировать")}")
            .WithCustomId($"InvInteractionType_{finder}_{user.id}")
            .AddOption("Информация", "info")
            .AddOption("Экипировать", "equip")
            .AddOption("Продать", "sell");

        var builder = new ComponentBuilder()
            .WithButton(label: "⮘", customId: $"InvPrewPage_{finder}_{user.id}", disabled: inventory.currentPage == 0, row: 0)
            .WithButton(label: $"{inventory.currentPage+1}/{inventory.lastPage+1}", customId: "none1", disabled: true, row: 0)
            .WithButton(label: "➣", customId: $"InvNextPage_{finder}_{user.id}", disabled: inventory.currentPage >= inventory.lastPage, row: 0)
            .WithButton(label: "♺", customId: $"InvReload_{finder}_{user.id}")
            .WithSelectMenu(select, row: 1);

        ButtonStyle style = inventory.interactionType == "info" ? ButtonStyle.Primary : inventory.interactionType == "sell" ? ButtonStyle.Danger : ButtonStyle.Success;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                builder.WithButton(label: $"{i+1}", customId: $"null{Random.Shared.Next()}", style: style, disabled: true,row: 2);
                continue;
            }
            
            if (inventory.interactionType == "info")
            {
                builder.WithButton(label: $"{i+1}", customId: $"InvInfo{i}_{finder}_{user.id}", style: style, disabled: false,row: 2);
                continue;
            }
            if (finder == user.id.ToString())
            {
                if (inventory.interactionType == "equip")
                    builder.WithButton(label: $"{i+1}", customId: $"InvEquip{i}_{finder}_{user.id}", style: style, disabled: !items[i]!.IsEquippable(),row: 2);
                else
                    builder.WithButton(label: $"{i+1}", customId: $"InvSell{i}_{finder}_{user.id}", style: style, disabled: false,row: 2);
            }
            
        }
        builder.WithButton(label: "Назад к профилю",customId:$"Equipment_{finder}_{user.id}" ,row:3);
        
        return builder.Build();
    }

    public static MessageComponent AcceptActions(string uid,long userId)
    {
        var builder = new ComponentBuilder()
            .WithButton(label: "Да", customId: $"Action_{userId}_{uid}_Accept", row: 0, style: ButtonStyle.Success)
            .WithButton(label: "Нет", customId: $"Action_{userId}_{uid}_Denied", row: 0, style: ButtonStyle.Danger);

        return builder.Build();
    }
}