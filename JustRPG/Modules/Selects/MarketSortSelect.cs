using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules.Selects;

public class MarketSortSelect : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private readonly DiscordSocketClient _client;
    private readonly DataBase _dataBase;

    public MarketSortSelect(IServiceProvider service)
    {
         _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
         _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    [ComponentInteraction("MarketSort|byLvl_*", true)]
    public async Task SelectLvl(string userId, string[] selected)
    {
        MarketSearchState search = (await _dataBase.MarketDb.GetSearch(userId))!;
        string[] res = selected[0].Split('-');

        search.itemLvl = new Tuple<int, int>(Convert.ToInt32(res[0]), Convert.ToInt32(res[1]));

        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }

    [ComponentInteraction("MarketSort|byRaty_*", true)]
    public async Task SelectRaty(string userId, string[] selected)
    {
        MarketSearchState search = (await _dataBase.MarketDb.GetSearch(userId))!;
        string res = selected[0];


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


    [ComponentInteraction("MarketSort|byType_*", true)]
    public async Task SelectType(string userId, string[] selected)
    {
        MarketSearchState search = (await _dataBase.MarketDb.GetSearch(userId))!;
        string res = selected[0];


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


    public async Task UpdateMessage(MarketSearchState search)
    {
        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.MarketPage(search);
            x.Components = ButtonSets.MarketSortComponents(Context.User.Id, search.id);
        });
    }
}