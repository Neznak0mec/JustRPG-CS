using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Exceptions;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Services;
using Action = JustRPG.Models.Action;

namespace JustRPG.Modules.Buttons;

public class ActionInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private readonly DataBase _dataBase;
    private Action? _action;
    private User? _dbUser;
    private readonly DiscordSocketClient _client;

    public ActionInteractions(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    public override async Task<Task> BeforeExecuteAsync(ICommandInfo command)
    {
        var buttonInfo = Context.Interaction.Data.CustomId.Split('_');
        _action = _dataBase.ActionDb.Get($"Action_{buttonInfo[2]}");
        if (_action == null)
        {
            throw new UserInteractionException("Действие не найдено. Создайте новое действие");
        }
        _dbUser = (User)(await _dataBase.UserDb.Get(Convert.ToUInt64(buttonInfo[1])))!;
        return base.BeforeExecuteAsync(command);
    }


    [ComponentInteraction("Action|Accept_*_*", true)]
    public async Task AcceptDistributor(string userId, string actionId)
    {
        switch (_action!.type)
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
            case "GuildLeave":
                await GuildLeave();
                break;
        }
    }

    [ComponentInteraction("Action|Denied_*_*", true)]
    private async Task DeniedAction(string userId, string actionId)
    {
        await Context.Interaction.UpdateAsync(x =>
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
            Item item = (Item)(await _dataBase.ItemDb.Get(_action.args[0]))!;

            int itemIndex = _dbUser.inventory.IndexOf(item.id);
            _dbUser.inventory = _dbUser.inventory.Where((x, y) => y != itemIndex).ToList();
            _dbUser.cash += item.price / 4;

            embed = EmbedCreater.SuccessEmbed($"Вы успешно продали `{item.name}` за `{item.price / 4}`");
            await _dataBase.UserDb.Update(_dbUser);
        }
        else
        {
            throw new UserInteractionException("Данный предмет не найден в вашем инвентаре");
        }

        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = null;
        });
    }

    private async Task AcceptEquip()
    {
        Embed embed;
        if (_dbUser!.inventory.Contains(_action!.args[0]))
        {
            Item item = (Item)(await _dataBase.ItemDb.Get(_action.args[0]))!;
            string? itemToChangeId = _dbUser.equipment!.GetByType(item.type);

            if (itemToChangeId != null)
            {
                int indexInInventory = _dbUser.inventory.IndexOf(item.id);
                _dbUser.inventory[indexInInventory] = itemToChangeId;
                _dbUser.equipment.SetByType(item.type, item.id);

                var itemToChange = (Item)(await _dataBase.ItemDb.Get(itemToChangeId))!;

                embed = EmbedCreater.SuccessEmbed($"Вы успешно сняли `{itemToChange.name}` и надели `{item.name}`");
            }
            else
            {
                int itemIndex = _dbUser.inventory.IndexOf(item.id);
                _dbUser.inventory = _dbUser.inventory.Where((x, y) => y != itemIndex).ToList();
                _dbUser.equipment.SetByType(item.type, item.id);

                embed = EmbedCreater.SuccessEmbed($"Вы успешно сняли надели `{item.name}`");
            }

            await _dataBase.UserDb.Update(_dbUser);
        }
        else
        {
            throw new UserInteractionException("Данный предмет не найден в вашем инвентаре");
        }

        await Context.Interaction.UpdateAsync(x =>
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
            Item item = (Item)(await _dataBase.ItemDb.Get(_action.args[0]))!;
            int itemIndex = _dbUser.inventory.IndexOf(item.id);
            _dbUser.inventory = _dbUser.inventory.Where((x, y) => y != itemIndex).ToList();

            embed = EmbedCreater.SuccessEmbed($"Вы успешно уничтожили `{item.name}`");
            await _dataBase.UserDb.Update(_dbUser);
        }
        else
        {
            throw new UserInteractionException("Данный предмет не найден в вашем инвентаре");
        }

        await Context.Interaction.UpdateAsync(x =>
        {
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
            throw new UserInteractionException("Предмет не найден, возможно он уже был продан или снят с продажи");
        }

        item = (SaleItem)(temp);
        user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;

        if (user.cash < item.price)
        {
            throw new UserInteractionException("У вас недостаточно средств для продажи");
        }

        if (user.inventory.Count >= 30)
        {
            throw new UserInteractionException("У вас недостаточно места в инвентаре");
        }


        user.cash -= item.price;
        user.inventory.Add(item.itemId);

        User seller = (User)(await _dataBase.UserDb.Get(item.userId))!;
        seller.cash += item.price - (int)(item.price * 0.05);

        await _dataBase.UserDb.Update(user);
        await _dataBase.UserDb.Update(seller);
        await _dataBase.MarketDb.Delete(item);

        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.SuccessEmbed("Предмет приобретён");
            x.Components = null;
        });
    }

    private async Task GuildLeave()
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(_action!.args[0]))!;
        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);

        User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;

        if (member == null || user.guildTag != guild.tag)
        {
            throw new UserInteractionException("Вы не являетесь участником этой гильдии");
        }

        user.guildTag = null;

        guild.members.Remove(member);

        await _dataBase.GuildDb.Update(guild);
        await _dataBase.UserDb.Update(user);

        await RespondAsync(embed: EmbedCreater.SuccessEmbed("Вы покинули гильдию"), ephemeral: true);
    }
}