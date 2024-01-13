using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Services;

namespace JustRPG.Modules;

public class OtherCommands: InteractionModuleBase<SocketInteractionContext>
{
    private readonly DataBase _dataBase;
    private readonly DiscordSocketClient _client;

    public OtherCommands(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase?)service.GetService(typeof(DataBase))!;
    }
    
    // [SlashCommand("ping", "Reciave a ping message")]
    // public async Task Ping()
    // {
    //     await RespondAsync(
    //         $"{(DateTimeOffset.Now - Context.Interaction.CreatedAt).TotalMilliseconds} ms to server");
    // }
    
    [SlashCommand("help", "Помощь по командам")]
    public async Task Help()
    {
        IUser owner = await _client.GetUserAsync(426986442632462347);
        await RespondAsync(embed: EmbedCreater.HelpEmbed(owner));
    }
    
    [SlashCommand("top", "Топ игроков на арене")]
    public async Task Top()
    {
        await RespondAsync(embed: await EmbedCreater.TopEmbed(_dataBase, _client));
    }
}