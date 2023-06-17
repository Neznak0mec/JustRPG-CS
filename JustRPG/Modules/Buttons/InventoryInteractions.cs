using System.ComponentModel;
using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;
using MongoDB.Driver.Core.WireProtocol.Messages;
using Newtonsoft.Json.Serialization;
using Action = JustRPG.Models.Action;

namespace JustRPG.Modules.Buttons;

public class InventoryInteractions : IInteractionMaster
{
    private readonly DiscordSocketClient _client;
    private readonly SocketMessageComponent _component;
    private readonly DataBase _dataBase;
    private Inventory? _inventory;
    private User? _dbUser;
    private SocketUser? _member;

    public InventoryInteractions(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }


    public async Task Distributor(string[] buttonInfo)
    {
        _inventory = (Inventory)(await _dataBase.InventoryDb.Get($"Inventory_{buttonInfo[1]}_{buttonInfo[2]}"))!;
        _inventory.DataBase = _dataBase;

        _dbUser = (User)(await _dataBase.UserDb.Get(Convert.ToUInt64(buttonInfo[2])))!;
        _member = _client.GetUser(Convert.ToUInt64(buttonInfo[2]));


        switch (buttonInfo[3])
        {
            case "PrewPage":
                await PreviousPage(buttonInfo[1]);
                return;
            case "NextPage":
                await NextPage(buttonInfo[1]);
                return;
            case "Reload":
                await Reload(buttonInfo[1]);
                return;
            case "InteractionType":
                await ChangeInteractionType(buttonInfo[1]);
                return;
            case "info":
                await ItemInfo(buttonInfo[4]);
                return;
            case "equip":
                await EquipItem(buttonInfo[4]);
                return;
            case "sell":
                await SellItem(buttonInfo[4]);
                return;
            case "destroy":
                await DestroyItem(buttonInfo[4]);
                return;
            case "OpenSlotsSettings":
                await OpenSlotsSettings(buttonInfo);
                return;
        }
    }

    private async Task PreviousPage(string finder)
    {
        await _inventory!.PreviousPage();
        await UpdateMessage(finder);
    }

    private async Task NextPage(string finder)
    {
        await _inventory!.NextPage();
        await UpdateMessage(finder);
    }

    private async Task Reload(string finder)
    {
        await _inventory!.Reload(_dbUser!.inventory);
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
            await _inventory.Reload(_dbUser!.inventory);
        }
        else
        {
            var item = await _dataBase.ItemDb.Get(itemId);
            embed = item == null
                ? EmbedCreater.ErrorEmbed("Этот предмет не найден, странно 🤔")
                : EmbedCreater.ItemInfo((Item)item);
        }

        await _component.RespondAsync(embed: embed, ephemeral: true);
    }

    private async Task UpdateMessage(string finder)
    {
        var items = await _inventory!.GetItems(_dataBase);
        await _component.UpdateAsync(
            x =>
            {
                x.Embed = EmbedCreater.UserInventory(_member!, items);
                x.Components = ButtonSets.InventoryButtonsSet(finder, _dbUser!.id, _inventory, items);
            }
        );
    }

    private async Task EquipItem(string buttonInfo)
    {
        var itemId = _inventory!.currentPageItems[Convert.ToInt16(buttonInfo)];
        Item? itemToChange = null;
        Embed embed;
        Action? action;

        string uId = Guid.NewGuid().ToString();

        if (itemId == null)
        {
            embed = EmbedCreater.ErrorEmbed("Произошла ошибка, обновите инвентарь");
            await _component.RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        object? item = await _dataBase.ItemDb.Get(itemId);


        if (item != null)
        {
            Item tempItem = (Item)item;
            string? idItemToChange = _dbUser!.equipment!.GetByName(tempItem.type);
            if (!tempItem.IsEquippable())
            {
                await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Этот предмет нельзя экипировать"),
                    ephemeral: true);
                return;
            }

            action = new Action
            {
                id = "Action_" + uId,
                date = DateTimeOffset.Now.ToUnixTimeSeconds(),
                userId = _dbUser.id,
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
            await _component.RespondAsync(embed: embed, components: ButtonSets.AcceptActions(uId, _dbUser!.id),
                ephemeral: true);
        }
        else
        {
            await _component.RespondAsync(embed: embed, ephemeral: true);
        }
    }

    private async Task SellItem(string buttonInfo)
    {
        string itemId = _inventory!.currentPageItems[Convert.ToInt16(buttonInfo)]!;

        long countOfSaleItems = await _dataBase.MarketDb.GetCountOfUserSlots(_component.User.Id);
        if (countOfSaleItems >= 5)
        {
            await _component.RespondAsync(
                embed: EmbedCreater.ErrorEmbed(
                    "Вы достигли лимита по продаже, одновременно можно выставлять только 5 предметов"),
                ephemeral: true);
            return;
        }

        Item item;

        User user = (User)(await _dataBase.UserDb.Get(_component.User.Id))!;

        if (user.inventory.All(x => x != itemId))
        {
            await _component.RespondAsync(
                embed: EmbedCreater.ErrorEmbed("Данный предмет не найден в вашем инвентаре, перезагрузите инвентарь"),
                ephemeral: true);
            return;
        }

        item = (Item)(await _dataBase.ItemDb.Get(itemId))!;

        SaleItem sellItem = new SaleItem()
        {
            id = Guid.NewGuid().ToString(),
            userId = _component.User.Id,
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

        await _component.RespondAsync(
            embed: EmbedCreater.WarningEmbed(
                "Предмет добавлен в лоты для продажи, но для начала продаж нужно установить цену"),
            components: new ComponentBuilder()
                .WithButton(label: "Установить цену", $"Market_{user.id}_editPrice_{sellItem.id}").Build(),
            ephemeral: true
        );
    }

    private async Task DestroyItem(string buttonInfo)
    {
        var itemId = _inventory!.currentPageItems[Convert.ToInt16(buttonInfo)];
        object? item = null;
        Embed embed;
        Action? action;

        string uId = Guid.NewGuid().ToString();

        if (itemId == null)
        {
            await _inventory.Reload(_dbUser!.inventory);
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
                userId = (long)_component.User.Id,
                args = new[]
                {
                    tempItem.id
                }
            };
            embed = EmbedCreater.WarningEmbed($"Вы уверены что хотите уничтожыть `{tempItem.name}`?");
        }
        else
        {
            embed = EmbedCreater.ErrorEmbed("Этот предмет не найден, странно 🤔");
            action = null;
        }

        if (action != null)
        {
            await _dataBase.ActionDb.CreateObject(action);
            await _component.RespondAsync(embed: embed,
                components: ButtonSets.AcceptActions(uId, (long)_component.User.Id), ephemeral: true);
        }
        else
        {
            await _component.RespondAsync(embed: embed, ephemeral: true);
        }
    }


    private async Task OpenSlotsSettings(string[] buttonInfo)
    {
        MarketSettings marketSettings = new MarketSettings
        {
            userId = _component.User.Id,
            id = Guid.NewGuid().ToString(),
            startPage = "inventory"
        };

        await _dataBase.MarketDb.GetUserSlots(marketSettings);
        await _dataBase.MarketDb.CreateSettings(marketSettings);

        await _component.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.MarketSettingsPage(marketSettings);
            x.Components = ButtonSets.MarketSettingComponents(marketSettings);
        });
    }
}