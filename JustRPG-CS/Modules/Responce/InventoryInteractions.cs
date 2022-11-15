using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Services;
using Serilog;

namespace JustRPG.Modules.Responce;

public class InventoryInteractions
{
    private DiscordSocketClient _client;
    private SocketMessageComponent _component;
    private DataBase _dataBase;
    private Inventory _inventory;
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
        _inventory = (Inventory)_dataBase.GetFromDataBase(Bases.Interactions, "id", $"Inventory_{buttonInfo[1]}_{buttonInfo[2]}")!;
        _dbUser = (User)_dataBase.GetFromDataBase(Bases.Users, "id", Convert.ToUInt64(buttonInfo[2]))!;
        _member = _client.GetUser(Convert.ToUInt64(buttonInfo[2]));
        Log.Debug(buttonInfo[0]);
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
            case "UpSkill":
                // await UpSkill(buttonInfo[1], buttonInfo[2]);
                break;
        }
    }

    private async Task PreviousPage(string finder)
    {
        _inventory.PrewPage();
        await UpdateMessage(finder);
    }
    
    private async Task NextPage(string finder)
    {
        _inventory.NextPage();
        await UpdateMessage(finder);
    }

    private async Task Reload(string finder)
    {
        _inventory.Reload(_dbUser.inventory);
        await UpdateMessage(finder);
    }

    private async Task ChangeinteractionType(string finder)
    {
        var interaction = string.Join("", _component.Data.Values);  
        _inventory.interactionType = interaction;
        Log.Fatal(interaction);
        await UpdateMessage(finder);
    }

    private async Task UpdateMessage(string finder)
    {
        var items = _inventory.GetItems(_dataBase);
        Log.Fatal(_inventory.interactionType);
        await _component.UpdateAsync(
            x =>
            {
                x.Embed = new EmbedCreater().UserInventory(_member, items);
                x.Components = ButtonSets.InventoryButtonsSet(finder, _dbUser, _inventory, items);
            }
        );
 
    }
}