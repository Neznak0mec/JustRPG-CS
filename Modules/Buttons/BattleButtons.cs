using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG_CS.Modules.Buttons;

public class BattleButtons {
    private DiscordSocketClient _client;
    private SocketMessageComponent _component;
    private DataBase _dataBase;

    public BattleButtons(DiscordSocketClient client, SocketMessageComponent component, object? service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service!;
    }
    
    public async Task Distributor(string[] buttonInfo)
    {
        object? temp = _dataBase.BattlesDb.Get(buttonInfo[3]);
        if (temp == null){
            await WrongInteraction("Боя не существует или он завершился");
            return;
        }
        Battle? battle = (Battle)temp;
        switch (buttonInfo[2])
        {
            case "Attack":
                break;
            case "Heal":
                break;
            case "Run":
                break;
        }
    }
    
    
    async Task Attack(Battle battle)
    {
        
    }
    
    async Task Heal(Battle battle)
    {

    }
    
    async Task Run(Battle battle)
    {

    }
    
    private async Task WrongInteraction(string text)
    {
        await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed(text), ephemeral:true);
    }
}