using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules.Selects;

public class MarketSortSelect  : IInteractionMaster
{
    private readonly DiscordSocketClient _client;
    private readonly SocketMessageComponent _component;
    private readonly DataBase _dataBase;

    public MarketSortSelect(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    public async Task Distributor(string[] buttonInfo)
    {
        switch (buttonInfo[2])
        {
            case "byLvl":
                await SelectLvl(buttonInfo);
                break;
            case "byRaty":
                await SelectRaty(buttonInfo);
                break;
            case "byType":
                await SelectType(buttonInfo);
                break;
        }
    }

    public async Task SelectLvl(string[] buttonInfo)
    {
        SearchState search = (await _dataBase.MarketDb.GetSearch(buttonInfo[3]))!;
        string[] res = _component.Data.Values.ToArray()[0].Split('-');
        
        search.itemLvl = new Tuple<int, int>(Convert.ToInt32( res[0]),Convert.ToInt32( res[1]));
        
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }
    
    public async Task SelectRaty(string[] buttonInfo)
    {
        SearchState search = (await _dataBase.MarketDb.GetSearch(buttonInfo[1]))!;
        string res = _component.Data.Values.ToArray()[0];
        
        
        if (res == "сброс")
            search.itemRarity = null;
        else if (res == "обычное")
            search.itemRarity = "common";
        else if (res == "необычное")
            search.itemRarity = "uncommon";
        else if (res == "редкое")
            search.itemRarity = "rare";
        else if (res == "эпическое")
            search.itemRarity = "epic";
        else if (res == "легендарное")
            search.itemRarity = "legendary";
        else
            search.itemRarity = null;
        
        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }
    
    public async Task SelectType(string[] buttonInfo)
    {
        SearchState search = (await _dataBase.MarketDb.GetSearch(buttonInfo[1]))!;
        string res = _component.Data.Values.ToArray()[0];
        
        
        if (res == "сброс")
            search.itemType = null;
        else if (res == "шлем")
            search.itemType = "helmet";
        else if (res == "нагрудник")
            search.itemType = "armor";
        else if (res == "перчатки")
            search.itemType = "gloves";
        else if (res == "штаны")
            search.itemType = "pants";
        else if (res == "оружие")
            search.itemType = "weapon";
        else if (res == "зелья")
            search.itemType = "posion";
        else
            search.itemType = null;
        
        
        
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