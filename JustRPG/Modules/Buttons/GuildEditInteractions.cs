using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Services;

namespace JustRPG.Modules.Buttons;

public class GuildEditInteractions: InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private DiscordSocketClient _client;
    private readonly DataBase _dataBase;
    
    public GuildEditInteractions(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }
    
    [ComponentInteraction($"Guild_*_*_edit_*", true)]
    private async Task GuildMain(string userId, string guildTag,string field)
    {
        
    }
}