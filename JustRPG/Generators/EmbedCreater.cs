using Discord;
using Discord.WebSocket;
using JustRPG.Models;
using JustRPG.Models.SubClasses;
using JustRPG.Services;
using JustRPG.Features;
using JustRPG.Models.Enums;

namespace JustRPG.Generators;

public class EmbedCreater
{
    public static Embed ErrorEmbed(string text)
    {
        var emb = new EmbedBuilder
        {
            Title = "🚫 Ошибка",
            Color = Color.Red,
            Description = text
        };
        return emb.Build();
    }

    public static Embed EmpEmbed(string text)
    {
        var emb = new EmbedBuilder
        {
            Description = text
        };
        return emb.Build();
    }

    public static Embed WarningEmbed(string text)
    {
        var emb = new EmbedBuilder
        {
            Title = "⚠ Внимание",
            Color = Color.DarkOrange,
            Description = text
        };
        return emb.Build();
    }

    public static Embed SuccessEmbed(string text)
    {
        var emb = new EmbedBuilder
        {
            Title = "✅ Успешно",
            Color = Color.Green,
            Description = text
        };
        return emb.Build();
    }

    public static async Task<Embed> UserProfile(User user, IUser member, DataBase dataBase)
    {
        var emb = new EmbedBuilder
        {
            Title = $"Профиль {await user.GetFullName(member.Username, dataBase)}"
        };
        var stats = await new BattleStats().BattleStatsAsync(user, dataBase);

        emb.AddField($"Уровень", $"{user.lvl}", inline: true)
            .AddField("Опыт", $"{(int)user.exp}\\{(int)user.expToLvl}", inline: true)
            .AddField("Баланс", $"{user.cash}", inline: true)
            .AddField("Очки рейтинга", $"{user.mmr}", inline: true)
            .AddField(name: "Статы",
                value:
                $":heart: : {user.stats.hp} + {stats.hp - user.stats.hp} |  :dagger: : {user.stats.damage} + {stats.damage - user.stats.damage}" +
                $"| :shield: : {user.stats.defence} + {stats.defence - user.stats.defence} \n:zap: : {user.stats.speed} + {stats.speed - user.stats.speed}" +
                $"| :four_leaf_clover: : {user.stats.luck} + {stats.luck - user.stats.luck}");
        return emb.Build();
    }

    public static async Task<Embed> UserEquipmentEmbed(User user, IUser member, DataBase dataBase)
    {
        var embed = new EmbedBuilder
        {
            Title = $"Экипировка {await user.GetFullName(member.Username, dataBase)}"
        };
        UserEquipment equipment = await user.GetEquipmentAsItems(dataBase!);

        embed.AddField(equipment.helmet == null ? "Шлем" : $"Шлем - {equipment.helmet!.name} | {equipment.helmet!.lvl}",
                equipment.helmet == null ? "Не надето" : equipment.helmet!.ToString(), true)
            .AddField(
                equipment.armor == null ? "Нагрудник" : $"Нагрудник - {equipment.armor!.name} | {equipment.armor!.lvl}",
                equipment.armor == null ? "Не надето" : equipment.armor!.ToString(), true)
            .AddField(equipment.pants == null ? "Штаны" : $"Штаны - {equipment.pants!.name} | {equipment.pants!.lvl}",
                equipment.pants == null ? "Не надето" : equipment.pants!.ToString(), true)
            .AddField(
                equipment.shoes == null ? "Ботинки" : $"Ботинки - {equipment.shoes!.name} | {equipment.shoes!.lvl}",
                equipment.shoes == null ? "Не надето" : equipment.shoes!.ToString(), true)
            .AddField(
                equipment.gloves == null
                    ? "Перчатки"
                    : $"Перчатки - {equipment.gloves!.name} | {equipment.gloves!.lvl}",
                equipment.gloves == null ? "Не надето" : equipment.gloves!.ToString(), true)
            .AddField(
                equipment.weapon == null ? "Оружие" : $"Оружие - {equipment.weapon!.name} | {equipment.weapon!.lvl}",
                equipment.weapon == null ? "Не надето" : equipment.weapon!.ToString(), true);

        return embed.Build();
    }

