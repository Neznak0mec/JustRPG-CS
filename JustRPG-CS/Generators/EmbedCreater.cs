using System.Reflection.Emit;
using Discord;
using JustRPG_CS.Models;
using JustRPG.Models;
using JustRPG.Models.SubClasses;
using JustRPG.Services;
using JustRPG.Features;

namespace JustRPG.Generators;

public class EmbedCreater
{
    private readonly DataBase? _dataBase;

    public EmbedCreater(DataBase? dataBase = null)
    {
        _dataBase = dataBase;
    }
    
    
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
            Color = Color.DarkOrange,
            Description = text
        };
        return emb.Build();
    } 

    public static Embed UserProfile(User user, IUser member)
    {
        var emb = new EmbedBuilder
        {
            Title = $"Профиль {member.Username}"
        };
        emb.AddField($"Уровень", $"{user.lvl}", inline: true)
            .AddField("Опыт", $"{Math.Round(user.expToLvl,2)}\\{(int)user.exp}", inline: true)
            .AddField("Баланс", $"{user.cash}", inline: true)
            .AddField("Очки рейтинга", $"{user.mmr}",inline:true)
            .AddField("Очки навыков", $"{user.skillPoints}", inline:true)
            .AddField(name: "Статы", value:$"<:health:997889169567260714> : {user.stats.hp} |  <:strength:997889205684420718> : {user.stats.damage} " +
                                          $"| <:armor:997889166673186987> : {user.stats.defence} \n<:dexterity:997889168216694854> : {user.stats.speed} " +
                                          $"| <:luck:997889165221957642> : {user.stats.luck}");
        return emb.Build();
    }
    
    public async Task<Embed> UserEquipment(User user, IUser member)
    {
        var embed = new EmbedBuilder
        {
            Title = $"Экипировка {member.Username}"
        };
        UserEquipment equipment = await user.GetEquipmentAsItems(_dataBase!);

        embed.AddField(equipment.helmet  == null ? "Шлем"      : $"Шлем - {equipment.helmet!.name}",     equipment.helmet == null ? "Не надето" : equipment.helmet!.ToString(), true)
            .AddField(equipment.armor  == null ? "Нагрудник" : $"Нагрудник - {equipment.armor!.name}", equipment.armor  == null ? "Не надето" : equipment.armor!.ToString(), true)
            .AddField(equipment.pants  == null ? "Штаны"     : $"Штаны - {equipment.pants!.name}",     equipment.pants  == null ? "Не надето" : equipment.pants!.ToString(), true)
            .AddField(equipment.shoes  == null ? "Ботинки"   : $"Ботинки - {equipment.shoes!.name}",   equipment.shoes  == null ? "Не надето" : equipment.shoes!.ToString(), true)
            .AddField(equipment.gloves == null ? "Перчатки"  : $"Перчатки - {equipment.gloves!.name}", equipment.gloves == null ? "Не надето" : equipment.gloves!.ToString(), true)
            .AddField(equipment.weapon == null ? "Оружие"    : $"Оружие - {equipment.weapon!.name}",   equipment.weapon == null ? "Не надето" : equipment.weapon!.ToString(), true);
 
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

    public Embed UserInventory(IUser member,Item?[] items)
    {
        var emb = new EmbedBuilder{Title = $"Инвентарь {member.Username}"};
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
            "common" => 0xffffff,
            "uncommon" => 0x0033cc,
            "rare" => 0x6600ff,
            "epic" => 0xffcc00,
            "legendary" => 0xcc0000,
            "impossible" => 0x000000,
            "exotic" =>  0xcc0066,
            "prize" => 0xcccc00,
            "event" => 0x666600,
            _ => 0xffffff
        };

        emb.Color = temp;
            
        return emb.Build();
    }

    public static Embed WorkEmbed(List<Work> works,int exp, int cash)
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
            case "adventure" or "dungeon":
                currentWarrior = battle.players[battle.currentUser];
                selectedEnemy = battle.enemies[battle.selectedEnemy];
                break;
            case "arena":
                currentWarrior = battle.players[battle.currentUser];
                selectedEnemy = battle.players[battle.currentUser == 1 ? 0 : 1];
                break;
            default:
                return ErrorEmbed("wha ?");
        }

        embed = new EmbedBuilder
        {
            Title = $"Бой {currentWarrior.name} - {selectedEnemy.name}",
            Description = battle.type == "arena" ? $"Сейчас ходит {currentWarrior.name}" : ""
        };


        if (battle.type is "adventure" or "dungeon")
            embed.WithThumbnailUrl(selectedEnemy.url);
        else
            embed.WithThumbnailUrl(currentWarrior.url);

        if (!gameEnded)
        {
            embed.AddField($"{currentWarrior.name} - {currentWarrior.lvl}",
                $"<:health:997889169567260714> - {progressBar(currentWarrior.stats.hp,currentWarrior.stats.MaxHP)}\n " +
                $"<:armor:997889166673186987> - {progressBar(currentWarrior.stats.defence,currentWarrior.stats.MaxDef)}\n" +
                $"<:strength:997764094125953054> - {currentWarrior.stats.damage}");

            embed.AddField($"{selectedEnemy.name} - {selectedEnemy.lvl}",
                $"<:health:997889169567260714> - {progressBar(selectedEnemy.stats.hp,selectedEnemy.stats.MaxHP)}\n " +
                $"<:armor:997889166673186987> - {progressBar(selectedEnemy.stats.defence,selectedEnemy.stats.MaxDef)}\n" +
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
            Description = "Чем дольше вы ждёте битву тем более сильнее или слабее противник может попасться"
        };
        embed.AddField("Ваш mmr", $"```{pvp.mmr}```", inline: true);
        embed.AddField("Время начала поиска", $"<t:{pvp.stratTime}:R>", inline:true);
        if (count != -1)
            embed.AddField("В поиске на моент начала", $"```{count}```");


        return embed.Build();
    }
}