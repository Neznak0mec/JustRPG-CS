using System.ComponentModel;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
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
         _inventory.DataBase = _dataBase;

        await _inventory!.PreviousPage();
        await UpdateMessage(finder, userId);
    }

    [ComponentInteraction("Inventory|NextPage_*_*", true)]
    private async Task NextPage(string finder, string userId)
    {
        _inventory.DataBase = _dataBase;

        await _inventory!.NextPage();
        await UpdateMessage(finder, userId);
    }

    [ComponentInteraction("Inventory|Reload_*_*", true)]
    private async Task Reload(string finder, string userId)
    {
        _inventory.DataBase = _dataBase;

        var dbUser = (User)(await _dataBase.UserDb.Get(Convert.ToUInt64(userId)))!;

        await _inventory!.Reload(dbUser!.inventory);
        await UpdateMessage(finder, userId);
    }

    [ComponentInteraction("Inventory|InteractionType_*_*", true)]
    private async Task ChangeInteractionType(string finder, string userId,string[] selected)
    {
        _inventory.DataBase = _dataBase;

        var interaction = string.Join("", selected);
        _inventory!.interactionType = interaction;
        await UpdateMessage(finder, userId);
    }

    private async Task UpdateMessage(string finder, string userId)
    {
        _inventory.DataBase = _dataBase;

        var dbUser = (User)(await _dataBase.UserDb.Get(Convert.ToUInt64(userId)))!;
        var member = _client.GetUser(Convert.ToUInt64(userId));
        var items = await _inventory!.GetItems(_dataBase);

        await Context.Interaction.UpdateAsync(
            x =>
            {
                x.Embed = EmbedCreater.UserInventory(member!, dbUser!,items);
                x.Components = ButtonSets.InventoryButtonsSet(finder, dbUser!.id, _inventory, items);
            }
        );

    }

    [ComponentInteraction("Inventory_*_*_info_*", true)]
    private async Task ItemInfo(string finder, string userId, string idString)
    {
        _inventory.DataBase = _dataBase;

        string itemId = _inventory!.currentPageItems[Convert.ToInt16(idString)]!;

        var item = await _dataBase.ItemDb.Get(itemId);
        Embed embed = item == null
            ? EmbedCreater.ErrorEmbed("Этот предмет не найден, странно 🤔")
            : EmbedCreater.ItemInfo((Item)item);

        await RespondAsync(embed: embed, ephemeral: true);
    }

    [ComponentInteraction("Inventory_*_*_equip_*", true)]
    private async Task EquipItem(string finder, string userId, string idString)
    {
        _inventory.DataBase = _dataBase;

        var dbUser = (User)(await _dataBase.UserDb.Get(Convert.ToUInt64(userId)))!;

        var itemId = _inventory!.currentPageItems[Convert.ToInt16(idString)];
        Item? itemToChange = null;
        Embed embed;
        Action? action;

        string uId = Guid.NewGuid().ToString();

        if (itemId == null)
        {
            embed = EmbedCreater.ErrorEmbed("Произошла ошибка, обновите инвентарь");
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        object? item = await _dataBase.ItemDb.Get(itemId);


        if (item != null)
        {
            Item tempItem = (Item)item;
            string? idItemToChange = dbUser!.equipment!.GetByType(tempItem.type);
            if (!tempItem.IsEquippable())
            {
                await RespondAsync(embed: EmbedCreater.ErrorEmbed("Этот предмет нельзя экипировать"),
                    ephemeral: true);
                return;
            }


            if (tempItem.lvl > dbUser.lvl)
            {
                await RespondAsync(embed: EmbedCreater.ErrorEmbed("Этот предмет cлишком высокого уровня для вас"),
                    ephemeral: true);
                return;
            }

            action = new Action
            {
                id = "Action_" + uId,
                date = DateTimeOffset.Now.ToUnixTimeSeconds(),
                userId = dbUser.id,
                type = "Equip",
                args = new[]
                {
                    itemId, "null"
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
            embed = EmbedCreater.ErrorEmbed("Этот предмет не найден, странно 🤔");
            action = null;
        }

        if (action != null)
        {
            await _dataBase.ActionDb.CreateObject(action);
            await RespondAsync(embed: embed, components: ButtonSets.AcceptActions(uId, dbUser!.id),
                ephemeral: true);
        }
        else
        {
            await RespondAsync(embed: embed, ephemeral: true);
        }
    }

    [ComponentInteraction("Inventory_*_*_sell_*", true)]
    private async Task SellItem(string finder, string userId, string idString)
    {
        _inventory.DataBase = _dataBase;

        string itemId = _inventory!.currentPageItems[Convert.ToInt16(idString)]!;

        long countOfSaleItems = await _dataBase.MarketDb.GetCountOfUserSlots(Context.User.Id);
        if (countOfSaleItems >= 5)
        {
            await RespondAsync(
                embed: EmbedCreater.ErrorEmbed(
                    "Вы достигли лимита по продаже, одновременно можно выставлять только 5 предметов"),
                ephemeral: true);
            return;
        }

        Item item;

        User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;

        if (user.inventory.All(x => x != itemId))
        {
            await RespondAsync(
                embed: EmbedCreater.ErrorEmbed("Данный предмет не найден в вашем инвентаре, перезагрузите инвентарь"),
                ephemeral: true);
            return;
        }

        item = (Item)(await _dataBase.ItemDb.Get(itemId))!;

        SaleItem sellItem = new SaleItem()
        {
            id = Guid.NewGuid().ToString(),
            userId = Context.User.Id,
            itemId = itemId,
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

        int indexToRemove = user.inventory.IndexOf(itemId);
        if (indexToRemove >= 0)
        {
            user.inventory = user.inventory.Where((_, index) => index != indexToRemove).ToList();
            await _dataBase.UserDb.Update(user);
        }

        await RespondAsync(
            embed: EmbedCreater.WarningEmbed(
                "Предмет добавлен в лоты для продажи, но для начала продаж нужно установить цену"),
            components: new ComponentBuilder()
                .WithButton(label: "Установить цену", $"Market_{user.id}_editPrice_{sellItem.id}").Build(),
            ephemeral: true
        );
    }

    [ComponentInteraction("Inventory_*_*_destroy_*", true)]
    private async Task DestroyItem(string finder, string userId, string idString)
    {
        _inventory.DataBase = _dataBase;

        var dbUser = (User)(await _dataBase.UserDb.Get(Convert.ToUInt64(userId)))!;

        var itemId = _inventory!.currentPageItems[Convert.ToInt16(idString)];
        object? item = null;
        Embed embed;
        Action? action;

        string uId = Guid.NewGuid().ToString();

        if (itemId == null)
        {
            await _inventory.Reload(dbUser!.inventory);
        }
        else
            item = await _dataBase.ItemDb.Get(itemId);

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
            embed = EmbedCreater.ErrorEmbed("Этот предмет не найден, странно 🤔");
            action = null;
        }

        if (action != null)
        {
            await _dataBase.ActionDb.CreateObject(action);
            await RespondAsync(embed: embed,
                components: ButtonSets.AcceptActions(uId, (long)Context.User.Id), ephemeral: true);
        }
        else
        {
            await RespondAsync(embed: embed, ephemeral: true);
        }
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

        await ModifyOriginalResponseAsync(x =>
        {
            x.Embed = EmbedCreater.MarketSettingsPage(marketSettings);
            x.Components = ButtonSets.MarketSettingComponents(marketSettings);
        });
    }
}