using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules.Buttons;

public class MarketInteractions : IInteractionMaster
{
    private readonly DiscordSocketClient _client;
    private readonly SocketMessageComponent _component;
    private readonly DataBase _dataBase;

    public MarketInteractions(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    public async Task Distributor(string[] buttonInfo)
    {
        switch (buttonInfo[2])
        {
            case "prewItem":
                await PreviousItem(buttonInfo);
                break;
            case "nextItem":
                await NextItem(buttonInfo);
                break;
            case "editPrice":
                await EditPrice(buttonInfo);
                break;
            case "editVisible":
                await EditVisible(buttonInfo);
                break;
            case "Remove":
                await RemoveItem(buttonInfo);
                break;
            case "reloadPage":
                await ReloadPage(buttonInfo);
                break;
            case "goBack":
                await GoBack(buttonInfo);
                break;
        }
    }

    public async Task PreviousItem(string[] buttonInfo)
    {
        MarketSettings search = (await _dataBase.MarketDb.GetSettings(buttonInfo[1]))!;
        search.DecrementItemIndex();
        await _dataBase.MarketDb.GetUserSlots(search);
        await UpdateMessage(search);
    }

    public async Task NextItem(string[] buttonInfo)
    {
        MarketSettings search = (await _dataBase.MarketDb.GetSettings(buttonInfo[1]))!;
        search.IncrementItemIndex();
        await _dataBase.MarketDb.GetUserSlots(search);
        await UpdateMessage(search);
    }

    private async Task EditPrice(string[] buttonInfo)
    {
        SaleItem? saleItem = await GetItem(buttonInfo);
        if (saleItem == null)
            return;

        ModalBuilder modalBuilder = new ModalBuilder()
            .WithTitle($"Продажа предмета {saleItem.itemName}")
            .WithCustomId($"Inventory_SetSellItemPrice_{saleItem.id}")
            .AddTextInput(label: "Цена продажи", placeholder: "Введите цену за которую хотете продать предмет",
                customId: "price", required: true, minLength: 1);

        await _component.RespondWithModalAsync(modalBuilder.Build());
    }

    private async Task EditVisible(string[] buttonInfo)
    {
        SaleItem? saleItem = await GetItem(buttonInfo);
        if (saleItem == null)
            return;


        saleItem.isVisible = !saleItem.isVisible;
        await _dataBase.MarketDb.Update(saleItem);

        MarketSettings search = (await _dataBase.MarketDb.GetSettings(buttonInfo[1]))!;

        await _dataBase.MarketDb.GetUserSlots(search);
        await UpdateMessage(search);
    }

    private async Task RemoveItem(string[] buttonInfo)
    {
        SaleItem? saleItem = await GetItem(buttonInfo);
        if (saleItem == null)
            return;

        await _dataBase.MarketDb.Delete(saleItem);
        User user = (User)(await _dataBase.UserDb.Get(buttonInfo[1]))!;
        user.inventory.Add(saleItem.itemId);

        await _dataBase.UserDb.Update(user);

        MarketSettings search = (await _dataBase.MarketDb.GetSettings(buttonInfo[1]))!;

        await _dataBase.MarketDb.GetUserSlots(search);
        await UpdateMessage(search);
    }

    private async Task ReloadPage(string[] buttonInfo)
    {
        MarketSettings search = (await _dataBase.MarketDb.GetSettings(buttonInfo[1]))!;

        await _dataBase.MarketDb.GetUserSlots(search);
        await UpdateMessage(search);
    }

    private async Task GoBack(string[] buttonInfo)
    {
        MarketSettings settings = (await _dataBase.MarketDb.GetSettings(buttonInfo[1]))!;

        if (settings.startPage == "market")
        {
            SearchState searchState = new SearchState()
            {
                id = Guid.NewGuid().ToString(),
                userId = _component.User.Id
            };
            await _dataBase.MarketDb.CreateSearch(searchState);

            await _dataBase.MarketDb.SearchGetAndUpdate(searchState);

            await _component.UpdateAsync(x =>
            {
                x.Embed = EmbedCreater.MarketPage(searchState);
                x.Components = ButtonSets.MarketSortComponents(_component.User.Id, searchState.id);
            });
            return;
        }
        else
        {
            Inventory inventory =
                (Inventory)(await _dataBase.InventoryDb.Get($"Inventory_{_component.User.Id}_{_component.User.Id}"))!;
            var items = await inventory.GetItems(_dataBase);


            await _component.UpdateAsync(x =>
            {
                x.Embed = EmbedCreater.UserInventory(_component.User, items);
                x.Components = ButtonSets.InventoryButtonsSet(_component.User.Id.ToString(), (long)_component.User.Id,
                    inventory, items);
            });
            return;
        }
    }

    async Task<SaleItem?> GetItem(string[] buttonInfo)
    {
        SaleItem? saleItem = null;
        MarketSettings settings = (await _dataBase.MarketDb.GetSettings(buttonInfo[1]))!;
        if (buttonInfo.Length == 3 && settings.searchResults.Count != 0)
        {
            saleItem = settings.searchResults[settings.currentItemIndex];
        }
        else
        {
            try
            {
                saleItem = (SaleItem)(await _dataBase.MarketDb.Get(buttonInfo[3]))!;
            }
            catch (Exception)
            {
                await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Предмет не найден"));
                return null;
            }
        }

        return saleItem;
    }

    public async Task UpdateMessage(MarketSettings settings)
    {
        await _component.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.MarketSettingsPage(settings);
            x.Components = ButtonSets.MarketSettingComponents(settings);
        });
    }
}