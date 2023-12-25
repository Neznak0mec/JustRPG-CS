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
        emb.AddField($"Уровень", $"{user.lvl}", inline: true)
            .AddField("Опыт", $"{(int)user.exp}\\{(int)user.expToLvl}", inline: true)
            .AddField("Баланс", $"{user.cash}", inline: true)
            .AddField("Очки рейтинга", $"{user.mmr}", inline: true)
            .AddField(name: "Статы",
                value:
                $"<:health:997889169567260714> : {user.stats.hp} |  <:strength:997889205684420718> : {user.stats.damage} " +
                $"| <:armor:997889166673186987> : {user.stats.defence} \n<:dexterity:997889168216694854> : {user.stats.speed} " +
                $"| <:luck:997889165221957642> : {user.stats.luck}");
        return emb.Build();
    }

    public static async Task<Embed> UserEquipmentEmbed(User user, IUser member, DataBase dataBase)
    {
        var embed = new EmbedBuilder
        {
            Title = $"Экипировка {await user.GetFullName(member.Username, dataBase)}"
        };
        UserEquipment equipment = await user.GetEquipmentAsItems(dataBase!);

        embed.AddField(equipment.helmet == null ? "Шлем" : $"Шлем - {equipment.helmet!.name}",
                equipment.helmet == null ? "Не надето" : equipment.helmet!.ToString(), true)
            .AddField(equipment.armor == null ? "Нагрудник" : $"Нагрудник - {equipment.armor!.name}",
                equipment.armor == null ? "Не надето" : equipment.armor!.ToString(), true)
            .AddField(equipment.pants == null ? "Штаны" : $"Штаны - {equipment.pants!.name}",
                equipment.pants == null ? "Не надето" : equipment.pants!.ToString(), true)
            .AddField(equipment.shoes == null ? "Ботинки" : $"Ботинки - {equipment.shoes!.name}",
                equipment.shoes == null ? "Не надето" : equipment.shoes!.ToString(), true)
            .AddField(equipment.gloves == null ? "Перчатки" : $"Перчатки - {equipment.gloves!.name}",
                equipment.gloves == null ? "Не надето" : equipment.gloves!.ToString(), true)
            .AddField(equipment.weapon == null ? "Оружие" : $"Оружие - {equipment.weapon!.name}",
                equipment.weapon == null ? "Не надето" : equipment.weapon!.ToString(), true);

        return embed.Build();
    }

    public static Embed UpSkills()
    {
        var embed = new EmbedBuilder
        {
            Title = "Прокачка навыков",
            Description = "Уровень навыка не может превышать уровень персонажа\n" +
                          "<:health:997889169567260714> - увеличивается только за счёт экипировки\n" +
                          "<:armor:997889166673186987> - принимают на себя весь урон с его частичным уменьшением\n" +
                          "<:dexterity:997889168216694854> - увеличивает вероятность уклонения\n" +
                          "<:luck:997889165221957642> - увеличивает получаемый опыт и монеты\n"
        };
        return embed.Build();
    }

    public static async Task<Embed> UserInventory(IUser member, User user, Item?[] items, DataBase dataBase)
    {
        var emb = new EmbedBuilder { Title = $"Инвентарь {await user.GetFullName(member.Username, dataBase)}" };
        foreach (var item in items)
        {
            if (item == null)
                emb.AddField("Пусто", "Слот не занят");
            else
                emb.AddField($"{item.lvl} | {item.name}", item.ToString());
        }

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
                currentWarrior = battle.players[battle.currentUser];
                selectedEnemy = battle.players[battle.currentUser == 1 ? 0 : 1];
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
                $"<:health:997889169567260714> - {progressBar(currentWarrior.stats.hp, currentWarrior.stats.MaxHP)}\n " +
                $"<:armor:997889166673186987> - {progressBar(currentWarrior.stats.defence, currentWarrior.stats.MaxDef)}\n" +
                $"<:strength:997764094125953054> - {currentWarrior.stats.damage}");

            embed.AddField($"{selectedEnemy.fullName} - {selectedEnemy.lvl}",
                $"<:health:997889169567260714> - {progressBar(selectedEnemy.stats.hp, selectedEnemy.stats.MaxHP)}\n " +
                $"<:armor:997889166673186987> - {progressBar(selectedEnemy.stats.defence, selectedEnemy.stats.MaxDef)}\n" +
                $"<:strength:997764094125953054> - {selectedEnemy.stats.damage}");
        }

        if (battle.log != "")
            embed.AddField("Логи", $"```{battle.log}```");

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


        return embed.Build();
    }

    public static Embed MarketPage(MarketSearchState searchState)
    {
        var emb = new EmbedBuilder { Title = "Маркет" };
        List<SaleItem> items = searchState.GetItemsOnPage(searchState.currentPage);
        for (int i = 0; i < 5; i++)
        {
            if (i >= items.Count)
                emb.AddField("-", "-");
            else
                emb.AddField(
                    (searchState.currentItemIndex == i ? "💠 " : "") +
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
                    $"{items[i].itemName} | {items[i].price}<:silver:997889161484828826>",
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
}