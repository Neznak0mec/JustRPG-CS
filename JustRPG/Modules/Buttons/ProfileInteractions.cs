using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules.Buttons;

public class ProfileInteractions : IInteractionMaster
{
    private readonly DiscordSocketClient _client;
    private readonly SocketMessageComponent _component;
    private readonly DataBase _dataBase;

    public ProfileInteractions(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
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
        var userDb = (User)(await _dataBase.UserDb.Get(memberId))!;

        await ResponseMessage(EmbedCreater.UserProfile(userDb, user),
            ButtonSets.ProfileButtonsSet(_component.User.Id.ToString(), memberId));
    }

    private async Task EquipmentButtonResponse(string memberId)
    {
        var user = _client.GetUser(Convert.ToUInt64(memberId));
        var userDb = (User)(await _dataBase.UserDb.Get(memberId))!;

        await ResponseMessage(await EmbedCreater.UserEquipmentEmbed(userDb, user, _dataBase),
            ButtonSets.ProfileButtonsSet(_component.User.Id.ToString(), memberId, "Equipment"));
    }

    private async Task UpSkillsButtonResponse(string memberId)
    {
        var userDb = (User)(await _dataBase.UserDb.Get(memberId))!;
        await ResponseMessage(EmbedCreater.UpSkills(), ButtonSets.UpUserSkills(memberId, userDb));
    }

    public async Task InventoryButtonResponse(string memberId)
    {
        var member = _client.GetUser(Convert.ToUInt64(memberId));
        Inventory inventory =
            (Inventory)(await _dataBase.InventoryDb.Get($"Inventory_{memberId}_{_component.User.Id.ToString()}"))!;
        var items = await inventory.GetItems(_dataBase);

        await ResponseMessage(EmbedCreater.UserInventory(member, items),
            ButtonSets.InventoryButtonsSet(_component.User.Id.ToString(), Convert.ToInt64(memberId), inventory, items));
    }

    private async Task UpSkill(string memberId, string skill)
    {
        var userDb = (User)(await _dataBase.UserDb.Get(memberId))!;
        if (userDb!.skillPoints <= 0)
        {
            await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed("У вас недостаточно скилл поинтов"),
                ephemeral: true);
            return;
        }

        await _dataBase.UserDb.Add(userDb, skill, 1);
        await _dataBase.UserDb.Add(userDb, "skill_points", -1);
        userDb = (User)(await _dataBase.UserDb.Get(userDb.id))!;

        await ResponseMessage(EmbedCreater.UpSkills(), ButtonSets.UpUserSkills(memberId, userDb!));
    }

    private async Task ResponseMessage(Embed embed, MessageComponent component)
    {
        await _component.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = component;
        });
    }
}