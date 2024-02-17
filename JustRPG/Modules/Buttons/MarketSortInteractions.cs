using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Exceptions;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Services;
using Action = JustRPG.Models.Action;

namespace JustRPG.Modules.Buttons;

public class MarketSortInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private readonly DiscordSocketClient _client;
    private readonly DataBase _dataBase;


    public MarketSortInteractions(IServiceProvider service)
    {
       _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
       _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    [ComponentInteraction("MarketSort|openSlotsSettings_*", true)]
    private async Task OpenSlotsSettings(string userId)
    {
        MarketSlotsSettings marketSettings = new MarketSlotsSettings
        {
            userId = Context.User.Id,
            id = Guid.NewGuid().ToString(),
            startPage = "market"
        };

        await _dataBase.MarketDb.GetUserSlots(marketSettings);
        await _dataBase.MarketDb.CreateSettings(marketSettings);

        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.MarketSettingsPage(marketSettings);
            x.Components = ButtonSets.MarketSettingComponents(marketSettings);
        });
    }

    [ComponentInteraction("MarketSort|priceDown_*", true)]
    private async Task PriceDown(string userId)
    {
        MarketSearchState search = (await _dataBase.MarketDb.GetSearch(userId))!;
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        search.Items.Sort((x, y) => x.price - y.price);
        await UpdateMessage(search);
    }

    [ComponentInteraction("MarketSort|priceUp_*", true)]
    private async Task PriceUp(string userId)
    {
        MarketSearchState search = (await _dataBase.MarketDb.GetSearch(userId))!;
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        search.Items.Sort((x, y) => y.price - x.price);
        await UpdateMessage(search);
    }

    [ComponentInteraction("MarketSort|reloadPage_*", true)]
    public async Task ReloadPage(string userId)
    {
        MarketSearchState search = (await _dataBase.MarketDb.GetSearch(userId))!;
        search.CurrentPage = 0;
        search.CurrentItemIndex = 0;
        search.itemLvl = null;
        search.itemRarity = null;
        search.itemType = null;
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }

    [ComponentInteraction("MarketSort|prewPage_*", true)]
    public async Task PreviousPage(string userId)
    {
        MarketSearchState search = (await _dataBase.MarketDb.GetSearch(userId))!;
        search.DecrementPage();
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }

    [ComponentInteraction("MarketSort|prewItem_*", true)]
    public async Task PreviousItem(string userId)
    {
        MarketSearchState search = (await _dataBase.MarketDb.GetSearch(userId))!;
        search.DecrementItemIndex();
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }

    [ComponentInteraction("MarketSort|buyItem_*", true)]
    public async Task BuyItem(string userId)
    {
        MarketSearchState search = (await _dataBase.MarketDb.GetSearch(userId))!;
        SaleItem item;
        try
        {
            item = search.GetItemsOnPage(search.CurrentPage)[search.CurrentItemIndex];
        }
        catch (Exception)
        {
            throw new UserInteractionException("Предмет не найден, обновите поиск");
        }

        string uId = Guid.NewGuid().ToString();

        Action action = new Action
        {
            id = "Action_" + uId,
            date = DateTime.Now,
            type = "MarketBuy",
            userId = (long)Context.User.Id,
            args = new[]
            {
                item.id
            }
        };

        _dataBase.ActionDb.CreateObject(action);
        Embed embed =
            EmbedCreater.WarningEmbed($"Вы уверены что хотите купить `{item.itemName}` за `{item.price}`<:silver:997889161484828826>?");
        await RespondAsync(embed: embed, components: ButtonSets.AcceptActions(uId, (long)Context.User.Id),
            ephemeral: true);
    }

    [ComponentInteraction("MarketSort|nextItem_*", true)]
    public async Task NextItem(string userId)
    {
        MarketSearchState search = (await _dataBase.MarketDb.GetSearch(userId))!;
        search.IncrementItemIndex();
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }

    [ComponentInteraction("MarketSort|nextPage_*", true)]
    public async Task NextPage(string userId)
    {
        MarketSearchState search = (await _dataBase.MarketDb.GetSearch(userId))!;
        search.IncrementPage();
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }

    public async Task UpdateMessage(MarketSearchState search)
    {
        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.MarketPage(search);
            x.Components = ButtonSets.MarketSortComponents(Context.User.Id, search.id);
        });
    }
}