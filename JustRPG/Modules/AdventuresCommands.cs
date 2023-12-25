using Discord.Interactions;
using JustRPG.Models;
using JustRPG.Generators;
using JustRPG.Services;

namespace JustRPG.Modules;

public class AdventuresCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DataBase _dataBase;

    public AdventuresCommands(IServiceProvider service)
    {
        _dataBase = (DataBase?)service.GetService(typeof(DataBase))!;
    }

//    [Cooldown(300)]
    [SlashCommand(name: "adventure", description: "...")]
    public async Task Adventure()
    {
        var locations =  await _dataBase.LocationsDb.GetAdventuresLocations();
        await RespondAsync(embed: EmbedCreater.SelectAdventureEmbed(),
            components: ButtonSets.SelectLocation(
                "adventure",
                Convert.ToInt64(Context.User.Id),
                locations
            ));
    }

//    [Cooldown(300)]
    [SlashCommand(name: "dungeon", description: "...")]
    public async Task Dungeon()
    {
        var locations = await _dataBase.LocationsDb.GetDungeons();
        await RespondAsync(embed: EmbedCreater.SelectAdventureEmbed(),
            components: ButtonSets.SelectLocation(
                "dungeon",
                Convert.ToInt64(Context.User.Id),
                locations
            ));
    }


    [SlashCommand(name: "arena", description: "...")]
    public async Task Arena()
    {
        User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;
        FindPVP findPvp = new FindPVP()
        {
            mmr = user.mmr,
            stratTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds(),
            userId = Convert.ToInt64(Context.User.Id)
        };

        long count = _dataBase.ArenaDb.CountOfFinfPVP();

        await RespondAsync(embed: EmbedCreater.FindPvp(findPvp, count),
            components: ButtonSets.CancelFindPvp(Context.User.Id));

        findPvp.msgLocation = Context.Interaction;
        _dataBase.ArenaDb.AppFindPVP(findPvp);
    }
}