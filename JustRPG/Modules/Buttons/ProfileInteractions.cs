using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules.Buttons;

public class ProfileInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private readonly DiscordSocketClient _client;
    private readonly DataBase _dataBase;

    public ProfileInteractions(IServiceProvider service)
    {
         _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
         _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    [ComponentInteraction("Profile_*_*", true)]
    private async Task ProfileButtonResponse(string memberId, string toFind)
    {
        var user = _client.GetUser(Convert.ToUInt64(toFind));
        var userDb = (User)(await _dataBase.UserDb.Get(toFind))!;

        await ResponseMessage(await EmbedCreater.UserProfile(userDb, user,_dataBase),
            ButtonSets.ProfileButtonsSet(memberId, toFind));
    }

    [ComponentInteraction("Equipment_*_*", true)]
    private async Task EquipmentButtonResponse(string memberId, string toFind)
    {
        var user = _client.GetUser(Convert.ToUInt64(toFind));
        var userDb = (User)(await _dataBase.UserDb.Get(toFind))!;

        await ResponseMessage(await EmbedCreater.UserEquipmentEmbed(userDb, user, _dataBase),
            ButtonSets.ProfileButtonsSet(memberId, toFind, "Equipment"));
    }

    [ComponentInteraction("Inventory_*_*", true)]
    public async Task InventoryButtonResponse(string memberId, string toFind)
    {
        var member = _client.GetUser(Convert.ToUInt64(toFind));
        Inventory inventory =
            (Inventory)(await _dataBase.InventoryDb.Get($"Inventory_{memberId}_{toFind}"))!;
        var items = inventory.GetItems();
        User user = (User)(await _dataBase.UserDb.Get(toFind))!;

        await ResponseMessage(await EmbedCreater.UserInventory(member, user,items, _dataBase),
            ButtonSets.InventoryButtonsSet(memberId, Convert.ToInt64(toFind), inventory, items)
            );
    }

    private async Task ResponseMessage(Embed embed, MessageComponent component)
    {
        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = component;
        });
    }
}