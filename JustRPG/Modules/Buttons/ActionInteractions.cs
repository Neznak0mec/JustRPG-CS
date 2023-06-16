using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;
using Serilog;
using Action = JustRPG.Models.Action;

namespace JustRPG.Modules.Buttons;

public class ActionInteractions : IInteractionMaster
{
    private DiscordSocketClient _client;
    private SocketMessageComponent _component;
    private readonly DataBase _dataBase;
    private Action? _action;
    private User? _dbUser;

    public ActionInteractions(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    public async Task Distributor(string[] buttonInfo)
    {
        _action = (Action)(await _dataBase.ActionDb.Get($"Action_{buttonInfo[2]}"))!;

        if (_action == null)
        {
            await _component.UpdateAsync(x =>
            {
                x.Embed = EmbedCreater.ErrorEmbed("Неизвестная ошибка, попробуйте ещё раз");
                x.Components = null;
            });
            return;
        }

        _dbUser = (User) (await _dataBase.UserDb.Get(Convert.ToUInt64(buttonInfo[1])))!;
        
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
            case "Destroy":
                await AcceptDestroy();
                break;
            case "MarketBuy":
                await MarketBuy();
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
        if (_dbUser!.inventory.Contains(_action!.args[0]))
        {
            Item item = (Item) (await _dataBase.ItemDb.Get(_action.args[0]))!;
            int itemIndex = Array.IndexOf(_dbUser.inventory,item.id);
            _dbUser.inventory = _dbUser.inventory.Where((x,y) => y != itemIndex).ToArray();
            _dbUser.cash += item.price / 4;
            
            embed = EmbedCreater.SuccessEmbed($"Вы успешно продали `{item.name}` за `{item.price / 4}`");
            await _dataBase.UserDb.Update(_dbUser);
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
        if (_dbUser!.inventory.Contains(_action!.args[0]))
        {
            Item item = (Item) (await _dataBase.ItemDb.Get(_action.args[0]))!;
            string? itemToChangeId = _dbUser.equipment!.GetByName(item.type);

            if (itemToChangeId != null)
            {
                int indexInInventory = Array.IndexOf(_dbUser.inventory, item.id);
                _dbUser.inventory[indexInInventory] = itemToChangeId;
                _dbUser.equipment.SetByName(item.type,item.id);

                var itemToChange = (Item) (await _dataBase.ItemDb.Get(itemToChangeId))!;

                embed = EmbedCreater.SuccessEmbed($"Вы успешно сняли `{itemToChange.name}` и надели `{item.name}`");
            }
            else
            {
                int itemIndex = Array.IndexOf(_dbUser.inventory,item.id);
                _dbUser.inventory = _dbUser.inventory.Where((x,y) => y != itemIndex).ToArray();
                _dbUser.equipment.SetByName(item.type,item.id);
                
                embed = EmbedCreater.SuccessEmbed($"Вы успешно сняли надели `{item.name}`");
            }
            
            await _dataBase.UserDb.Update(_dbUser);
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

    private async Task AcceptDestroy()
    {
        Embed embed;
        if (_dbUser!.inventory.Contains(_action!.args[0]))
        {
            Item item = (Item) (await _dataBase.ItemDb.Get(_action.args[0]))!;
            int itemIndex = Array.IndexOf(_dbUser.inventory,item.id);
            _dbUser.inventory = _dbUser.inventory.Where((x,y) => y != itemIndex).ToArray();

            embed = EmbedCreater.SuccessEmbed($"Вы успешно уничтожили `{item.name}`");
            await _dataBase.UserDb.Update(_dbUser);
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

    private async Task MarketBuy()
    {
        var temp = await _dataBase.MarketDb.Get(_action!.args[0]);
        SaleItem item;
        User user;

        if (temp == null)
        {
            await _component.UpdateAsync(x=> x.Embed = EmbedCreater.ErrorEmbed("Предмет не найден, возможно он уже был продан или снят с продажи"));
            return;
        }
        item = (SaleItem)(temp!);
        user = (User)(await _dataBase.UserDb.Get(_component.User.Id))!;

        if (user.cash < item.price)
        {
            await _component.UpdateAsync(x=> x.Embed = EmbedCreater.ErrorEmbed("У вас недостаточно средств для продажи"));
            return;
        }
        if (user.inventory.Length >= 30)
        {
            await _component.UpdateAsync(x=> x.Embed = EmbedCreater.ErrorEmbed("У вас недостаточно места в инвентаре"));
            return;
        }


        user.cash -= item.price;
        List<string> inventary = user.inventory.ToList();
        inventary.Add(item.itemId);
        user.inventory = inventary.ToArray();

        User seller = (User)(await _dataBase.UserDb.Get(item.userId))!      ;
        seller.cash += item.price;

        await _dataBase.UserDb.Update(user);
        await _dataBase.UserDb.Update(seller);
        await _dataBase.MarketDb.Delete(item);

        await _component.UpdateAsync(x=>
        {
            x.Embed = EmbedCreater.SuccessEmbed("Предмет приобретён");
            x.Components = null;
        });
    }
}