using Discord;
using JustRPG_CS.Classes;

namespace JustRPG_CS;

public class EmbedCreater
{
    private DataBase? _dataBase;

    public EmbedCreater(DataBase? dataBase = null)
    {
        _dataBase = dataBase;
    }
    
    
    public static Embed ErrorEmbed(string text)
    {
        var emb = new EmbedBuilder();
        emb.Title = "🚫 Ошибка";
        emb.Color = Color.Red;
        emb.Description = text;
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

    public static Embed UserProfile(User user, Discord.IUser member)
    {
        var emb = new EmbedBuilder();
        emb.Title = $"Профиль {member.Username}";
        emb.AddField($"Уровень", $"{user.lvl}", inline: true)
            .AddField("Опыт", $"{Math.Round(user.exp_to_lvl,2)}\\{(int)user.exp}", inline: true)
            .AddField("Баланс", $"{user.cash}", inline: true)
            .AddField("Очки навыков", $"{user.skill_points}", inline:true)
            .AddField(name: "Статы", value:$"<:health:997889169567260714> : {user.hp} |  <:strength:997889205684420718> : {user.damage} " +
                                          $"| <:armor:997889166673186987> : {user.defence} \n<:dexterity:997889168216694854> : {user.speed} " +
                                          $"| <:luck:997889165221957642> : {user.luck} | <:crit:997889163552628757> : {user.krit}");
        return emb.Build();
    }
    
    public Embed UserEqipment(User user, Discord.IUser member)
    {
        var emb = new EmbedBuilder();
        emb.Title = $"Экипировка {member.Username}";
        UserEquipment equipment = user.GetEquipmentAsItems(_dataBase!);

        emb.AddField(equipment.helmet  == null ? "Шлем"      : $"Шлем - {equipment.helmet!.name}",     equipment.helmet == null ? "Не надето" : equipment.helmet!.GetStatsAsString(), true)
            .AddField(equipment.armor  == null ? "Нагрудник" : $"Нагрудник - {equipment.armor!.name}", equipment.armor  == null ? "Не надето" : equipment.armor!.GetStatsAsString(), true)
            .AddField(equipment.pants  == null ? "Штаны"     : $"Штаны - {equipment.pants!.name}",     equipment.pants  == null ? "Не надето" : equipment.pants!.GetStatsAsString(), true)
            .AddField(equipment.shoes  == null ? "Ботинки"   : $"Ботинки - {equipment.shoes!.name}",   equipment.shoes  == null ? "Не надето" : equipment.shoes!.GetStatsAsString(), true)
            .AddField(equipment.gloves == null ? "Перчатки"  : $"Перчатки - {equipment.gloves!.name}", equipment.gloves == null ? "Не надето" : equipment.gloves!.GetStatsAsString(), true)
            .AddField(equipment.weapon == null ? "Оружие"    : $"Оружие - {equipment.weapon!.name}",   equipment.weapon == null ? "Не надето" : equipment.weapon!.GetStatsAsString(), true);
 
        return emb.Build();
    }
}