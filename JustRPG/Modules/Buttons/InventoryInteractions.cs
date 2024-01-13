using System.ComponentModel;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Exceptions;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;
using MongoDB.Driver.Core.WireProtocol.Messages;
using Newtonsoft.Json.Serialization;
using Action = JustRPG.Models.Action;

namespace JustRPG.Modules.Buttons;

public class InventoryInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private readonly DiscordSocketClient _client;
    private readonly DataBase _dataBase;

    private Inventory _inventory;

    public InventoryInteractions(IServiceProvider service)
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

    [ComponentInteraction("Inventory|PrewPage_*_*", true)]
    private async Task PreviousPage(string finder, string userId)
    {
        _inventory!.DecrementPage();
        await UpdateMessage(finder, userId);
    }

    [ComponentInteraction("Inventory|NextPage_*_*", true)]
    private async Task NextPage(string finder, string userId)
    {
        _inventory!.IncrementPage();
        await UpdateMessage(finder, userId);
    }

    [ComponentInteraction("Inventory|Reload_*_*", true)]
    private async Task Reload(string finder, string userId)
    {
        var dbUser = (User)(await _dataBase.UserDb.Get(Convert.ToUInt64(userId)))!;

        await _inventory.Reload(dbUser!.inventory, _dataBase);
        await UpdateMessage(finder, userId);
    }

    [ComponentInteraction("Inventory|InteractionType_*_*", true)]
    private async Task ChangeInteractionType(string finder, string userId, string[] selected)
    {
        var interaction = string.Join("", selected);
        _inventory!.interactionType = interaction;
        await UpdateMessage(finder, userId);
    }

    private async Task UpdateMessage(string finder, string userId)
    {
        var dbUser = (User)(await _dataBase.UserDb.Get(Convert.ToUInt64(userId)))!;
        var member = _client.GetUser(Convert.ToUInt64(userId));
        var items = _inventory!.GetItems();
        
        var embed = await EmbedCreater.UserInventory(member!, dbUser!, items, _dataBase);

        await Context.Interaction.UpdateAsync(x =>
            {
                x.Embed = embed;
                x.Components = ButtonSets.InventoryButtonsSet(finder, dbUser!.id, _inventory, items);
            }
        );

        await _inventory.Save(_dataBase);
    }

    [ComponentInteraction("Inventory|info_*_*_*", true)]
    private async Task ItemInfo(string finder, string userId, string idString)
    {
        Item item = _inventory!.GetItems()[Convert.ToInt16(idString)]!;

        await RespondAsync(embed: EmbedCreater.ItemInfo(item), ephemeral: true);
    }

    [ComponentInteraction("Inventory|equip_*_*_*", true)]
    private async Task EquipItem(string finder, string userId, string idString)
    {
        var dbUser = (User)(await _dataBase.UserDb.Get(Convert.ToUInt64(userId)))!;

        Item? item = _inventory!.GetItems()[Convert.ToInt16(idString)];
        Item? itemToChange = null;
        Embed embed;
        Action? action;

        string uId = Guid.NewGuid().ToString();

        if (item != null)
        {
            Item tempItem = (Item)item;
            string? idItemToChange = dbUser!.equipment!.GetByType(tempItem.type);
            if (!tempItem.IsEquippable())
            {
                throw new UserInteractionException("Этот предмет нельзя экипировать");
            }


            if (tempItem.lvl > dbUser.lvl)
            {
                throw new UserInteractionException("Этот предмет cлишком высокого уровня для вас");
            }

            action = new Action
            {
                id = "Action_" + uId,
                date = DateTimeOffset.Now.ToUnixTimeSeconds(),
                userId = dbUser.id,
                type = "Equip",
                args = new[]
                {
                    item.id, "null"
                }
            };


            if (idItemToChange != null)
            {
                itemToChange = (Item)(await _dataBase.ItemDb.Get(idItemToChange))!;
                action.args[1] = itemToChange.id;
            }

            embed = EmbedCreater.WarningEmbed(idItemToChange != null
                ? $"Вы уверены что хотите снять `{itemToChange!.name}` и надеть `{tempItem.name}` ?"
                : $"Вы уверены что хотите надеть `{tempItem.name}` ?");
        }
        else
        {
            throw new UserInteractionException("Этот предмет не найден, странно 🤔");
        }

        await _dataBase.ActionDb.CreateObject(action);
        await RespondAsync(embed: embed, components: ButtonSets.AcceptActions(uId, dbUser!.id),
            ephemeral: true);
    }

    [ComponentInteraction("Inventory|sell_*_*_*", true)]
    private async Task SellItem(string finder, string userId, string idString)
    {
        long countOfSaleItems = await _dataBase.MarketDb.GetCountOfUserSlots(Context.User.Id);
        if (countOfSaleItems >= 5)
        {
            throw new UserInteractionException("Вы достигли лимита по продаже, одновременно можно выставлять только 5 предметов");
        }

        Item item = _inventory!.GetItems()[Convert.ToInt16(idString)]!;

        User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;

        if (user.inventory.All(x => x != item.id))
        {
            throw new UserInteractionException("Данный предмет не найден в вашем инвентаре, перезагрузите инвентарь");
        }

        SaleItem sellItem = new SaleItem()
        {
            id = Guid.NewGuid().ToString(),
            userId = Context.User.Id,
            itemId = item.id,
            price = -1,
            dateListed = DateTimeOffset.Now.ToUnixTimeSeconds(),
            itemDescription = item.ToStringWithRarity(),
            isVisible = false,

            itemName = $"{item.name} | {item.lvl} lvl",
            itemLvl = item.lvl,
            itemRarity = item.rarity,
            itemType = item.type
        };

        await _dataBase.MarketDb.CreateObject(sellItem);

        int indexToRemove = user.inventory.IndexOf(item.id);
        if (indexToRemove >= 0)
        {
            user.inventory = user.inventory.Where((_, index) => index != indexToRemove).ToList();
            await _dataBase.UserDb.Update(user);
        }

        await _inventory.Reload(user.inventory, _dataBase);
        _inventory.interactionType = "sell";
        await _inventory.Save();

        await UpdateMessage(finder, userId);
        
        await Context.Interaction.FollowupAsync(
            embed: EmbedCreater.WarningEmbed(
                "Предмет добавлен в лоты для продажи, но для начала продаж нужно установить цену"),
            components: ButtonSets.SaleItemButtonsSet(user.id, sellItem.id),
            ephemeral: true
        );
    }

    [ComponentInteraction("Inventory|destroy_*_*_*", true)]
    private async Task DestroyItem(string finder, string userId, string idString)
    {
        var dbUser = (User)(await _dataBase.UserDb.Get(Convert.ToUInt64(userId)))!;

        var itemId = _inventory!.GetItems()[Convert.ToInt16(idString)];
        object? item = null;
        Embed embed;
        Action? action;

        string uId = Guid.NewGuid().ToString();

        if (itemId == null)
        {
            await _inventory.Reload(dbUser!.inventory, _dataBase);
            await _inventory.Save(_dataBase);
        }
        else
            item = _inventory!.GetItems()[Convert.ToInt16(idString)]!;

        if (item != null)
        {
            Item tempItem = (Item)item;

            action = new Action
            {
                id = "Action_" + uId,
                date = DateTimeOffset.Now.ToUnixTimeSeconds(),
                type = "Destroy",
                userId = (long)Context.User.Id,
                args = new[]
                {
                    tempItem.id
                }
            };
            embed = EmbedCreater.WarningEmbed($"Вы уверены что хотите уничтожить `{tempItem.name}`?");
        }
        else
        {
            throw new UserInteractionException("Этот предмет не найден, странно 🤔");
        }

        await _dataBase.ActionDb.CreateObject(action);
        await RespondAsync(embed: embed,
            components: ButtonSets.AcceptActions(uId, (long)Context.User.Id), ephemeral: true);
    }

    [ComponentInteraction("Inventory|OpenSlotsSettings_*_*", true)]
    private async Task OpenSlotsSettings(string userId, string idString)
    {
        MarketSlotsSettings marketSettings = new MarketSlotsSettings
        {
            userId = Context.User.Id,
            id = Guid.NewGuid().ToString(),
            startPage = "inventory"
        };

        await _dataBase.MarketDb.GetUserSlots(marketSettings);
        await _dataBase.MarketDb.CreateSettings(marketSettings);

        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.MarketSettingsPage(marketSettings);
            x.Components = ButtonSets.MarketSettingComponents(marketSettings);
        });
    }
}
