using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Features;
using JustRPG.Generators;
using JustRPG.Services;

namespace JustRPG.Modules.Buttons;

public class FindPvpInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>> {
    private DiscordSocketClient _client;
    private readonly DataBase _dataBase;


    public FindPvpInteractions(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    [ComponentInteraction("FindPvp_*_CancelFind", true)]
    private async Task CancelFind(string userId)
    {
        _dataBase.ArenaDb.DeletFindPVP((long)Context.User.Id);
        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.EmpEmbed("Поиск боя отменён");
            x.Components = null;
        });
        await Task.Delay(5000);
        var a = await GetOriginalResponseAsync();
        await a.DeleteAsync();
    }
}