using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;
using Action = JustRPG.Models.Action;

namespace JustRPG.Modules.Buttons;

public class MarketSortInteractions  : IInteractionMaster
{
    private readonly DiscordSocketClient _client;
    private readonly SocketMessageComponent _component;
    private readonly DataBase _dataBase;

    public MarketSortInteractions(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!; 
    }

    public async Task Distributor(string[] buttonInfo)
    {
        switch (buttonInfo[2])
        {
            case "prewPage":
                await PreviousPage(buttonInfo);
                break;
            case "prewItem":
                await PreviousItem(buttonInfo);
                break;
            case "buyItem":
                await BuyItem(buttonInfo);
                break;
            case "nextItem":
                await NextItem(buttonInfo);
                break;
            case "nextPage":
                await NextPage(buttonInfo);
                break;
            case "reloadPage":
                await ReloadPage(buttonInfo);
                break;
                
            case "priceUp":
                await PriceUp(buttonInfo);
                break;
                
            case "priceDown":
                await PriceDown(buttonInfo);
                break;
                
            case "openSlotsSettings":
                await OpenSlotsSettings(buttonInfo);
                break;
        }
    }

    private async Task OpenSlotsSettings(string[] buttonInfo)
    {
        MarketSettings marketSettings = new MarketSettings
        {
            userId = _component.User.Id,
            id = Guid.NewGuid().ToString(),
            startPage = "market"
        };
        
        await _dataBase.MarketDb.GetUserSlots(marketSettings);
        await _dataBase.MarketDb.CreateSettings(marketSettings);
        
        await _component.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.MarketSettingsPage(marketSettings);
            x.Components = ButtonSets.MarketSettingComponents(marketSettings);
            });
    }

    private async Task PriceDown(string[] buttonInfo)
    {
        SearchState search = (await _dataBase.MarketDb.GetSearch(buttonInfo[1]))!;
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        search.SearchResults.Sort((x,y) => x.price - y.price);
        await UpdateMessage(search);
    }

    private async Task PriceUp(string[] buttonInfo)
    {
        SearchState search = (await _dataBase.MarketDb.GetSearch(buttonInfo[1]))!;
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        search.SearchResults.Sort((x,y) => y.price - x.price);
        await UpdateMessage(search);
    }

    public async Task ReloadPage(string[] buttonInfo)
    {
        SearchState search = (await _dataBase.MarketDb.GetSearch(buttonInfo[1]))!;
        search.CurrentPage=0;
        search.CurrentItemIndex=0;
        search.itemLvl = null;
        search.itemRarity = null;
        search.itemType = null;
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }

    public async Task PreviousPage(string[] buttonInfo)
    {
        SearchState search = (await _dataBase.MarketDb.GetSearch(buttonInfo[1]))!;
        search.DecrementPage();
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }
    
    public async Task PreviousItem(string[] buttonInfo)
    {
        SearchState search = (await _dataBase.MarketDb.GetSearch(buttonInfo[1]))!;
        search.DecrementItemIndex();
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }
    
    public async Task BuyItem(string[] buttonInfo)
    {
        SearchState search = (await _dataBase.MarketDb.GetSearch(buttonInfo[1]))!;
        SaleItem item;
        try
        {
            item = search.GetItemsOnPage(search.CurrentPage)[search.CurrentItemIndex];
        }
        catch (Exception)
        {
            await _component.RespondAsync(embed:EmbedCreater.ErrorEmbed("Предмет не найден, обновите поиск"),ephemeral:true);
            return;
        }
        
        string uId = Guid.NewGuid().ToString();
        
        Action action = new Action
            {
                id = "Action_"+uId,
                date = DateTimeOffset.Now.ToUnixTimeSeconds(),
                type = "MarketBuy",
                userId = (long)_component.User.Id,
                args = new[]
                {
                    item.id
                }
            };
        
        await _dataBase.ActionDb.CreateObject(action);
        Embed embed = EmbedCreater.WarningEmbed($"Вы уверены что хотите уничтожыть `{item.itemName}` за `{item.price}`?");
        await _component.RespondAsync(embed: embed,components: ButtonSets.AcceptActions(uId,(long)_component.User.Id), ephemeral: true);
    }
    
    public async Task NextItem(string[] buttonInfo)
    {
        SearchState search = (await _dataBase.MarketDb.GetSearch(buttonInfo[1]))!;
        search.IncrementItemIndex();
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }
    
    public async Task NextPage(string[] buttonInfo)
    {
        SearchState search = (await _dataBase.MarketDb.GetSearch(buttonInfo[1]))!;
        search.IncrementPage();
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }
    
    public async Task UpdateMessage(SearchState search)
    {
        await _component.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.MarketPage(search);
            x.Components =ButtonSets.MarketSortComponents(_component.User.Id,search.id);
            });
    }
}