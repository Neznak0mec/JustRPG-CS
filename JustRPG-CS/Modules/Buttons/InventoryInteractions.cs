using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;
using Action = JustRPG.Models.Action;

namespace JustRPG.Modules.Buttons;

public class InventoryInteractions : IInteractionMaster
{
    private readonly DiscordSocketClient _client;
    private readonly SocketMessageComponent _component;
    private readonly DataBase _dataBase;
    private Inventory? _inventory;
    private User _dbUser;
    private SocketUser _member;

    public InventoryInteractions(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;

    }
    
    
    public async Task Distributor(string[] buttonInfo)
    {
        _inventory = (Inventory) (await _dataBase.InventoryDb.Get($"Inventory_{buttonInfo[1]}_{buttonInfo[2]}"))!;
        _inventory.DataBase = _dataBase;
        
        _dbUser = (User) (await _dataBase.UserDb.Get(Convert.ToUInt64(buttonInfo[2])))!;
        _member = _client.GetUser(Convert.ToUInt64(buttonInfo[2]));
        
        
        switch (buttonInfo[0])
        {
            case "InvPrewPage":
                await PreviousPage(buttonInfo[1]);
                return;
            case "InvNextPage":
                await NextPage(buttonInfo[1]);
                return;
            case "InvReload":
                await Reload(buttonInfo[1]);
                return;
            case "InvInteractionType":
                await ChangeInteractionType(buttonInfo[1]);
                return;
        }

        if (buttonInfo[0].StartsWith("InvInfo"))
        {
            await ItemInfo(buttonInfo[3]);
        }
        else if (buttonInfo[0].StartsWith("InvEquip"))
        {
            await EquipItem(buttonInfo[3]);
        }
        else
        {
            await SellItem(buttonInfo[3]);
        }
    }

    private async Task PreviousPage(string finder)
    {
        _inventory!.PreviousPage();
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

    private async Task ChangeInteractionType(string finder)
    {
        var interaction = string.Join("", _component.Data.Values);  
        _inventory!.interactionType = interaction;
        await UpdateMessage(finder);
    }

    private async Task ItemInfo(string buttonInfo)
    {
        var itemId = _inventory!.currentPageItems[Convert.ToInt16(buttonInfo)];
        
        Embed embed;
        if (itemId == null)
        {
            embed = EmbedCreater.ErrorEmbed("Произошла ошибка, инвентрать будет обновлён");
            _inventory.Reload(_dbUser.inventory);
        }
        else
        {
            var item = await _dataBase.ItemDb.Get(itemId);
            embed = item == null ? EmbedCreater.ErrorEmbed("Этот предмет не найден, странно 🤔") : EmbedCreater.ItemInfo((Item)item);
        }

        await _component.RespondAsync(embed: embed, ephemeral: true);
    }

    private async Task UpdateMessage(string finder)
    {
        var items = await _inventory!.GetItems(_dataBase);
        await _component.UpdateAsync(
            x =>
            {
                x.Embed = new EmbedCreater().UserInventory(_member, items);
                x.Components = ButtonSets.InventoryButtonsSet(finder, _dbUser.id, _inventory, items);
            }
        );
        
    }

    private async Task EquipItem(string buttonInfo)
    {
        var itemId = _inventory!.currentPageItems[Convert.ToInt16(buttonInfo)];
        Item? itemToChange = null;
        Embed embed;
        Action? action;
        
        string uId = Guid.NewGuid().ToString().Split("-")[^1];
        
        if (itemId == null)
        {
//            embed = EmbedCreater.ErrorEmbed("Произошла ошибка, инвентрать будет обновлён");
            _inventory.Reload(_dbUser.inventory);
            return;
        }
        object? item = await _dataBase.ItemDb.Get(itemId);

        //todo Проверка на возможность экипировать
        if (item != null)
        {
            Item tempItem = (Item)item;
            string? idItemToChange = _dbUser.equipment!.GetByName(tempItem.type);
            
            action = new Action
            {
                id = "Action_"+uId,
                date = DateTimeOffset.Now.ToUnixTimeSeconds(),
                userId = _dbUser.id,
                type = "Equip",
                args = new[]
                {
                    itemId,"null"
                }
            };

            
            if (idItemToChange != null)
            {
                itemToChange = (Item) (await _dataBase.ItemDb.Get(idItemToChange))!;
                action.args[1] = itemToChange.id;
            }

            embed = EmbedCreater.WarningEmbed(idItemToChange != null ?
                $"Вы уверены что хотите снять `{itemToChange!.name}` и надеть `{tempItem.name}` ?" :
                $"Вы уверены что хотите надеть `{tempItem.name}` ?");
        }
        else
        {
            embed = EmbedCreater.ErrorEmbed("Этот предмет не найден, странно 🤔");
            action = null;
        }

        if (action != null)
        {
            await _dataBase.ActionDb.CreateObject(action);
            await _component.RespondAsync(embed: embed,components: ButtonSets.AcceptActions(uId, _dbUser.id), ephemeral: true);
        }
        else
        {
            await _component.RespondAsync(embed: embed, ephemeral: true);
        }
        
    }

    private async Task SellItem(string buttonInfo)
    {
        var itemId = _inventory!.currentPageItems[Convert.ToInt16(buttonInfo)];
        object? item = null;
        Embed embed;
        Action? action;
        
        string uId = Guid.NewGuid().ToString().Split("-")[^1];
        
        if (itemId == null)
        {
            _inventory.Reload(_dbUser.inventory);
        }
        else
            item = await _dataBase.ItemDb.Get(itemId);
        
        if (item != null)
        {
            Item tempItem = (Item)item;
            
            
            action = new Action
            {
                id = "Action_"+uId,
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
            await _dataBase.ActionDb.CreateObject(action);
            await _component.RespondAsync(embed: embed,components: ButtonSets.AcceptActions(uId,_dbUser.id), ephemeral: true);
        }
        else
        {
            await _component.RespondAsync(embed: embed, ephemeral: true);
        }
    }
}