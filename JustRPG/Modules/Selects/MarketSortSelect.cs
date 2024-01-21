using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Models.Enums;
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

        search.itemRarity = res switch
        {
            "обычное" => Rarity.common,
            "необычное" => Rarity.uncommon,
            "редкое" => Rarity.rare,
            "эпическое" => Rarity.epic,
            "легендарное" => Rarity.legendary,
            _ => null
        };

        await _dataBase.MarketDb.SearchGetAndUpdate(search);
        await UpdateMessage(search);
    }


    [ComponentInteraction("MarketSort|byType_*", true)]
    public async Task SelectType(string userId, string[] selected)
    {
        MarketSearchState search = (await _dataBase.MarketDb.GetSearch(userId))!;
        string res = selected[0];

        search.itemType = res switch
        {
            "шлем" => ItemType.helmet,
            "нагрудник" => ItemType.armor,
            "перчатки" => ItemType.gloves,
            "штаны" => ItemType.pants,
            "ботинки" => ItemType.shoes,
            "оружие" => ItemType.weapon,
            "зелья" => ItemType.potion,
            _ => null
        };

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