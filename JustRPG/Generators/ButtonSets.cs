using Discord;
using Discord.WebSocket;
using JustRPG.Features;
using JustRPG.Models;
using JustRPG.Models.Enums;

namespace JustRPG.Generators;

public static class ButtonSets
{
    public static MessageComponent ProfileButtonsSet(string finder, string toFind, string currentButton = "Profile")
    {
        var builder = new ComponentBuilder()
            .WithButton(label: "Профиль", customId: $"Profile_{finder}_{toFind}", disabled: currentButton == "Profile")
            .WithButton(label: "Экипировка", customId: $"Equipment_{finder}_{toFind}",
                disabled: currentButton == "Equipment")
            .WithButton(label: "Инвентарь", customId: $"Inventory_{finder}_{toFind}");

        return builder.Build();
    }

    public static MessageComponent InventoryButtonsSet(string finder, long userId, Inventory? inventory, Item?[] items)
    {
        IEmote emote = inventory.interactionType switch
        {
            "info" => Emoji.Parse(":identification_card:"),
            "destroy" => Emoji.Parse(":wastebasket:"),
            "sell" => Emoji.Parse(":scales:"),
            _ => Emoji.Parse(":shirt:")
        };

        ButtonStyle style = inventory.interactionType switch
        {
            "info" => ButtonStyle.Primary,
            "destroy" => ButtonStyle.Danger,
            _ => ButtonStyle.Success
        };


        var builder = new ComponentBuilder()
            .WithButton(label: "←", customId: $"Inventory|prewPage_{finder}_{userId}", row: 0)
            .WithButton(label: "↑", customId: $"Inventory|prewItem_{finder}_{userId}", row: 0)
            .WithButton(emote: emote, customId: $"Inventory|interact_{finder}_{userId}", style: style, row: 0)
            .WithButton(label: "↓", customId: $"Inventory|nextItem_{finder}_{userId}", row: 0)
            .WithButton(label: "→", customId: $"Inventory|nextPage_{finder}_{userId}", row: 0);

        if (inventory.showSortSelections)
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
                "ботинки",
                "оружие",
                "зелья"
            };

            var levelOptions = Enumerable.Range(1, 65)
                .GroupBy(x => (x - 1) / 5)
                .Select(g => new { range = $"{g.First()}-{g.Last()}" })
                .ToArray();

            var selectMenuLvl = new SelectMenuBuilder()
                .WithCustomId($"Inventory|byLvl_{finder}_{userId}")
                .WithPlaceholder("Выберете уровень предмета")
                .WithOptions(levelOptions.Select(x =>
                    new SelectMenuOptionBuilder().WithLabel($"Уровень {x.range}").WithValue($"{x.range}")).ToList());

            var selectMenuRaty = new SelectMenuBuilder()
                .WithCustomId($"Inventory|byRaty_{finder}_{userId}")
                .WithPlaceholder("Выберете редкость предмета")
                .WithOptions(rarityOptions.Select(x =>
                    new SelectMenuOptionBuilder().WithLabel(x).WithValue($"{x}")).ToList());

            var selectMenuType = new SelectMenuBuilder()
                .WithCustomId($"Inventory|byType_{finder}_{userId}")
                .WithPlaceholder("Выберете тип предмета")
                .WithOptions(itemTypeOptions.Select(x =>
                    new SelectMenuOptionBuilder().WithLabel(x).WithValue($"{x}")).ToList());

