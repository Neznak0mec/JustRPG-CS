using Discord.WebSocket;
using JustRPG_CS.Modules.Buttons;
using JustRPG.Generators;
using JustRPG.Modules.Responce;
using Serilog;

namespace JustRPG.Services;

public class ButtonHandler
{
    private SocketMessageComponent _component;
    private ProfileButtons _profileButtons;
    private InventoryInteractions _inventoryInteractions;
    private ActionButtons _actionButtons;
    private BattleButtons _battleButtons;

    public ButtonHandler(DiscordSocketClient client, SocketMessageComponent component, object service)
    {
        Log.Information("тт");
        _component = component;
        _profileButtons = new ProfileButtons(client, component, service);
        _inventoryInteractions = new InventoryInteractions(client, component, service);
        _actionButtons = new ActionButtons(client, component, service);
        _battleButtons = new BattleButtons(client, component, service);
    }

    public async Task ButtonDistributor()
    {
        var buttonInfo = _component.Data.CustomId.Split('_');
        if (_component.User.Id.ToString() != buttonInfo[1])
        {
            await WrongInteraction();
            return;
        }

        if (buttonInfo[0] == "Profile" || buttonInfo[0] == "Equipment" || buttonInfo[0] == "Inventory" ||
            buttonInfo[0] == "UpSkills" || buttonInfo[0] == "UpSkill")
            await _profileButtons.Distributor(buttonInfo);

        if (buttonInfo[0].Contains("Inv"))
        {
            await _inventoryInteractions.Distributor(buttonInfo);
        }

        if (buttonInfo[0] == "Action")
        {
            await _actionButtons.Distributor(buttonInfo);
        }

        if (buttonInfo[0] == "Battle")
        {
            await _battleButtons.Distributor(buttonInfo);
        }
    }

    private async Task WrongInteraction()
    {
        await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не можете с этим взаимодействовать"), ephemeral:true);
    }
    

    
}