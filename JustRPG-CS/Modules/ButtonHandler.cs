using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Modules.Responce;
using Serilog;

namespace JustRPG;

public class ButtonHandler
{
    private SocketMessageComponent _component;
    private ProfileButtons _profileButtons;

    public ButtonHandler(DiscordSocketClient client, SocketMessageComponent component, object service)
    {
        _component = component;
        _profileButtons = new ProfileButtons(client, component, service);
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

    }

    private async Task WrongInteraction()
    {
        await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не можете с этим взаимодействовать"));
    }
    

    
}