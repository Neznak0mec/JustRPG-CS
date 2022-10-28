using Discord.WebSocket;
using JustRPG_CS.Classes;

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
                break;
        }
        
    }

    private async Task WrongInteraction()
    {
        await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не можете с этим взаимодействовать"));
    }


    private async Task ProfileButtonResponse( string memberID)
    {
        var user = _client.GetUser(Convert.ToUInt64(memberID));
        var userDb = (User)_dataBase.GetFromDataBase(Bases.Users, memberID)!;
        await _component.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.UserProfile(userDb, user);
            x.Components = ButtonSets.ProfileButtonsSet(_component.User.Id.ToString(), memberID, "Profile");
        });
    }

    private async Task EquipmentButtonResponse(string memberID)
    {
        var user = _client.GetUser(Convert.ToUInt64(memberID));
        var userDb = (User)_dataBase.GetFromDataBase(Bases.Users, memberID)!;
        await _component.UpdateAsync(x =>
        {
            x.Embed = new EmbedCreater(_dataBase).UserEqipment(userDb, user);
            x.Components = ButtonSets.ProfileButtonsSet(_component.User.Id.ToString(), memberID, "Equipment");
        });
    }
    
}