using Discord.Interactions;
using Discord.WebSocket;
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

    [ComponentInteraction($"Guild|officers_*_*", true)]
    public async Task SelectOfficers(string userId,string tag ,string[] selected)
    {
    }
}