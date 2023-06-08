using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Generators;
using JustRPG.Services;
using Serilog;

namespace JustRPG.Modules.Buttons;

public class SelectLocation : IInteractionMaster
{
    private DiscordSocketClient _client;
    private readonly SocketMessageComponent _component;
    private readonly DataBase _dataBase;

    public SelectLocation(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    public async Task Distributor(string[] buttonInfo)
    {
        switch (buttonInfo[2])
        {
            case "adventure":
                await GenerateAdventure(buttonInfo[1]);
                break;
            case "dungeon":
                await GenerateDungeon(buttonInfo[1]);
                break;
        }
    }

    private async Task GenerateDungeon(string userId)
    {
        Location location = _dataBase.LocationsDb.Get(_component.Data.Values.ToArray()[0]);
        Warrior mainPlayer = await AdventureGenerators.GenerateWarriorByUser((User) (await _dataBase.UserDb.Get(userId))!, _component.User.Username, _dataBase);

        List<Warrior> enemies = new List<Warrior>();
        for (int i = 0; i < Random.Shared.Next(1,3);i++)
        {
            enemies.Add(AdventureGenerators.GenerateMob(location, mainPlayer.stats));
        }

        Battle newBattle = new Battle
        {
            id = Guid.NewGuid().ToString(),
            type = "dungeon",
            drop = location.drops,
            players = new[] { mainPlayer },
            enemies = enemies.ToArray(),
            log = "-"
        };

        await _dataBase.BattlesDb.CreateObject(newBattle);

        await ResponseMessage(EmbedCreater.BattleEmbed(newBattle), component: ButtonSets.BattleButtonSet(newBattle,Convert.ToInt64(userId)));
    }

    async Task GenerateAdventure(string userId)
    {
        Location location = _dataBase.LocationsDb.Get(_component.Data.Values.ToArray()[0]);
        Warrior mainPlayer = await AdventureGenerators.GenerateWarriorByUser((User) (await _dataBase.UserDb.Get(userId))!, _component.User.Username, _dataBase);
        Battle newBattle = new Battle
        {
            id = Guid.NewGuid().ToString(),
            type = "adventure",
            drop = location.drops,
            players = new[] { mainPlayer },
            enemies = new[] { AdventureGenerators.GenerateMob(location, mainPlayer.stats) },
            log = "-"
        };

        await _dataBase.BattlesDb.CreateObject(newBattle);

        await ResponseMessage(EmbedCreater.BattleEmbed(newBattle), component: ButtonSets.BattleButtonSet(newBattle,Convert.ToInt64(userId)));
    }
    
    private async Task ResponseMessage(Embed embed, MessageComponent? component = null)
    {
        await _component.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = component;
        });
    }
}