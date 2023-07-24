using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Modules.Buttons;

namespace JustRPG.Services;

public class ButtonHandler
{
    private DiscordSocketClient _client;
    private IServiceProvider _service;
    private SocketMessageComponent _component;


    public ButtonHandler(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _component = component;
        _client = client;
        _service = service;
    }

    public async Task ButtonDistributor()
    {
        IInteractionMaster master;

        var buttonInfo = _component.Data.CustomId.Split('_');
        if (_component.User.Id.ToString() != buttonInfo[1])
        {
            await WrongInteraction("Вы не можете с этим взаимодействовать");
            return;
        }

        switch (buttonInfo[0])
        {
            case "Profile":
            case "Equipment":
            case "Inventory":
                master = new ProfileInteractions(_client, _component, _service);
                break;
            case "Inventary":
                master = new InventoryInteractions(_client, _component, _service);
                break;
            case "Action":
                master = new ActionInteractions(_client, _component, _service);
                break;
            case "Battle":
                master = new BattleInteractions(_client, _component, _service);
                break;
            case "FindPvp":
                master = new FindPvpInteractions(_client, _component, _service);
                break;
            case "Market":
                master = new MarketInteractions(_client, _component, _service);
                break;
            case "MarketSort":
                master = new MarketSortInteractions(_client, _component, _service);
                break;
            case "Guild":
                master = new GuildInteractions(_client, _component, _service);
                break;
            default:
                await WrongInteraction("Кнопка не найдена, попробуйте вызвать меню ещё раз");
                return;
        }


        await master.Distributor(buttonInfo);
    }

    private async Task WrongInteraction(string text)
    {
        await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed(text), ephemeral: true);
    }
}