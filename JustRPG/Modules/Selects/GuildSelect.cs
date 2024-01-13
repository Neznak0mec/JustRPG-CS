using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Exceptions;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Models.Enums;
using JustRPG.Services;

namespace JustRPG.Modules.Selects;

public class GuildSelect : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private readonly DiscordSocketClient _client;
    private readonly DataBase _dataBase;

    public GuildSelect(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    [ComponentInteraction($"Guild|joinType_*_*", true)]
    public async Task SelectOfficers(string userId,string tag ,string[] selected)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(tag))!;
        GuildMember? moderator = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        if (moderator == null)
        {
            throw new UserInteractionException("Вы не являетесь участником этой гильдии");
        }
        
        if (moderator.rank < GuildRank.owner)
        {
            throw new UserInteractionException("У вас недостаточно прав для этого действия");
        }

        JoinType joinType = selected[0] switch
        {
            "open" => JoinType.open,
            "invite" => JoinType.invite,
            _ => JoinType.close
        };

        guild.join_type = joinType;
        await _dataBase.GuildDb.Update(guild);
        
        await UpdateMessage(embed: EmbedCreater.GuildEmbed(guild),components:ButtonSets.GuildComponents(guild,Context.User.Id));
    }
    
    private async Task UpdateMessage(Embed? embed = null, MessageComponent? components = null)
    {
        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = components;
        });
    }
}