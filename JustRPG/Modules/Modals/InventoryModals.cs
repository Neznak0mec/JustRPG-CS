using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules.Modals;

public class InventoryModals : InteractionModuleBase<SocketInteractionContext<SocketModal>>
{
    private DiscordSocketClient _client;
    private readonly DataBase _dataBase;


    public InventoryModals(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    [ModalInteraction("Inventory_SetSellItemPrice_*", true)]
    private async Task SetSellItemPrice(string id, SellItemModal modal)
    {
        int price;
        try
        {
            price = Convert.ToInt32(modal.Price);
            if (price < 0)
                throw new Exception();
        }
        catch (Exception)
        {
            await WrongInteraction("Цена предмета должна быть числом, и быть более 0");
            return;
        }

        SaleItem saleItem = (SaleItem)(await _dataBase.MarketDb.Get(id))!;
        saleItem.price = price;
        saleItem.isVisible = true;
        await _dataBase.MarketDb.Update(saleItem);

        await RespondAsync(embed: EmbedCreater.SuccessEmbed($"Цена на предмет успешно изменена на {price}"),
            ephemeral: true);
    }

    private async Task WrongInteraction(string text)
    {
        await RespondAsync(embed: EmbedCreater.ErrorEmbed(text), ephemeral: true);
    }
}