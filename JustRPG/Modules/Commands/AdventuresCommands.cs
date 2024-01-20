using Discord.Interactions;
using JustRPG.Exceptions;
using JustRPG.Features.Cooldown;
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

    [Cooldown(300)]
    [SlashCommand(name: "adventure", description: "Отправится в небольшой поход")]
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

    [Cooldown(300)]
    [SlashCommand(name: "dungeon", description: "Отправится в ближайшее подземелье")]
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


    [SlashCommand(name: "arena", description: "Сражаться на арене с другими игроками")]
    public async Task Arena()
    {
        FindPVP findPvp;
        long count;
        if (_dataBase.ArenaDb.IsFindPVP((long)Context.User.Id))
        {
            findPvp = _dataBase.ArenaDb.Get((long)Context.User.Id)!;
            
            count = _dataBase.ArenaDb.CountOfFinfPVP();

            await findPvp.msgLocation.ModifyOriginalResponseAsync(x =>
            {
                x.Embed = EmbedCreater.WarningEmbed("Вы начали новый поиск, этот поиск был отменен");
                x.Components = null;
            });
            
            findPvp.msgLocation = Context.Interaction;
            _dataBase.ArenaDb.Update(findPvp);
            
            await Context.Interaction.RespondAsync(embed: EmbedCreater.FindPvp(findPvp, count+1),
                components: ButtonSets.CancelFindPvp(Context.User.Id));
            return;
        }
        
        User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;
        findPvp = new FindPVP()
        {
            mmr = user.mmr,
            stratTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds(),
            msgLocation = Context.Interaction,
            userId = Convert.ToInt64(Context.User.Id)
        };

        count = _dataBase.ArenaDb.CountOfFinfPVP();
        
        _dataBase.ArenaDb.AppFindPVP(findPvp);

        await RespondAsync(embed: EmbedCreater.FindPvp(findPvp, count),
            components: ButtonSets.CancelFindPvp(Context.User.Id));

    }
}