using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Services;
using Serilog;
using Action = JustRPG.Models.Action;

namespace JustRPG.Modules.Responce;

public class InventoryInteractions
{
    private DiscordSocketClient _client;
    private SocketMessageComponent _component;
    private DataBase _dataBase;
    private Inventory? _inventory;
    private User _dbUser;
    private SocketUser _member;

    public InventoryInteractions(DiscordSocketClient client, SocketMessageComponent component, object? service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service!;

    }
    
    
    public async Task Distributor(string[] buttonInfo)
    {
        _inventory = (Inventory)_dataBase.InventoryDb.Get($"Inventory_{buttonInfo[1]}_{buttonInfo[2]}")!;
        _inventory.DataBase = _dataBase;
        
        _dbUser = (User)_dataBase.UserDb.Get(Convert.ToUInt64(buttonInfo[2]))!;
        _member = _client.GetUser(Convert.ToUInt64(buttonInfo[2]));
        
        
        switch (buttonInfo[0])
        {
            case "InvPrewPage":
                await PreviousPage(buttonInfo[1]);
                break;
            case "InvNextPage":
                await NextPage(buttonInfo[1]);
                break;
            case "InvReload":
                await Reload(buttonInfo[1]);
                break;
            case "InvInteractionType":
                await ChangeinteractionType(buttonInfo[1]);
                break;
        }

        if (buttonInfo[0].StartsWith("InvInfo"))
        {
            await ItemInfo(buttonInfo[0]);
        }
        else if (buttonInfo[0].StartsWith("InvEquip"))
        {
            await EquipItem(buttonInfo[0]);
        }
        else
        {
            await SellItem(buttonInfo[0]);
        }
    }

    private async Task PreviousPage(string finder)
    {
        _inventory!.PrewPage();
        await UpdateMessage(finder);
    }
    
    private async Task NextPage(string finder)
    {
        _inventory!.NextPage();
        await UpdateMessage(finder);
    }

    private async Task Reload(string finder)
    {
        _inventory!.Reload(_dbUser.inventory);
        await UpdateMessage(finder);
    }

    private async Task ChangeinteractionType(string finder)
    {
        var interaction = string.Join("", _component.Data.Values);  
        _inventory!.interactionType = interaction;
        await UpdateMessage(finder);
    }

    private async Task ItemInfo(string buttonInfo)
    {
        var itemId = _inventory!.currentPageItems[Convert.ToInt32(buttonInfo[^1].ToString())];
        
        Embed embed;
        if (itemId == null)
        {
            embed = EmbedCreater.ErrorEmbed("Произошла ошибка, инвентрать будет обновлён");
            _inventory.Reload(_dbUser.inventory);
        }
        else
        {
            var item = _dataBase.ItemDb.Get(itemId);
            embed = item == null ? EmbedCreater.ErrorEmbed("Этот предмет не найден, странно 🤔") : EmbedCreater.ItemInfo((Item)item);
        }

        await _component.RespondAsync(embed: embed, ephemeral: true);
    }

    private async Task UpdateMessage(string finder)
    {
        var items = _inventory!.GetItems(_dataBase);
        await _component.UpdateAsync(
            x =>
            {
                x.Embed = new EmbedCreater().UserInventory(_member, items);
                x.Components = ButtonSets.InventoryButtonsSet(finder, _dbUser, _inventory, items);
            }
        );
        
    }

    private async Task EquipItem(string buttonInfo)
    {
        var itemId = _inventory!.currentPageItems[Convert.ToInt32(buttonInfo[^1].ToString())];
        object? item = null;
        Item? itemToChange = null;
        Embed embed;
        Action? action = null;
        
        string UID = Guid.NewGuid().ToString().Split("-")[^1];
        
        if (itemId == null)
        {
            embed = EmbedCreater.ErrorEmbed("Произошла ошибка, инвентрать будет обновлён");
            _inventory.Reload(_dbUser.inventory);
            return;
        }
        else
            item = _dataBase.ItemDb.Get(itemId);
        
        //todo Проверка на возможность экипировать

        if (item != null)
        {
            Item tempItem = (Item)item;
            string? idItemToChange = _dbUser.equipment!.GetByName(tempItem.type);
            
            action = new Action()
            {
                id = "Action_"+UID,
                date = DateTimeOffset.Now.ToUnixTimeSeconds(),
                userId = _dbUser.id,
                type = "Equip",
                args = new[]
                {
                    itemId!,"null"
                }
            };

            
            if (idItemToChange != null)
            {
                itemToChange = (Item)_dataBase.ItemDb.Get(idItemToChange)!;
                action.args[1] = itemToChange.id;
            }

            embed = EmbedCreater.WarningEmbed(idItemToChange != null ? $"Вы уверены что хотите снять `{itemToChange!.name}` и надеть `{tempItem.name}` ?" : $"Вы уверены что хотите надеть `{tempItem.name}` ?");
        }
        else
        {
            embed = EmbedCreater.ErrorEmbed("Этот предмет не найден, странно 🤔");
            action = null;
        }

        if (action != null)
        {
            _dataBase.ActionDb.CreateObject(action);
            await _component.RespondAsync(embed: embed,components: ButtonSets.AcceptActions(UID, _dbUser.id), ephemeral: true);
        }
        else
        {
            await _component.RespondAsync(embed: embed, ephemeral: true);
        }
        
    }

    private async Task SellItem(string buttonInfo)
    {
        var itemId = _inventory!.currentPageItems[Convert.ToInt32(buttonInfo[^1])];
        object? item = null;
        Embed embed;
        Action? action = null;
        
        string UID = Guid.NewGuid().ToString().Split("-")[^1];
        
        if (itemId == null)
        {
            embed = EmbedCreater.ErrorEmbed("Произошла ошибка, инвентрать будет обновлён");
            _inventory.Reload(_dbUser.inventory);
        }
        else
            item = _dataBase.ItemDb.Get(itemId);
        
        if (item != null)
        {
            Item tempItem = (Item)item;
            
            
            action = new Action()
            {
                id = "Action_"+UID,
                date = DateTimeOffset.Now.ToUnixTimeSeconds(),
                type = "Sell",
                userId = _dbUser.id,
                args = new[]
                {
                    tempItem.id
                }
            };
            
            embed = EmbedCreater.WarningEmbed($"Вы уверены что хотите продать `{tempItem.name}` за `{tempItem.price / 4}`?");
        }
        else
        {
            embed = EmbedCreater.ErrorEmbed("Этот предмет не найден, странно 🤔");
            action = null;
        }
        
        if (action != null)
        {
            _dataBase.ActionDb.CreateObject(action);
            await _component.RespondAsync(embed: embed,components: ButtonSets.AcceptActions(UID,_dbUser.id), ephemeral: true);
        }
        else
        {
            await _component.RespondAsync(embed: embed, ephemeral: true);
        }
    }
}