            builder.WithSelectMenu(selectMenuLvl, row: 1)
                .WithSelectMenu(selectMenuRaty, row: 2)
                .WithSelectMenu(selectMenuType, row: 3);
        }
        else
        {
            var select = new SelectMenuBuilder()
                .WithPlaceholder(
                    $"Тип взаимодействия: {(inventory!.interactionType == "info" ? "информация" : inventory.interactionType == "sell" ? "продажа" : inventory.interactionType == "destroy" ? "уничтожить" : "экипировать")}")
                .WithCustomId($"Inventory|InteractionType_{finder}_{userId}")
                .AddOption("Информация", "info")
                .AddOption("Экипировать", "equip")
                .AddOption("Продать", "sell")
                .AddOption("Уничтожить", "destroy")
                .WithDisabled(finder != userId.ToString());

            builder.WithSelectMenu(select, row: 1);
        }


        builder
            .WithButton(emote: Emoji.Parse(":back:"), customId: $"Profile_{finder}_{userId}", row: 2)
            .WithButton(emote: Emoji.Parse(":tools:"), label: "🛒",
                customId: $"Inventory|OpenSlotsSettings_{finder}_{userId}", row: 2)
            .WithButton(emote: Emoji.Parse(":compression:"), customId: $"Inventory|SortMenu_{finder}_{userId}", row: 2)
            .WithButton(label: "♺", customId: $"Inventory|Reload_{finder}_{userId}", row: 2);

        return builder.Build();
    }

    public static MessageComponent SaleItemButtonsSet(long userid, string itemId)
    {
        ComponentBuilder builder =
            new ComponentBuilder().WithButton(label: "Установить цену", $"Market|setPrice_{userid}_{itemId}");

        return builder.Build();
    }

    public static MessageComponent AcceptActions(string uid, long userId)
    {
        var builder = new ComponentBuilder()
            .WithButton(label: "Да", customId: $"Action|Accept_{userId}_{uid}", row: 0, style: ButtonStyle.Success)
            .WithButton(label: "Нет", customId: $"Action|Denied_{userId}_{uid}", row: 0, style: ButtonStyle.Danger);

        return builder.Build();
    }

    public static MessageComponent SelectLocation(string type, long userId, List<Location> locations)
    {
        var select = new SelectMenuBuilder()
            .WithPlaceholder("Куда вы хотете отправиться ?")
            .WithCustomId($"SelectLocation|{type}_{userId}");

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
            .WithButton(emote: Emoji.Parse(":dagger:"), customId: $"Battle|Attack_{userId}_{battle!.id}",
                disabled: disableButtons)
            .WithButton(emote: Emoji.Parse(":heavy_plus_sign:"), customId: $"Battle|Heal_{userId}_{battle.id}",
                disabled: disableButtons,
                style: ButtonStyle.Success)
            .WithButton(emote: Emoji.Parse(":person_running:"), customId: $"Battle|Run_{userId}_{battle.id}",
                disabled: disableButtons || battle.type == BattleType.arena,
                style: ButtonStyle.Danger);

        if (battle.type == BattleType.dungeon)
        {
            SelectMenuBuilder select = new SelectMenuBuilder()
                .WithPlaceholder("Выберите противника")
                .WithCustomId($"Battle|SelectEnemy_{userId}_{battle.id}");

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
            .WithButton(label: "Отмена", customId: $"FindPvp|Cancel_{userId}", style: ButtonStyle.Danger);

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
            "ботинки",
            "оружие",
            "зелья"
        };

        var levelOptions = Enumerable.Range(1, 65)
            .GroupBy(x => (x - 1) / 5)
            .Select(g => new { range = $"{g.First()}-{g.Last()}" })
            .ToArray();


        var selectMenuLvl = new SelectMenuBuilder()
            .WithCustomId($"MarketSort|byLvl_{userId}")
            .WithPlaceholder("Выберете уровень предмета")
            .WithOptions(levelOptions.Select(x =>
                new SelectMenuOptionBuilder().WithLabel($"Уровень {x.range}").WithValue($"{x.range}")).ToList());

        var selectMenuRaty = new SelectMenuBuilder()
            .WithCustomId($"MarketSort|byRaty_{userId}")
            .WithPlaceholder("Выберете редкость предмета")
            .WithOptions(rarityOptions.Select(x =>
                new SelectMenuOptionBuilder().WithLabel(x).WithValue($"{x}")).ToList());

        var selectMenuType = new SelectMenuBuilder()
            .WithCustomId($"MarketSort|byType_{userId}")
            .WithPlaceholder("Выберете тип предмета")
            .WithOptions(itemTypeOptions.Select(x =>
                new SelectMenuOptionBuilder().WithLabel(x).WithValue($"{x}")).ToList());


        var builder = new ComponentBuilder()
            .WithButton(label: "←", customId: $"MarketSort|prewPage_{userId}", row: 0)
            .WithButton(label: "↑", customId: $"MarketSort|prewItem_{userId}", row: 0)
            .WithButton(emote: Emoji.Parse(":shopping_cart:"), customId: $"MarketSort|buyItem_{userId}", row: 0)
            .WithButton(label: "↓", customId: $"MarketSort|nextItem_{userId}", row: 0)
            .WithButton(label: "→", customId: $"MarketSort|nextPage_{userId}", row: 0)
            .WithSelectMenu(selectMenuLvl, row: 1)
            .WithSelectMenu(selectMenuRaty, row: 2)
            .WithSelectMenu(selectMenuType, row: 3)
            .WithButton(emote: Emoji.Parse(":repeat:"), customId: $"MarketSort|reloadPage_{userId}", row: 4)
            .WithButton(emote: Emoji.Parse("🔍"), customId: $"MarketSort|search_{userId}", row: 4, disabled: true)
            .WithButton(emote: Emote.Parse("<:silver:997889161484828826>"), label: "↑",
                customId: $"MarketSort|priceUp_{userId}", row: 4)
            .WithButton(emote: Emote.Parse("<:silver:997889161484828826>"), label: "↓",
                customId: $"MarketSort|priceDown_{userId}", row: 4)
            .WithButton(emote: Emoji.Parse(":tools:"), customId: $"MarketSort|openSlotsSettings_{userId}", row: 4);


        return builder.Build();
    }

    public static MessageComponent MarketSettingComponents(MarketSlotsSettings settings)
    {
        var builder = new ComponentBuilder()
            .WithButton(label: "↑", customId: $"Market|prewItem_{settings.userId}", row: 0,
                disabled: settings.searchResults.Count == 0)
            .WithButton(label: "↓", customId: $"Market|nextItem_{settings.userId}", row: 0,
                disabled: settings.searchResults.Count == 0)
            .WithButton(emote: Emoji.Parse(":money_with_wings:"), customId: $"Market|updatePrice_{settings.userId}",
                row: 0, disabled: settings.searchResults.Count == 0)
            .WithButton(emote: Emoji.Parse(":eye:"), customId: $"Market|editVisible_{settings.userId}", style:
                (settings.searchResults.Count == 0 ? ButtonStyle.Success :
                    settings.searchResults[settings.currentItemIndex].isVisible ? ButtonStyle.Success :
                    ButtonStyle.Danger)
                , row: 0, disabled: settings.searchResults.Count == 0)
            .WithButton(label: "Снять с продажи", customId: $"Market|Remove_{settings.userId}", row: 0,
                disabled: settings.searchResults.Count == 0)
            .WithButton(emote: Emoji.Parse(":repeat:"), customId: $"Market|reloadPage_{settings.userId}", row: 1)
            .WithButton(label: "Назад", customId: $"Market|goBack_{settings.userId}", row: 1);

        return builder.Build();
    }

    public static MessageComponent GuildComponents(Guild guild, ulong userId)
    {
        var builder = new ComponentBuilder();

        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)userId);
        if (member == null)
        {
            builder.WithButton(label: "Участники", $"Guild|Members_{userId}_{guild.tag}");
            switch (guild.join_type)
            {
                case JoinType.open:
                    builder.WithButton("Вступить", $"Guild|Join_{userId}_{guild.tag}");
                    break;
                case JoinType.invite:
                    builder.WithButton("Заявка", $"Guild|Join_{userId}_{guild.tag}");
                    break;
                default:
                    builder.WithButton("Вступить", $"Guild|Join_{userId}_{guild.tag}", disabled: true);
                    break;
            }
        }
        else
            switch (member.rank)
            {
                case GuildRank.warrior:
                    builder.WithButton("Участники", $"Guild|Members_{userId}_{guild.tag}")
                        .WithButton("Выйти", $"Guild|Leave_{userId}_{guild.tag}", style: ButtonStyle.Danger);
                    break;
                case GuildRank.officer:
                    builder.WithButton("Участники", $"Guild|Members_{userId}_{guild.tag}")
                        .WithButton("Заявки", $"Guild|Applications_{userId}_{guild.tag}",
                            disabled: guild.join_type != JoinType.invite)
                        .WithButton("Выйти", $"Guild|Leave_{userId}_{guild.tag}", style: ButtonStyle.Danger);
                    break;
                default:
                    builder.WithButton("Участники", $"Guild|Members_{userId}_{guild.tag}")
                        .WithButton("Заявки", $"Guild|Applications_{userId}_{guild.tag}")
                        .WithButton("Настройки", $"Guild|Settings_{userId}_{guild.tag}");
                    break;
            }

        return builder.Build();
    }

    public static MessageComponent GuildMembers(Guild guild, ulong userId, DiscordSocketClient bot)
    {
        var builder = new ComponentBuilder();

        builder.WithButton("Главная", $"Guild_{userId}_{guild.tag}");

        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)userId);
        if (member is { rank: GuildRank.officer or GuildRank.owner })
        {
            builder.WithButton("Кикнуть", customId: $"Guild|Kick_{userId}_{guild.tag}");
        }

        if (member is { rank: GuildRank.owner })
        {
            builder.WithButton("Оффицер", customId: $"Guild|Officer_{userId}_{guild.tag}");
        }

        return builder.Build();
    }

    public static MessageComponent GuildSettings(Guild guild, ulong userId)
    {
        var builder = new ComponentBuilder();


        var selectMenuType = new SelectMenuBuilder()
            .WithCustomId($"Guild|joinType_{userId}_{guild.tag}")
            .WithPlaceholder("Тип входа");

        selectMenuType.AddOption(new SelectMenuOptionBuilder().WithLabel("Открытый").WithValue("open"));
        selectMenuType.AddOption(new SelectMenuOptionBuilder().WithLabel("По инвайтам").WithValue("invite"));
        selectMenuType.AddOption(new SelectMenuOptionBuilder().WithLabel("Закрыт").WithValue("closed"));


        builder
            .WithButton("Главная", $"Guild_{userId}_{guild.tag}")
            .WithButton("Значок", $"Guild|EditSymbol_{userId}_{guild.tag}")
            .WithSelectMenu(selectMenuType);

        return builder.Build();
    }

    public static MessageComponent GuildApplications(Guild guild, ulong userId)
    {
        var builder = new ComponentBuilder();
        bool listIsEmpty = guild.wantJoin.Count == 0;

        builder.WithButton("Главная", $"Guild_{userId}_{guild.tag}")
            .WithButton("Принять", $"Guild|Accept_{userId}_{guild.tag}", disabled: listIsEmpty,
                style: ButtonStyle.Success)
            .WithButton("Отклонить", $"Guild|Denied_{userId}_{guild.tag}", disabled: listIsEmpty,
                style: ButtonStyle.Danger);


        return builder.Build();
    }

    public static MessageComponent GuildCreateComponents(ulong userId)
    {
        var builder = new ComponentBuilder();

        builder.WithButton("Создать", $"Guild|Create_{userId}");

        return builder.Build();
    }

    public static MessageComponent SelectRewardsComponents(ulong userId, BattleResultDrop resultDrop,bool end = false)
    {
        var builder = new ComponentBuilder();

        builder.WithButton(label: "↑", customId: $"SelectRewards|prewItem_{userId}_{resultDrop.id}", row: 0, disabled: end)
            .WithButton(emote: Emoji.Parse(":pushpin:"), customId: $"SelectRewards|Select_{userId}_{resultDrop.id}", row: 0, disabled: end)
            .WithButton(label: "↓", customId: $"SelectRewards|nextItem_{userId}_{resultDrop.id}", row: 0, disabled: end)
            .WithButton(emote: Emoji.Parse(":heavy_check_mark:"), customId: $"SelectRewards|Complete_{userId}_{resultDrop.id}", row: 1,style: ButtonStyle.Success, disabled: end);
        
        return builder.Build();
    }

    public static MessageComponent StartSelectRewards(ulong userId)
    {
        var builder = new ComponentBuilder();

        builder.WithButton(emote: Emoji.Parse(":gem:"), customId: $"Battle|StartSelectReward_{userId}_a", style: ButtonStyle.Success);

        return builder.Build();
    }
}