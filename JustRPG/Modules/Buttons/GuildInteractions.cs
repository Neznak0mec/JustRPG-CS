using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules.Buttons;

public class GuildInteractions : IInteractionMaster
{
    private DiscordSocketClient _client;
    private readonly SocketMessageComponent _component;
    private readonly DataBase _dataBase;


    public GuildInteractions(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    public async Task Distributor(string[] buttonInfo)
    {
        switch (buttonInfo[2])
        {
            case "join":
                await JoinGuild(buttonInfo);
                break;
            case "leave":
                await LeaveGuild(buttonInfo);
                break;
        }
    }

    private async Task LeaveGuild(string[] buttonInfo)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(buttonInfo[3]))!;
        if (!guild.members.Contains(_component.User.Id))
        {
            await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не участник этой гильдии"),
                ephemeral: true);
            return;
        }

        User user = (User)(await _dataBase.UserDb.Get(_component.User.Id))!;

        user.guildTag = null;

        guild.members.Remove(_component.User.Id);

        await _dataBase.GuildDb.Update(guild);
        await _dataBase.UserDb.Update(user);

        await _component.RespondAsync(embed: EmbedCreater.SuccessEmbed("Вы вышли из гильдии"), ephemeral: true);
    }

    private async Task JoinGuild(string[] buttonInfo)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(buttonInfo[3]))!;
        if (guild.members.Contains(_component.User.Id))
        {
            await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы уже участник гильдии"), ephemeral: true);
            return;
        }
        
        User user = (User)(await _dataBase.UserDb.Get(_component.User.Id))!;

        user.guildTag = guild.tag;

        guild.members.Add(_component.User.Id);

        await _dataBase.GuildDb.Update(guild);
        await _dataBase.UserDb.Update(user);

        await _component.RespondAsync(embed: EmbedCreater.SuccessEmbed("Вы ступили в гильдию"), ephemeral: true);
    }
}