    public static async Task<Embed> UserInventory(IUser member, User user, Inventory inventory, DataBase dataBase)
    {
        var emb = new EmbedBuilder { Title = $"Инвентарь {await user.GetFullName(member.Username, dataBase)}" };
        Item?[] items = inventory.GetItems();
        for (int i = 0; i < 5; i++)
        {
            if (items[i] == null)
            {
                emb.AddField("Пусто", "Слот не занят");
                continue;
            }

            string title = inventory.CurrentItemIndex == i
                ? $":diamond_shape_with_a_dot_inside: `{items[i]!.lvl} | {items[i]!.name}`"
                : $"{items[i]!.lvl} | {items[i]!.name}";
            emb.AddField(title, $">>> {items[i]!}");

            if (inventory.CurrentItemIndex == i && inventory.interactionType == "equip" &&
                inventory.Items[i].IsEquippable())
            {
                UserEquipment equipment = await user.GetEquipmentAsItems(dataBase);
                Item? equippedItem = equipment.GetEquippedItemByType(inventory.Items[i].type);

                if (equippedItem != null)
                    emb.AddField($":scales: Сейчас надето `{equippedItem.lvl} | {equippedItem.name}`",
                        $">>> {equippedItem}");
            }
        }


        string footer = $"Страница {inventory.CurrentPage + 1}/{inventory.GetCountOfPages()}\n" +
                        (inventory.itemLvl != null
                            ? $" | Сортировка по {inventory.itemLvl?.Item1}-{inventory.itemLvl?.Item2} уровню\n"
                            : "") +
                        (inventory.itemRarity != null ? $" | редкость - {SecondaryFunctions.GetRarityColoredEmoji(inventory.itemRarity!)}\n" : "") +
                        (inventory.itemType != null ? $" | тип - {inventory.itemType}" : "");
        emb.WithFooter(footer);


        return emb.Build();
    }

    public static Embed ItemInfo(Item item)
    {
        var emb = new EmbedBuilder { Title = "Информация о " + item.name }
            .AddField("Тип", item.type, inline: true)
            .AddField("Уровень", item.lvl.ToString(), inline: true)
            .AddField("Редкость", item.rarity, inline: true)
            .AddField("Описание", item.description != "" ? item.description : "Описания нет", inline: true)
            .AddField("Статы", item.ToString(), inline: true)
            .AddField("uid", $"`{item.id}`", inline: true);

        Color temp = item.rarity switch
        {
            Rarity.common => 0xffffff,
            Rarity.uncommon => 0x0033cc,
            Rarity.rare => 0x6600ff,
            Rarity.epic => 0xffcc00,
            Rarity.legendary => 0xcc0000,
            Rarity.impossible => 0x000000,
            Rarity.exotic => 0xcc0066,
            Rarity.prize => 0xcccc00,
            Rarity.eventt => 0x666600,
            _ => 0xffffff
        };

        emb.Color = temp;

        return emb.Build();
    }

    public static Embed WorkEmbed(List<Work> works, int exp, int cash)
    {
        Work work = works[Random.Shared.Next(0, works.Count)];

        EmbedBuilder embed = new EmbedBuilder
        {
            Title = work.title,
            Description = string.Format(work.description, exp, cash),
            ThumbnailUrl = work.url
        };

        return embed.Build();
    }

    public static Embed SelectAdventureEmbed()
    {
        EmbedBuilder embed = new EmbedBuilder
        {
            Title = "Выберите куда хочешь отправиться",
            Description = "Поход - самый эффективный способ прокачки для новичков."
        };

        return embed.Build();
    }

    public static Embed BattleEmbed(Battle? battle, bool gameEnded = false)
    {
        Warrior selectedEnemy, currentWarrior;
        EmbedBuilder embed;
        var progressBar = SecondaryFunctions.ProgressBar;
        switch (battle!.type)
        {
            case BattleType.adventure or BattleType.dungeon:
                currentWarrior = battle.players[battle.currentUser];
                selectedEnemy = battle.enemies[battle.selectedEnemy];
                break;
            case BattleType.arena:
                currentWarrior = battle.players[0];
                selectedEnemy = battle.players[1];
                break;
            default:
                return ErrorEmbed("wha ?");
        }

        embed = new EmbedBuilder
        {
            Title =
                $"Бой {battle.players[0].fullName} - {(battle.type is BattleType.arena ? battle.players[1] : selectedEnemy).fullName}",
            Description = battle.type == BattleType.arena ? $"Сейчас ходит {currentWarrior.name}" : ""
        };


        embed.WithThumbnailUrl(battle.type is BattleType.adventure or BattleType.dungeon
            ? selectedEnemy.url
            : currentWarrior.url);

        if (!gameEnded)
        {
            embed.AddField($"{currentWarrior.fullName} - {currentWarrior.lvl}",
                $":heart: - {progressBar(currentWarrior.stats.hp, currentWarrior.stats.MaxHP)}\n " +
                $":shield: - {progressBar(currentWarrior.stats.defence, currentWarrior.stats.MaxDef)}\n" +
                $":dagger: - {currentWarrior.stats.damage}");

            embed.AddField($"{selectedEnemy.fullName} - {selectedEnemy.lvl}",
                $":heart: - {progressBar(selectedEnemy.stats.hp, selectedEnemy.stats.MaxHP)}\n " +
                $":shield: - {progressBar(selectedEnemy.stats.defence, selectedEnemy.stats.MaxDef)}\n" +
                $":dagger: - {selectedEnemy.stats.damage}");
        }

        if (battle.log != "")
            embed.AddField("Логи", $">>> {battle.log}");

        return embed.Build();
    }

