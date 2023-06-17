using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;
using MongoDB.Driver.Core.WireProtocol.Messages;

namespace JustRPG.Modules.Modals;

public class InventoryModals : IModalMaster
{
    DiscordSocketClient _client;
    SocketModal _modal;
    DataBase _dataBase;


    public InventoryModals(DiscordSocketClient client, SocketModal modal, IServiceProvider service)
    {
        _client = client;
        _modal = modal;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    public async Task Distributor(string[] modalInfo)
    {
        switch (modalInfo[1])
        {
            case "SetSellItemPrice":
                await SetSellItemPrice(modalInfo);
                break;
            default:
                await WrongInteraction("Что-то пошло не так");
                break;
        }
    }

    private async Task SetSellItemPrice(string[] modalInfo)
    {
        List<SocketMessageComponentData> components = _modal.Data.Components.ToList();

        string stringPrice = components.First(x => x.CustomId == "price").Value;
        int price;
        try
        {
            price = Convert.ToInt32(stringPrice);
            if (price < 0)
                throw new Exception();
        }
        catch (Exception)
        {
            await WrongInteraction("Цена предмета должна быть числом, и быть более 0");
            return;
        }

        SaleItem saleItem = (SaleItem)(await _dataBase.MarketDb.Get(modalInfo[2]))!;
        saleItem.price = price;
        saleItem.isVisible = true;
        await _dataBase.MarketDb.Update(saleItem);

        await _modal.RespondAsync(embed: EmbedCreater.SuccessEmbed($"Цена на предмет успешно изменена на {price}"),
            ephemeral: true);
    }

    private async Task WrongInteraction(string text)
    {
        await _modal.RespondAsync(embed: EmbedCreater.ErrorEmbed(text), ephemeral: true);
    }
}