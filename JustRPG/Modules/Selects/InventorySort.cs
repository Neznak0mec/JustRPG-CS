using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Models.Enums;
using JustRPG.Services;
using Serilog;

namespace JustRPG.Modules.Selects;

public class InventorySort : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private readonly DiscordSocketClient _client;
    private readonly DataBase _dataBase;
    private Inventory _inventory;

    public InventorySort(IServiceProvider service)
    {
         _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
         _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }
    
    public override async Task<Task> BeforeExecuteAsync(ICommandInfo command)
    {
        var buttonInfo = Context.Interaction.Data.CustomId.Split('_');
        _inventory = (Inventory)(await _dataBase.InventoryDb.Get($"Inventory_{buttonInfo[1]}_{buttonInfo[2]}"))!;
        return base.BeforeExecuteAsync(command);
    }

    [ComponentInteraction("Inventory|byLvl_*_*", true)]
    public async Task SelectLvl(string finder, string userId, string[] selected)
    {
        string[] res = selected[0].Split('-');

        _inventory.itemLvl = new Tuple<int, int>(Convert.ToInt32(res[0]), Convert.ToInt32(res[1]));

        await UpdateMessage(finder, userId);
    }

    [ComponentInteraction("Inventory|byRaty_*_*", true)]
    public async Task SelectRaty(string finder, string userId, string[] selected)
    {
        string res = selected[0];

        _inventory.itemRarity = res switch
        {
            "обычное" => Rarity.common,
            "необычное" => Rarity.uncommon,
            "редкое" => Rarity.rare,
            "эпическое" => Rarity.epic,
            "легендарное" => Rarity.legendary,
            _ => null
        };

        await UpdateMessage( finder, userId);
    }


    [ComponentInteraction("Inventory|byType_*_*", true)]
    public async Task SelectType(string finder, string userId, string[] selected)
    {
        string res = selected[0];

        _inventory.itemType = res switch
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

        await UpdateMessage(finder, userId);
    }


    private async Task UpdateMessage(string finder, string userId)
    {
        var dbUser = (User)(await _dataBase.UserDb.Get(Convert.ToUInt64(userId)))!;
        var member = _client.GetUser(Convert.ToUInt64(userId));
        
        _inventory = (Inventory)(await _dataBase.InventoryDb.Get($"Inventory_{finder}_{userId}"))!;
        await _inventory.Sort(_dataBase);
        _dataBase.InventoryDb.Update(_inventory);
        
        var items = _inventory!.GetItems();
        
        var embed = await EmbedCreater.UserInventory(member!, dbUser!, _inventory, _dataBase);

        await Context.Interaction.UpdateAsync(x =>
            {
                x.Embed = embed;
                x.Components = ButtonSets.InventoryButtonsSet(finder, dbUser!.id, _inventory, items);
            }
        );

    }
}