    public static Embed FindPvp(FindPVP pvp, long count = -1)
    {
        EmbedBuilder embed = new EmbedBuilder
        {
            Title = $"Поиск битвы",
            Description = "Чем дольше вы ждёте битву тем более сильный или слабый противник может попасться"
        };
        embed.AddField("Ваш mmr", $"```{pvp.mmr}```", inline: true);
        embed.AddField("Время начала поиска", $"<t:{pvp.stratTime}:R>", inline: true);
        if (count != -1)
            embed.AddField("Игроков в поиске на момент его начала", $"```{count}```");

        embed.WithFooter("Может возникнуть ошибка вечного поиска. Проблема будет решена в ближайшее время");

        return embed.Build();
    }

    public static Embed MarketPage(MarketSearchState searchState)
    {
        var emb = new EmbedBuilder { Title = "Маркет" };
        List<SaleItem> items = searchState.GetItemsOnPage(searchState.CurrentPage);
        for (int i = 0; i < 5; i++)
        {
            if (i >= items.Count)
                emb.AddField("-", "-");
            else
                emb.AddField(
                    (searchState.CurrentItemIndex == i ? "💠 " : "") +
                    $"{items[i].itemName} | {items[i].price}<:silver:997889161484828826>", items[i].itemDescription);
        }

        return emb.Build();
    }

    public static Embed MarketSettingsPage(MarketSlotsSettings searchState)
    {
        List<SaleItem> items = searchState.searchResults;
        var emb = new EmbedBuilder
        {
            Title = "Настрокйи товаров",
            Description = (items.Count == 0 ? "Вы не выставили предметы на продажу" : null)
        };

        if (searchState.currentItemIndex > searchState.searchResults.Count - 1)
            searchState.currentItemIndex = 0;

        if (items.Count == 0)
            return emb.Build();

        for (int i = 0; i < 5; i++)
        {
            if (i < items.Count)
                emb.AddField(
                    (searchState.currentItemIndex == i ? "💠 " : "") +
                    $"{items[i].itemName} | {(items[i].price == -1 ? "не установлено" : items[i].price)}<:silver:997889161484828826>",
                    items[i].itemDescription);
        }

        return emb.Build();
    }

    public static Embed GuildEmbed(Guild guild)
    {
        var builder = new EmbedBuilder()
            .WithTitle($"Гильдия {guild.symbol} [{guild.tag}] {guild.name}")
            .WithThumbnailUrl(guild.logo)
            .AddField("Глава", $"<@{guild.members.First(x => x.rank == GuildRank.owner).user}>", inline: true)
            .AddField("Участников", $"{guild.members.Count}/30", inline: true)
            .AddField("Тег", guild.tag, inline: true);

        if (guild.premium)
            builder.AddField("Значок", guild.symbol ?? "Не установлен", inline: true);

        string inviteType = guild.join_type switch
        {
            JoinType.open => "Открытый",
            JoinType.invite => "Через заявку",
            _ => "Закрыт"
        };

        builder.AddField("Тип присоединения", inviteType, inline: true);

        return builder.Build();
    }

    public static async Task<Embed> GuildMembers(Guild guild, DiscordSocketClient client)
    {
        var emb = new EmbedBuilder
        {
            Title = $"Участники {guild.name}"
        };
        foreach (var member in guild.members)
        {
            string rank = member.rank switch
            {
                GuildRank.owner => "глава",
                GuildRank.officer => "оффицер",
                _ => "Участник"
            };
            var userName = await SecondaryFunctions.GetUserName(member.user, client);

            emb.AddField(userName, $"Звание - `{rank}`");
        }

        return emb.Build();
    }

