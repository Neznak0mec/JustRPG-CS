using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Modules.Modals;

namespace JustRPG.Services;

public class ModalHandler
{
    private DiscordSocketClient _client;
    private IServiceProvider _service;
    private SocketModal _modal;


    public ModalHandler(DiscordSocketClient client, SocketModal component, IServiceProvider service)
    {
        _modal = component;
        _client = client;
        _service = service;
    }


    public async Task ModalDistributor()
    {
        var modalInfo = _modal.Data.CustomId.Split('_');
        IModalMaster master;


        switch (modalInfo[0])
        {
            case "Inventory":
                master = new InventoryModals(_client, _modal, _service);
                break;

            default:
                await WrongInteraction("Данная форма не найдена, пропробуйте чуть позже");
                return;
        }

        await master.Distributor(modalInfo);
    }

    private async Task WrongInteraction(string text)
    {
        await _modal.RespondAsync(embed: EmbedCreater.ErrorEmbed(text), ephemeral: true);
    }
}