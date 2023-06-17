using Discord;
using Discord.Rest;
using Discord.WebSocket;
using JustRPG.Features;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Services;
using MongoDB.Driver.Core.WireProtocol.Messages;

namespace JustRPG.Modules.Buttons;

public class FindPvpInteractions : IInteractionMaster {
    private DiscordSocketClient _client;
    private readonly SocketMessageComponent _component;
    private readonly DataBase _dataBase;
    private readonly Background _background;


    public FindPvpInteractions(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
        _background = (Background)service.GetService(typeof(Background))!;
    }

    public async Task Distributor(string[] buttonInfo)
    {
        if (buttonInfo[2] == "CancelFind")
        {
            await CancelFind();
            
        }
    }

    private async Task CancelFind()
    {
        _dataBase.ArenaDb.DeletFindPVP((long)_component.User.Id);
        await _component.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.EmpEmbed("Поиск боя отменён");
            x.Components = null;
        });
        await Task.Delay(5000);
        var a = await _component.GetOriginalResponseAsync();
        await a.DeleteAsync();
    }
}