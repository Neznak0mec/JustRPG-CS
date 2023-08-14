using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules.Buttons;

public class GuildInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private DiscordSocketClient _client;
    private readonly DataBase _dataBase;


    public GuildInteractions(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }


//    [ComponentInteraction("Guild_*_join_*", true)]
    private async Task LeaveGuild(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        if (!guild.members.Contains(Context.User.Id))
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не участник этой гильдии"),
                ephemeral: true);
            return;
        }

        User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;

        user.guildTag = null;

        guild.members.Remove(Context.User.Id);

        await _dataBase.GuildDb.Update(guild);
        await _dataBase.UserDb.Update(user);

        await RespondAsync(embed: EmbedCreater.SuccessEmbed("Вы вышли из гильдии"), ephemeral: true);
    }

//    [ComponentInteraction("Guild_*_leave_*", true)]
    private async Task JoinGuild(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        if (guild.members.Contains(Context.User.Id))
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы уже участник гильдии"), ephemeral: true);
            return;
        }
        
        User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;

        user.guildTag = guild.tag;

        guild.members.Add(Context.User.Id);

        await _dataBase.GuildDb.Update(guild);
        await _dataBase.UserDb.Update(user);

        await RespondAsync(embed: EmbedCreater.SuccessEmbed("Вы ступили в гильдию"), ephemeral: true);
    }
}