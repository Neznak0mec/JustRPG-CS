using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Models.Enums;
using JustRPG.Services;

namespace JustRPG.Modules.Selects;

public class SelectLocation : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private readonly DiscordSocketClient _client;
    private readonly DataBase _dataBase;

    public SelectLocation(IServiceProvider service)
    {
         _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
         _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    [ComponentInteraction("SelectLocation|dungeon_*", true)]
    private async Task GenerateDungeon(string userId,string[] selected)
    {
        Location location = await _dataBase.LocationsDb.Get(Context.Interaction.Data.Values.ToArray()[0]);
        Warrior mainPlayer =
            await AdventureGenerators.GenerateWarriorByUser((User)(await _dataBase.UserDb.Get(userId))!,
            Context.User.Username, _dataBase);

        List<Warrior> enemies = new List<Warrior>();
        for (int i = 0; i < Random.Shared.Next(1, 3); i++)
        {
            enemies.Add(AdventureGenerators.GenerateMob(location));
        }

        Battle newBattle = new Battle
        {
            id = Guid.NewGuid().ToString(),
            type = BattleType.dungeon,
            drop = location.drops,
            players = new[] { mainPlayer },
            enemies = enemies.ToArray(),
            originalInteraction = new List<object> {Context},
            log = "-"
        };


        await _dataBase.BattlesDb.CreateObject(newBattle);

        await ResponseMessage(EmbedCreater.BattleEmbed(newBattle),
            component: ButtonSets.BattleButtonSet(newBattle, Convert.ToInt64(userId)));
    }
    
    [ComponentInteraction("SelectLocation|adventure_*", true)]
    async Task GenerateAdventure(string id, string[] selectedRoles)
    {
        Location location = await _dataBase.LocationsDb.Get(Context.Interaction.Data.Values.ToArray()[0]);
        Warrior mainPlayer =
            await AdventureGenerators.GenerateWarriorByUser((User)(await _dataBase.UserDb.Get(Context.User.Id))!,
            Context.User.Username, _dataBase);

        Battle newBattle = new Battle
        {
            id = Guid.NewGuid().ToString(),
            type = BattleType.adventure,
            drop = location.drops,
            players = new[] { mainPlayer },
            originalInteraction = new List<object> {Context},
            enemies = new[] { AdventureGenerators.GenerateMob(location) },
            log = "-"
        };

        await _dataBase.BattlesDb.CreateObject(newBattle);

        await ResponseMessage(EmbedCreater.BattleEmbed(newBattle),
            component: ButtonSets.BattleButtonSet(newBattle, (long)Context.User.Id));
        
        // await Context.Interaction.Message.ModifyAsync(x=> x.Content = "biba");
    }

    private async Task ResponseMessage(Embed embed, MessageComponent? component = null)
    {
        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = component;
        });
    }
}