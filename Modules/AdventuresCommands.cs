using Discord.Interactions;
using JustRPG.Features.Cooldown;
using JustRPG.Generators;
using JustRPG.Services;

namespace JustRPG.Modules;

public class AdventuresCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DataBase _bases;

    public AdventuresCommands(DataBase service)
    {
        _bases = service;
    }

    [Cooldown(300)]
    [SlashCommand(name: "adventure", description: "...")]
    public async Task Adventure()
    {
        var locations = _bases.LocationsDb.GetAdventuresLocations();
        await RespondAsync( embed: EmbedCreater.SelectAdventureEmbed(),
                            components: ButtonSets.SelectLocation(
                                        "adventure",
                                        Convert.ToInt64(Context.User.Id),
                                        locations
                                    ));
    }
}