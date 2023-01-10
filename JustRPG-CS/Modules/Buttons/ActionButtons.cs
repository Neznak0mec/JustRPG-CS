using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Services;
using Serilog;
using Action = JustRPG.Models.Action;

namespace JustRPG.Modules.Responce;

public class ActionButtons
{
    private DiscordSocketClient _client;
    private SocketMessageComponent _component;
    private readonly DataBase _dataBase;
    private Action? _action;
    private User _dbUser;

    public ActionButtons(DiscordSocketClient client, SocketMessageComponent component, object? service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service!;
    }

    public async Task Distributor(string[] buttonInfo)
    {
        Log.Debug($"Action_{buttonInfo[2]}");
        _action = (Action)_dataBase.ActionDb.Get($"Action_{buttonInfo[2]}");

        if (_action == null)
        {
            await _component.UpdateAsync(x =>
            {
                x.Embed = EmbedCreater.ErrorEmbed("Неизвестная ошибка, попробуйте ещё раз");
                x.Components = null;
            });
            return;
        }

        _dbUser = (User)_dataBase.UserDb.Get(Convert.ToUInt64(buttonInfo[1]))!;
        
        if (buttonInfo[3] == "Denied")
        {
            await DeniedAction();
            return;
        }
        
        switch (_action.type)
        {
            case "Sell":
                await AcceptSell();
                break;
            case "Equip":
                await AcceptEquip();
                break;
        }
    }

    private async Task DeniedAction()
    {
        await _component.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.EmpEmbed("Действие было отменено");
            x.Components = null;
        });
    }

    private async Task AcceptSell()
    {
        Embed embed;
        if (_dbUser.inventory.Contains(_action!.args[0]))
        {
            Item item = (Item)_dataBase.ItemDb.Get(_action.args[0])!;
            int itemIndex = Array.IndexOf(_dbUser.inventory,item.id);
            _dbUser.inventory = _dbUser.inventory.Where((x,y) => y != itemIndex).ToArray();
            _dbUser.cash += item.price / 4;
            
            
            embed = EmbedCreater.SuccessEmbed($"Вы успешно продали `{item.name}` за `{item.price / 4}`");
            _dataBase.UserDb.Update(_dbUser);
        }
        else
        {
            embed = EmbedCreater.ErrorEmbed("Данный предмет не найден в вашем инвентаре");
        }
        
        await _component.UpdateAsync(x => {
            x.Embed = embed;
            x.Components = null;
        });
    }

    private async Task AcceptEquip()
    {
        Embed embed;
        if (_dbUser.inventory.Contains(_action!.args[0]))
        {
            Item item = (Item)_dataBase.ItemDb.Get(_action.args[0])!;
            string? itemToChangeId = _dbUser.equipment!.GetByName(item.type);

            if (itemToChangeId != null)
            {
                int indexInInventory = Array.IndexOf(_dbUser.inventory, item.id);
                _dbUser.inventory[indexInInventory] = itemToChangeId;
                _dbUser.equipment.SetByName(item.type,item.id);

                var itemToChange = (Item)_dataBase.ItemDb.Get(itemToChangeId)!;

                embed = EmbedCreater.SuccessEmbed($"Вы успешно сняли `{itemToChange.name}` и надели `{item.name}`");
            }
            else
            {
                int itemIndex = Array.IndexOf(_dbUser.inventory,item.id);
                _dbUser.inventory = _dbUser.inventory.Where((x,y) => y != itemIndex).ToArray();
                _dbUser.equipment.SetByName(item.type,item.id);
                
                embed = EmbedCreater.SuccessEmbed($"Вы успешно сняли надели `{item.name}`");
            }
            
            _dataBase.UserDb.Update(_dbUser);
        }
        else
        {
            embed = EmbedCreater.ErrorEmbed("Данный предмет не найден в вашем инвентаре");
        }
        
        await _component.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = null;
        });
    }
}