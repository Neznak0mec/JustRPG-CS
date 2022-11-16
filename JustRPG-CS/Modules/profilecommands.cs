using Discord.Interactions;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Services;
using Serilog;

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
            [Discord.Interactions.Summary(name: "user", description:"пользователь чей профиль хотете посмотерть")]
            Discord.IUser? needToFound = null)
        {
            needToFound ??= Context.User;
            User? user = (User)_bases.UserDb.Get("id", Context.User.Id);

            if (user == null && Context.User.Id == needToFound.Id)
                user = (User)_bases.UserDb.CreateObject(Context.User.Id);

            if (user == null)
                await RespondAsync(embed: EmbedCreater.ErrorEmbed("Данный пользователь не найден"), ephemeral: true);
            else
                await RespondAsync(embed: EmbedCreater.UserProfile(user, needToFound), components:ButtonSets.ProfileButtonsSet(Context.User.Id.ToString(),user.id.ToString()));
        }
}