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
            .WithButton(label: "Экипировка", customId: $"Equipment_{finder}_{toFind}",
                disabled: currentButton == "Equipment")
            .WithButton(label: "Инвентарь", customId: $"Inventory_{finder}_{toFind}")
            .WithButton(label: "Прокачка навыков", customId: $"UpSkills_{finder}", disabled: finder != toFind, row: 1);

        return builder.Build();
    }

    public static MessageComponent UpUserSkills(string finder, User user)
    {
        int lvl = user.lvl;
        bool canUpSkills = user.skillPoints > 0;
        var builder = new ComponentBuilder()
            .WithButton(label: $"{user.stats.hp} - хп", customId: "nothing", row: 0, disabled: true)
            .WithButton(label: $"{user.stats.defence} - броня", customId: $"UpSkill_{finder}_defence", row: 0,
                disabled: user.stats.defence >= lvl || !canUpSkills)
            .WithButton(label: $"{user.stats.damage} - урон", customId: $"UpSkill_{finder}_damage", row: 1,
                disabled: user.stats.damage >= lvl || !canUpSkills)
            .WithButton(label: $"{user.stats.speed} - ловкость", customId: $"UpSkill_{finder}_speed", row: 1,
                disabled: user.stats.speed >= lvl || !canUpSkills)
            .WithButton(label: $"{user.stats.luck} - удача", customId: $"UpSkill_{finder}_luck", row: 2,
                disabled: user.stats.luck >= lvl || !canUpSkills)
            .WithButton(label: "К профилю", customId: $"Profile_{finder}_{finder}", row: 3, emote: Emoji.Parse("🔙"));

        return builder.Build();
    }

    public static MessageComponent InventoryButtonsSet(string finder, long userId, Inventory? inventory, Item?[] items)
    {
        var select = new SelectMenuBuilder()
            .WithPlaceholder(
                $"Тип взаимодействия: {(inventory!.interactionType == "info" ? "информация" : inventory.interactionType == "sell" ? "продажа" : inventory.interactionType == "destroy" ? "уничтожть" : "экипировать")}")
            .WithCustomId($"Inventary_{finder}_{userId}_InteractionType")
            .AddOption("Информация", "info")
            .AddOption("Экипировать", "equip")
            .AddOption("Продать", "sell")
            .AddOption("Уничтожить", "destroy")
            .WithDisabled(finder != userId.ToString());

        var builder = new ComponentBuilder()
            .WithButton(label: "⮘", customId: $"Inventary_{finder}_{userId}_PrewPage",
                disabled: inventory.currentPage == 0, row: 0)
            .WithButton(label: $"{inventory.currentPage + 1}/{inventory.lastPage + 1}", customId: "null1",
                disabled: true, row: 0)
            .WithButton(label: "➣", customId: $"Inventary_{finder}_{userId}_NextPage",
                disabled: inventory.currentPage >= inventory.lastPage, row: 0)
            .WithButton(label: "♺", customId: $"Inventary_{finder}_{userId}_Reload")
            .WithSelectMenu(select, row: 1);

        ButtonStyle style;

        switch (inventory.interactionType)
        {
            case "info":
                style = ButtonStyle.Primary;
                break;
            case "destroy":
                style = ButtonStyle.Danger;
                break;
            default:
                style = ButtonStyle.Success;
                break;
        }


        for (int i = 0; i < items.Length; i++)
        {
            string customId;
            bool disabled = false;

            if (items[i] == null)
            {
                customId = $"null-{Guid.NewGuid()}";
                disabled = true;
            }
            else if (inventory.interactionType == "info" || finder == userId.ToString())
            {
                customId = $"Inventary_{finder}_{userId}_{inventory.interactionType}_{i}";
                ;

                if (inventory.interactionType == "equip")
                    disabled = !items[i]!.IsEquippable();
            }
            else
            {
                continue;
            }

            builder.WithButton(label: $"{i + 1}", customId: customId, style: style, disabled: disabled, row: 2);
        }

        builder.WithButton(label: "Назад к профилю", customId: $"Equipment_{finder}_{userId}", row: 3)
            .WithButton(emote: Emoji.Parse(":tools:"), label: "🛒",
                customId: $"Inventary_{finder}_{userId}_OpenSlotsSettings", row: 3);

        return builder.Build();
    }

    public static MessageComponent AcceptActions(string uid, long userId)
    {
        var builder = new ComponentBuilder()
            .WithButton(label: "Да", customId: $"Action_{userId}_{uid}_Accept", row: 0, style: ButtonStyle.Success)
            .WithButton(label: "Нет", customId: $"Action_{userId}_{uid}_Denied", row: 0, style: ButtonStyle.Danger);

        return builder.Build();
    }

    public static MessageComponent SelectLocation(string type, long userId, List<Location> locations)
    {
        var select = new SelectMenuBuilder()
            .WithPlaceholder("Куда вы хотете отправиться ?")
            .WithCustomId($"SelectLocation_{userId}_{type}");

        locations.Sort((x, y) => x.lvl - y.lvl);

        foreach (var i in locations)
        {
            select.AddOption($"{i.name} - {i.lvl}", i.id);
        }

        return new ComponentBuilder().WithSelectMenu(select).Build();
    }

    public static MessageComponent BattleButtonSet(Battle? battle, long userId, bool disableButtons = false,
        bool disableSelectEnemy = false)
    {
        var builder = new ComponentBuilder()
            .WithButton(label: "Атака", customId: $"Battle_{userId}_Attack_{battle!.id}", disabled: disableButtons)
            .WithButton(label: "Хил", customId: $"Battle_{userId}_Heal_{battle.id}", disabled: disableButtons,
                style: ButtonStyle.Success)
            .WithButton(label: "Побег", customId: $"Battle_{userId}_Run_{battle.id}", disabled: disableButtons,
                style: ButtonStyle.Danger);

        if (battle.type == "dungeon")
        {
            SelectMenuBuilder select = new SelectMenuBuilder()
                .WithPlaceholder("Выберите противника")
                .WithCustomId($"Battle_{userId}_SelectEnemy_{battle.id}");

            for (int i = 0; i < battle.enemies.Length; i++)
            {
                var temp = SecondaryFunctions.WarriorToStatusString(battle.enemies[i], i == battle.selectedEnemy);
                select.AddOption(label: $"{i + 1} - {battle.enemies[i].name}", value: $"{i}", description: temp.Item2,
                    emote: new Emoji(temp.Item1));
            }

            select.IsDisabled = disableSelectEnemy || disableButtons;

            builder.WithSelectMenu(select);
        }

        return builder.Build();
    }

    public static MessageComponent CancelFindPvp(ulong userId)
    {
        var builder = new ComponentBuilder()
            .WithButton(label: "Отмена", customId: $"FindPvp_{userId}_CancelFind", style: ButtonStyle.Danger);

        return builder.Build();
    }

    public static MessageComponent MarketSortComponents(ulong userId, string interactionId)
    {
        var rarityOptions = new[]
        {
            "сброс",
            "обычное",
            "необычное",
            "редкое",
            "эпическое",
            "легендарное",
        };

        var itemTypeOptions = new[]
        {
            "сброс",
            "шлем",
            "нагрудник",
            "перчатки",
            "штаны",
            "оружие",
            "зелья"
        };

        var levelOptions = Enumerable.Range(1, 50)
            .GroupBy(x => (x - 1) / 5)
            .Select(g => new { range = $"{g.First()}-{g.Last()}" })
            .ToArray();


        var selectMenuLvl = new SelectMenuBuilder()
            .WithCustomId($"MarketSort_{userId}_byLvl")
            .WithPlaceholder("Выберете уровень предмета")
            .WithOptions(levelOptions.Select(x =>
                new SelectMenuOptionBuilder().WithLabel($"Уровень {x.range}").WithValue($"{x.range}")).ToList());

        var selectMenuRaty = new SelectMenuBuilder()
            .WithCustomId($"MarketSort_{userId}_byRaty")
            .WithPlaceholder("Выберете редкость предмета")
            .WithOptions(rarityOptions.Select(x =>
                new SelectMenuOptionBuilder().WithLabel(x).WithValue($"{x}")).ToList());

        var selectMenuType = new SelectMenuBuilder()
            .WithCustomId($"MarketSort_{userId}_byType")
            .WithPlaceholder("Выберете тип предмета")
            .WithOptions(itemTypeOptions.Select(x =>
                new SelectMenuOptionBuilder().WithLabel(x).WithValue($"{x}")).ToList());


        var builder = new ComponentBuilder()
            .WithButton(label: "←", customId: $"MarketSort_{userId}_prewPage", row: 0)
            .WithButton(label: "↑", customId: $"MarketSort_{userId}_prewItem", row: 0)
            .WithButton(emote: Emoji.Parse(":shopping_cart:"), customId: $"MarketSort_{userId}_buyItem", row: 0)
            .WithButton(label: "↓", customId: $"MarketSort_{userId}_nextItem", row: 0)
            .WithButton(label: "→", customId: $"MarketSort_{userId}_nextPage", row: 0)
            .WithSelectMenu(selectMenuLvl, row: 1)
            .WithSelectMenu(selectMenuRaty, row: 2)
            .WithSelectMenu(selectMenuType, row: 3)
            .WithButton(emote: Emoji.Parse(":repeat:"), customId: $"MarketSort_{userId}_reloadPage", row: 4)
            .WithButton(emote: Emoji.Parse("🔍"), customId: $"MarketSort_{userId}_search", row: 4, disabled: true)
            .WithButton(emote: Emote.Parse("<:silver:997889161484828826>"), label: "↑",
                customId: $"MarketSort_{userId}_priceUp", row: 4)
            .WithButton(emote: Emote.Parse("<:silver:997889161484828826>"), label: "↓",
                customId: $"MarketSort_{userId}_priceDown", row: 4)
            .WithButton(emote: Emoji.Parse(":tools:"), customId: $"MarketSort_{userId}_openSlotsSettings", row: 4);


        return builder.Build();
    }

    public static MessageComponent MarketSettingComponents(MarketSettings settings)
    {
        var builder = new ComponentBuilder()
            .WithButton(label: "↑", customId: $"Market_{settings.userId}_prewItem", row: 0,
                disabled: settings.searchResults.Count == 0)
            .WithButton(label: "↓", customId: $"Market_{settings.userId}_nextItem", row: 0,
                disabled: settings.searchResults.Count == 0)
            .WithButton(emote: Emoji.Parse(":money_with_wings:"), customId: $"Market_{settings.userId}_editPrice",
                row: 0, disabled: settings.searchResults.Count == 0)
            .WithButton(emote: Emoji.Parse(":eye:"), customId: $"Market_{settings.userId}_editVisible", style:
                (settings.searchResults.Count == 0 ? ButtonStyle.Success :
                    settings.searchResults[settings.currentItemIndex].isVisible ? ButtonStyle.Success :
                    ButtonStyle.Danger)
                , row: 0, disabled: settings.searchResults.Count == 0)
            .WithButton(label: "Снять с продажи", customId: $"Market_{settings.userId}_Remove", row: 0,
                disabled: settings.searchResults.Count == 0)
            .WithButton(emote: Emoji.Parse(":repeat:"), customId: $"Market_{settings.userId}_reloadPage", row: 1)
            .WithButton(label: "Назад", customId: $"Market_{settings.userId}_goBack", row: 1);

        return builder.Build();
    }
}