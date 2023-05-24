using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Modules.Responce;
using Serilog;

namespace JustRPG.Services;

public class SelectHandler
{
    private SocketMessageComponent _component;
    private InventoryInteractions _profileButtons;
    private SelectLocation _selectLocation;
    
    public SelectHandler(DiscordSocketClient client, SocketMessageComponent component, object service)
    {
        _component = component;
        _profileButtons = new InventoryInteractions(client, component, service);
        _selectLocation = new SelectLocation(client, component, service);
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
            await _profileButtons.Distributor(buttonInfo);

        if (buttonInfo[0] == "SelectLocation")
            await _selectLocation.Distributor(buttonInfo);

    }
    
    private async Task WrongInteraction()
    {
        await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не можете с этим взаимодействовать"));
    }
    
}