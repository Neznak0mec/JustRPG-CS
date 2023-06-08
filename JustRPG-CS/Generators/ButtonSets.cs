using Discord;
using JustRPG.Features;
using JustRPG.Models;

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
        bool canUpSkills = user.skillPoints > 0;
        var builder = new ComponentBuilder()
            .WithButton(label: $"{user.stats.hp} - хп",customId:"nothing", row: 0, disabled: true)
            .WithButton(label: $"{user.stats.defence} - броня",customId: $"UpSkill_{finder}_defence" ,row: 0, disabled: user.stats.defence >= lvl || !canUpSkills)
            .WithButton(label: $"{user.stats.damage} - урон",customId: $"UpSkill_{finder}_damage" , row: 1, disabled: user.stats.damage >= lvl || !canUpSkills)
            .WithButton(label: $"{user.stats.speed} - ловкость",customId: $"UpSkill_{finder}_speed" , row: 1, disabled: user.stats.speed >= lvl || !canUpSkills)
            .WithButton(label: $"{user.stats.luck} - удача",customId: $"UpSkill_{finder}_luck" , row: 2, disabled: user.stats.luck >= lvl || !canUpSkills)
            .WithButton(label: "К профилю",customId:$"Profile_{finder}_{finder}" ,row: 3, emote: Emoji.Parse("🔙"));

        return builder.Build();
    }

    public static MessageComponent InventoryButtonsSet(string finder, long userId, Inventory? inventory,Item?[] items)
    {
        
        
        var select = new SelectMenuBuilder()
            .WithPlaceholder(
                $"Тип взаимодействия: {(inventory!.interactionType == "info" ? "информация" : inventory.interactionType == "sell" ? "продажа" : "экипировать")}")
            .WithCustomId($"InvInteractionType_{finder}_{userId}")
            .AddOption("Информация", "info")
            .AddOption("Экипировать", "equip")
            .AddOption("Продать", "sell");

        var builder = new ComponentBuilder()
            .WithButton(label: "⮘", customId: $"InvPrewPage_{finder}_{userId}", disabled: inventory.currentPage == 0, row: 0)
            .WithButton(label: $"{inventory.currentPage+1}/{inventory.lastPage+1}", customId: "null1", disabled: true, row: 0)
            .WithButton(label: "➣", customId: $"InvNextPage_{finder}_{userId}", disabled: inventory.currentPage >= inventory.lastPage, row: 0)
            .WithButton(label: "♺", customId: $"InvReload_{finder}_{userId}")
            .WithSelectMenu(select, row: 1);

        ButtonStyle style = inventory.interactionType == "info" ? ButtonStyle.Primary : inventory.interactionType == "sell" ? ButtonStyle.Danger : ButtonStyle.Success;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                builder.WithButton(label: $"{i+1}", customId: $"null-{Guid.NewGuid()}", style: style, disabled: true,row: 2);
                continue;
            }
            
            if (inventory.interactionType == "info")
            {
                builder.WithButton(label: $"{i+1}", customId: $"InvInfo_{finder}_{userId}_{i}", style: style, disabled: false,row: 2);
                continue;
            }
            if (finder == userId.ToString())
            {
                if (inventory.interactionType == "equip")
                    builder.WithButton(label: $"{i+1}", customId: $"InvEquip_{finder}_{userId}_{i}", style: style, disabled: !items[i]!.IsEquippable(),row: 2);
                else
                    builder.WithButton(label: $"{i+1}", customId: $"InvSell_{finder}_{userId}_{i}", style: style, disabled: false,row: 2);
            }
            
        }
        builder.WithButton(label: "Назад к профилю",customId:$"Equipment_{finder}_{userId}" ,row:3);
        
        return builder.Build();
    }

    public static MessageComponent AcceptActions(string uid,long userId)
    {
        var builder = new ComponentBuilder()
            .WithButton(label: "Да", customId: $"Action_{userId}_{uid}_Accept", row: 0, style: ButtonStyle.Success)
            .WithButton(label: "Нет", customId: $"Action_{userId}_{uid}_Denied", row: 0, style: ButtonStyle.Danger);

        return builder.Build();
    }

    public static MessageComponent SelectLocation(string type,long userId, List<Location> locations)
    {
        var select = new SelectMenuBuilder()
            .WithPlaceholder("Куда вы хотете отправиться ?")
            .WithCustomId($"SelectLocation_{userId}_{type}");
        
        locations.Sort((x, y) => x.lvl-y.lvl);

        foreach (var i in locations)
        {
            select.AddOption($"{i.name} - {i.lvl}", i.id);
        }

        return new ComponentBuilder().WithSelectMenu(select).Build();
    }

    public static MessageComponent BattleButtonSet(Battle? battle, long userId,bool disableButtons = false, bool disableSelectEnemy = false)
    {
        var builder = new ComponentBuilder()
            .WithButton(label: "Атака", customId: $"Battle_{userId}_Attack_{battle!.id}",disabled: disableButtons)
            .WithButton(label: "Хил", customId: $"Battle_{userId}_Heal_{battle.id}",disabled: disableButtons, style:ButtonStyle.Success)
            .WithButton(label: "Побег", customId: $"Battle_{userId}_Run_{battle.id}",disabled: disableButtons, style:ButtonStyle.Danger);

        if (battle.type == "dungeon"){
            SelectMenuBuilder select = new SelectMenuBuilder()
                .WithPlaceholder("Выберите противника")
                .WithCustomId($"Battle_{userId}_SelectEnemy_{battle.id}");

            for (int i = 0; i < battle.enemies.Length; i++)
            {
                var temp = SecondaryFunctions.WarriorToStatusString(battle.enemies[i], i == battle.selectedEnemy);
                select.AddOption(label:$"{i+1} - {battle.enemies[i].name}",value:$"{i}",description:temp.Item2,emote: new Emoji(temp.Item1) );
            }

            select.IsDisabled = disableSelectEnemy || disableButtons;

            builder.WithSelectMenu(select);
        }

        return builder.Build();
    }
}