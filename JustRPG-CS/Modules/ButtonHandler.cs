using Discord.WebSocket;
using JustRPG_CS.Classes;
using Serilog;

namespace JustRPG_CS;

public class ButtonHandler
{
    private DiscordSocketClient _client;
    private SocketMessageComponent _component;
    private DataBase _dataBase;


    public ButtonHandler(DiscordSocketClient client, SocketMessageComponent component, object? service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service!;
    }

    public async Task ButtonDistributor()
    {
        var buttonInfo = _component.Data.CustomId.Split('_');
        if (_component.User.Id.ToString() != buttonInfo[1])
        {
            await WrongInteraction();
            return;
        }
        switch (buttonInfo[0])
        {
            case "Profile":
                await ProfileButtonResponse(buttonInfo[2]);
                break;
            case "Equipment":
                await EquipmentButtonResponse(buttonInfo[2]);
                break;
            case "Inventory":
                break;
            case "UpSkills":
                await UpSkillsButtonResponse(buttonInfo[1]);
                break;
            case "UpSkill":
                await UpSkill(buttonInfo[1], buttonInfo[2]);
                break;
        }
        
    }

    private async Task WrongInteraction()
    {
        await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не можете с этим взаимодействовать"));
    }


    private async Task ProfileButtonResponse(string memberId)
    {
        var user = _client.GetUser(Convert.ToUInt64(memberId));
        var userDb = (User)_dataBase.GetFromDataBase(Bases.Users, "id",memberId)!;
        await _component.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.UserProfile(userDb, user);
            x.Components = ButtonSets.ProfileButtonsSet(_component.User.Id.ToString(), memberId, "Profile");
        });
    }

    private async Task EquipmentButtonResponse(string memberId)
    {
        var user = _client.GetUser(Convert.ToUInt64(memberId));
        var userDb = (User)_dataBase.GetFromDataBase(Bases.Users, "id",memberId)!;
        await _component.UpdateAsync(x =>
        {
            x.Embed = new EmbedCreater(_dataBase).UserEquipment(userDb, user);
            x.Components = ButtonSets.ProfileButtonsSet(_component.User.Id.ToString(), memberId, "Equipment");
        });
    }

    private async Task UpSkillsButtonResponse(string memberId)
    {
        var userDb = (User)_dataBase.GetFromDataBase(Bases.Users, "id",memberId)!;
        await _component.UpdateAsync(x =>
        {
            x.Embed = new EmbedCreater(_dataBase).UpSkills();
            x.Components = ButtonSets.UpUserSkills(memberId,userDb);
        });
    }

    private async Task UpSkill(string memberId, string skill)
    {
        
        var userDb = (User)_dataBase.GetFromDataBase(Bases.Users, "id",memberId)!;
        if (userDb.skill_points <= 0)
        {
            await _component.RespondAsync( embed: EmbedCreater.ErrorEmbed("У вас недостаточно скилл поинтов"), ephemeral: true);
            return;
        }

        _dataBase.Add(Bases.Users, "id", memberId, skill, 1);
        _dataBase.Add(Bases.Users, "id", memberId, "skill_points", -1);
        userDb = (User)_dataBase.GetFromDataBase(Bases.Users, "id", memberId)!;
        await _component.UpdateAsync(x =>
        {
            x.Embed = new EmbedCreater(_dataBase).UpSkills();
            x.Components = ButtonSets.UpUserSkills(memberId, userDb);
        });
        
    }
    
}