    public static async Task<Embed> GuildApplications(Guild guild, DiscordSocketClient client)
    {
        List<long> users = guild.wantJoin.Where((x, y) => y <= 25).ToList();
        var emb = new EmbedBuilder
        {
            Title = "Заявки на вступление"
        };

        if (users.Count == 0)
            emb.Description = "Заявок нет";

        foreach (var user in users)
        {
            emb.AddField(await SecondaryFunctions.GetUserName(user, client), $"id - `{user}`");
        }

        if (guild.wantJoin.Count > 25)
            emb.Description = $"Отображены первые 25 заявок, скрыто {users.Count - 25}";

        return emb.Build();
    }

    public static Embed GuildSettings(Guild guild)
    {
        var emb = new EmbedBuilder
        {
            Title = $"Настройки гильдии {guild.name}"
        };

        emb.WithThumbnailUrl(guild.logo)
            .AddField("Тег", guild.tag)
            .AddField("Название", guild.name)
            .AddField("Тип присоединения", guild.join_type switch
            {
                JoinType.open => "Открытый",
                JoinType.invite => "Через заявку",
                _ => "Закрыт"
            })
            .AddField("Глава", $"<@{guild.members.First(x => x.rank == GuildRank.owner).user}>")
            .AddField("Участников", $"{guild.members.Count}/30")
            .AddField("Премиум", guild.premium ? "Есть" : "Отсутвует")
            .AddField("Значок", guild.symbol == "" ? "Не установлен" : guild.symbol);


        return emb.Build();
    }

    public static Embed HelpEmbed(IUser owner)
    {
        var emb = new EmbedBuilder
        {
            Title = "**Привет! Я бот Just RPG! Я являюсь игрой в жанре RPG.**",
            Description =
                "Благодоря мне вы можете ходить в походы, прокачивать персонажа, выбивать предметы с монстров... " +
                "Короче почти всё, что можно делать в обычной  RPG \ud83e\udd17\n\n" +
                "**Если возникли технические шоколадки, либо нужна помощь, свяжитесь с нами на оффициальном сервере поддержки бота:** " +
                "[Оффициальный сервер бота](https://discord.gg/a8XdEThKzM)\nТак же в нашу команду требуются художники и кодеры," +
                " так что будем рады любой помощи \ud83d\ude18"
        };

        emb.WithFooter(text: owner.GlobalName, iconUrl: owner.GetAvatarUrl()!);

        return emb.Build();
    }

    public static async Task<Embed> TopEmbed(DataBase dataBase, DiscordSocketClient client)
    {
        var emb = new EmbedBuilder
        {
            Title = "Топ игроков"
        };
        List<User> users = dataBase.UserDb.GetTopMMR();
        for (int i = 0; i < 10; i++)
        {
            if (i >= users.Count)
                break;

            User dbUser = (User)await dataBase.UserDb.Get(users[i].id);
            string user = await SecondaryFunctions.GetUserName(dbUser.id, client);
            string fullName = await dbUser.GetFullName(user, dataBase);
            emb.AddField($"{i + 1}. {fullName}", $"Очки рейтинга - {users[i].mmr}");
        }

        return emb.Build();
    }

    public static async Task<Embed?> SelectRewardsEmbed(BattleResultDrop drop,DataBase dataBase,bool end = false)
    {
        var emb = new EmbedBuilder
        {
            Title = "Выбор награды"
        };
        if (end)
        {
            emb.Description = "Выбранные награды были отправлены в инвентарь";
            for (int i = 0; i < drop.Items.Count; i++)
            {
                if (drop.selectedItems.Contains(i))
                {
                    
                    Item item = drop.Items[i];
                    emb.AddField($"{item.name} | {item.lvl}", $">>> {drop.Items[i]}");
                }
            }
            return emb.Build();
        }

        for (int i = 0; i < drop.Items.Count; i++)
        {
            string itemName = drop.Items[i].name;
            if (drop.selectedItems.Contains(i))
                itemName = "\ud83d\udccc " + itemName;
            if (i == drop.CurrentItemIndex)
                itemName = "💠 " + itemName;
            emb.AddField(itemName, $">>> {drop.Items[i]}");
            
            if (drop.CurrentItemIndex == i &&
                drop.Items[i].IsEquippable())
            {
                User user = dataBase.UserDb.Get(drop.userId).Result!;
                UserEquipment equipment = await user.GetEquipmentAsItems(dataBase);
                Item? equippedItem = equipment.GetEquippedItemByType(drop.Items[i].type);

                if (equippedItem != null)
                    emb.AddField($":scales: Сейчас надето `{equippedItem.lvl} | {equippedItem.name}`",
                        $">>> {equippedItem}");
            }
            
        }

        return emb.Build();
    }
}