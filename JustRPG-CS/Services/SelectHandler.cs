using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Modules.Buttons;
using Serilog;

namespace JustRPG.Services;

public class SelectHandler
{
    
    private DiscordSocketClient _client;
    private object _service;
    private SocketMessageComponent _component;
    IInteractionMaster master;
    public SelectHandler(DiscordSocketClient client, SocketMessageComponent component, object service)
    {
        _component = component;
        _client = client;
        _service = service;
    }
    
    public async Task SelectDistributor()
    {
        var buttonInfo = _component.Data.CustomId.Split('_');
        if (_component.User.Id.ToString() != buttonInfo[1])
        {
            await WrongInteraction();
            return;
        }

        if (buttonInfo[0].StartsWith("InvInteractionType"))
            master = new InventoryInteractions(_client, _component, _service);

        if (buttonInfo[0] == "SelectLocation")
            master = new SelectLocation(_client, _component, _service);

        if (buttonInfo[0] == "Battle")
            master = new BattleInteractions(_client, _component, _service);

        await master.Distributor(buttonInfo);
    }
    
    private async Task WrongInteraction()
    {
        await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не можете с этим взаимодействовать"));
    }
    
}