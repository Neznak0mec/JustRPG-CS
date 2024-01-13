using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Exceptions;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules.Buttons;

public class MarketInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private readonly DiscordSocketClient _client;
    private readonly DataBase _dataBase;

    public MarketInteractions(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    [ComponentInteraction("Market|prewItem_*", true)]
    public async Task PreviousItem(string userId)
    {
        MarketSlotsSettings search = (await _dataBase.MarketDb.GetSettings(userId))!;
        search.DecrementItemIndex();
        await _dataBase.MarketDb.GetUserSlots(search);
        await UpdateMessage(search);
    }

    [ComponentInteraction("Market|nextItem_*", true)]
    public async Task NextItem(string userId)
    {
        MarketSlotsSettings search = (await _dataBase.MarketDb.GetSettings(userId))!;
        search.IncrementItemIndex();
        await _dataBase.MarketDb.GetUserSlots(search);
        await UpdateMessage(search);
    }


    [ComponentInteraction("Market|setPrice_*_*", true)]
    private async Task SetPrice(string userId, string itemId)
    {
        SaleItem? saleItem = await GetItemById(userId, itemId);
        if (saleItem == null)
            return;

        ModalBuilder modalBuilder = new ModalBuilder()
            .WithTitle($"Продажа {saleItem.itemName}")
            .WithCustomId($"Inventory_SetSellItemPrice_{saleItem.id}")
            .AddTextInput(label: "Цена продажи", placeholder: "Введите цену за которую хотете продать предмет",
                customId: "price", required: true, minLength: 1);

        await RespondWithModalAsync(modalBuilder.Build());
    }

    [ComponentInteraction("Market|updatePrice_*", true)]
    private async Task UpdatePrice(string userId)
    {
        SaleItem? saleItem = await GetSelectedItem(userId);
        if (saleItem == null)
            return;

        ModalBuilder modalBuilder = new ModalBuilder()
            .WithTitle($"Продажа {saleItem.itemName}")
            .WithCustomId($"Inventory_SetSellItemPrice_{saleItem.id}")
            .AddTextInput(label: "Цена продажи", placeholder: "Введите цену за которую хотете продать предмет",
                customId: "price", required: true, minLength: 1);

        await RespondWithModalAsync(modalBuilder.Build());
    }

    [ComponentInteraction("Market|editVisible_*", true)]
    private async Task EditVisible(string userId)
    {
        SaleItem? saleItem = await GetSelectedItem(userId);
        if (saleItem == null)
            return;

        if (saleItem.price <= 0)
        {
            throw new UserInteractionException("Невозможно выставить на продажу предмет за бесплатно или с отрицательной ценой");
        }

        saleItem.isVisible = !saleItem.isVisible;
        await _dataBase.MarketDb.Update(saleItem);

        MarketSlotsSettings search = (await _dataBase.MarketDb.GetSettings(userId))!;

        await _dataBase.MarketDb.GetUserSlots(search);
        await UpdateMessage(search);
    }

    [ComponentInteraction("Market|Remove_*", true)]
    private async Task RemoveItem(string userId)
    {
        SaleItem? saleItem = await GetSelectedItem(userId);
        if (saleItem == null)
            return;

        await _dataBase.MarketDb.Delete(saleItem);
        User user = (User)(await _dataBase.UserDb.Get(userId))!;
        user.inventory.Add(saleItem.itemId);

        await _dataBase.UserDb.Update(user);

        MarketSlotsSettings search = (await _dataBase.MarketDb.GetSettings(userId))!;

        await _dataBase.MarketDb.GetUserSlots(search);

        search.currentItemIndex--;
        if (search.currentItemIndex < 0)
            search.currentItemIndex = 0;
        await UpdateMessage(search);
    }

    [ComponentInteraction("Market|reloadPage_*", true)]
    private async Task ReloadPage(string userId)
    {
        MarketSlotsSettings search = (await _dataBase.MarketDb.GetSettings(userId))!;

        await _dataBase.MarketDb.GetUserSlots(search);
        await UpdateMessage(search);
    }

    [ComponentInteraction("Market|goBack_*", true)]
    private async Task GoBack(string userId)
    {
        MarketSlotsSettings settings = (await _dataBase.MarketDb.GetSettings(userId))!;

        if (settings.startPage == "market")
        {
            MarketSearchState searchState = new MarketSearchState()
            {
                id = Guid.NewGuid().ToString(),
                userId = Context.User.Id
            };
            await _dataBase.MarketDb.CreateSearch(searchState);

            await _dataBase.MarketDb.SearchGetAndUpdate(searchState);

            await Context.Interaction.UpdateAsync(x =>
            {
                x.Embed = EmbedCreater.MarketPage(searchState);
                x.Components = ButtonSets.MarketSortComponents(Context.User.Id, searchState.id);
            });
            return;
        }
        else
        {
            Inventory inventory =
                (Inventory)(await _dataBase.InventoryDb.Get($"Inventory_{Context.User.Id}_{Context.User.Id}"))!;
            var items = inventory.GetItems();
            User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;

            var embed = await EmbedCreater.UserInventory(Context.User, user, items, _dataBase);
            
            await Context.Interaction.UpdateAsync(x =>
            {
                x.Embed = embed;
                x.Components = ButtonSets.InventoryButtonsSet(Context.User.Id.ToString(), (long)Context.User.Id,
                    inventory, items);
            });
        }
    }

    async Task<SaleItem?> GetSelectedItem(string userId)
    {
        SaleItem? saleItem = null;
        MarketSlotsSettings settings = (await _dataBase.MarketDb.GetSettings(userId))!;
        if (settings.searchResults.Count != 0)
        {
            saleItem = settings.searchResults[settings.currentItemIndex];
        }
        else
        {
            throw new UserInteractionException("Предмет не найден");
        }

        return saleItem;
    }


    async Task<SaleItem?> GetItemById(string userId, string itemId)
    {
        SaleItem? saleItem = (SaleItem?)await _dataBase.MarketDb.Get(itemId);
        if (saleItem == null)
        {
            throw new UserInteractionException("Предмет не найден");
        }

        return saleItem;
    }

    public async Task UpdateMessage(MarketSlotsSettings settings)
    {
        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.MarketSettingsPage(settings);
            x.Components = ButtonSets.MarketSettingComponents(settings);
        });
    }
}