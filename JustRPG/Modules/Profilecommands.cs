using Discord.Interactions;
using JustRPG.Features.Cooldown;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules;

public class Profilecommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DataBase _bases;

    public Profilecommands(DataBase service)
    {
        _bases = service;
    }

    [SlashCommand("profile", "Просмотреть профиль")]
    public async Task Profile(
        [Summary(name: "user", description: "пользователь чей профиль хотите посмотерть")]
        Discord.IUser? needToFound = null
    )
    {
        needToFound ??= Context.User;

        User? user = await GetUser(Context.User.Id, Context.User.Id == needToFound.Id);

        if (user == null)
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Данный пользователь не найден"), ephemeral: true);
        else
            await RespondAsync(embed: EmbedCreater.UserProfile(user, needToFound),
                components: ButtonSets.ProfileButtonsSet(Context.User.Id.ToString(), user.id.ToString()));
    }

    [Cooldown(300)]
    [SlashCommand("work", "Помочь в городе")]
    public async Task Work()
    {
        User user = (await GetUser(Context.User.Id, true))!;

        int exp = 10 + Random.Shared.Next(0, 2 * user.stats.luck);
        int cash = 10 + Random.Shared.Next(0, 2 * user.stats.luck);

        await _bases.UserDb.Add(user, "exp", exp);
        await _bases.UserDb.Add(user, "cash", cash);

        await Context.Interaction.RespondAsync(embed: EmbedCreater.WorkEmbed(_bases.works!, exp, cash));
    }

    [SlashCommand("market", "Посмотреть торговую площадку")]
    public async Task Market()
    {
        SearchState searchState = new SearchState()
        {
            id = Guid.NewGuid().ToString(),
            userId = Context.User.Id
        };
        await _bases.MarketDb.CreateSearch(searchState);

        await _bases.MarketDb.SearchGetAndUpdate(searchState);

        await RespondAsync(embed: EmbedCreater.MarketPage(searchState),
            components: ButtonSets.MarketSortComponents(Context.User.Id, searchState.id));
    }

    private async Task<User?> GetUser(ulong userId, bool create = false)
    {
        var tempUser = await _bases.UserDb.Get(userId);
        if (tempUser == null && !create)
            return null;
        else
            return (User)tempUser!;
    }
}