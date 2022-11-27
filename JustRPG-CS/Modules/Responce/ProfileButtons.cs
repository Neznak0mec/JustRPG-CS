using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules.Responce;

public class ProfileButtons
{
    private DiscordSocketClient _client;
    private SocketMessageComponent _component;
    private DataBase _dataBase;

    public ProfileButtons(DiscordSocketClient client, SocketMessageComponent component, object? service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service!;
    }
    
    public async Task Distributor(string[] buttonInfo) 
    {
        switch (buttonInfo[0])
        {
            case "Profile":
                await ProfileButtonResponse(buttonInfo[2]);
                break;
            case "Equipment":
                await EquipmentButtonResponse(buttonInfo[2]);
                break;
            case "Inventory":
                await InventoryButtonResponse(buttonInfo[2]);
                break;
            case "UpSkills":
                await UpSkillsButtonResponse(buttonInfo[1]);
                break;
            case "UpSkill":
                await UpSkill(buttonInfo[1], buttonInfo[2]);
                break;
        }
    }
    
    private async Task ProfileButtonResponse(string memberId)
    {
        var user = _client.GetUser(Convert.ToUInt64(memberId));
        var userDb = (User) _dataBase.UserDb.Get(memberId)!;

        await ResponceMessage(EmbedCreater.UserProfile(userDb, user),
            ButtonSets.ProfileButtonsSet(_component.User.Id.ToString(), memberId));
    }

    private async Task EquipmentButtonResponse(string memberId)
    {
        var user = _client.GetUser(Convert.ToUInt64(memberId));
        var userDb = (User)_dataBase.UserDb.Get( memberId)!;

        await ResponceMessage(new EmbedCreater(_dataBase).UserEquipment(userDb, user),
            ButtonSets.ProfileButtonsSet(_component.User.Id.ToString(), memberId, "Equipment"));
    }

    private async Task UpSkillsButtonResponse(string memberId)
    {
        var userDb = (User)_dataBase.UserDb.Get(memberId)!;
        await ResponceMessage(EmbedCreater.UpSkills(), ButtonSets.UpUserSkills(memberId, userDb));
    }

    public async Task InventoryButtonResponse(string memberId)
    {
        var userDb = (User)_dataBase.UserDb.Get(memberId)!;
        var user = _client.GetUser(Convert.ToUInt64(memberId));

        Inventory inventory = (Inventory)_dataBase.InventoryDb.Get( $"Inventory_{userDb.id}_{_component.User.Id.ToString()}")!;

        var items = inventory.GetItems(_dataBase);
        
        await ResponceMessage(new EmbedCreater(_dataBase).UserInventory(user, items),
            ButtonSets.InventoryButtonsSet(_component.User.Id.ToString(), userDb, inventory, items));

    }

    private async Task UpSkill(string memberId, string skill)
    {
        
        var userDb = (User)_dataBase.UserDb.Get(memberId)!;
        if (userDb.skill_points <= 0)
        {
            await _component.RespondAsync( embed: EmbedCreater.ErrorEmbed("У вас недостаточно скилл поинтов"), ephemeral: true);
            return;
        }

        _dataBase.UserDb.Add(userDb,skill,1);
        _dataBase.UserDb.Add(userDb, "skill_points", -1);
        userDb = (User)_dataBase.UserDb.Get(userDb.id)!;

        await ResponceMessage(EmbedCreater.UpSkills(), ButtonSets.UpUserSkills(memberId, userDb));
    }

    private async Task ResponceMessage(Embed embed, MessageComponent component)
    {
        await _component.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = component;
        });
    }
}