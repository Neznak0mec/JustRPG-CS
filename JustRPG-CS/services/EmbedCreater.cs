using Discord;
using JustRPG_CS.Classes;
using Serilog;

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
}