using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Modules.Buttons;
using JustRPG.Modules.Selects;
using Serilog;

namespace JustRPG.Services;

public class SelectHandler
{
    private DiscordSocketClient _client;
    private IServiceProvider _service;
    private SocketMessageComponent _component;

    public SelectHandler(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _component = component;
        _client = client;
        _service = service;
    }

    public async Task SelectDistributor()
    {
        var selectInfo = _component.Data.CustomId.Split('_');
        if (_component.User.Id.ToString() != selectInfo[1])
        {
            await WrongInteraction();
            return;
        }

        IInteractionMaster master;

        if (selectInfo[0] == "Inventary")
            master = new InventoryInteractions(_client, _component, _service);

        else if (selectInfo[0] == "SelectLocation")
            master = new SelectLocation(_client, _component, _service);

        else if (selectInfo[0] == "Battle")
            master = new BattleInteractions(_client, _component, _service);

        else if (selectInfo[0] == "MarketSort")
        {
            master = new MarketSortSelect(_client, _component, _service);
        }

        else
            throw new Exception("wtf");

        await master.Distributor(selectInfo);
    }

    private async Task WrongInteraction()
    {
        await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не можете с этим взаимодействовать"));
    }
}