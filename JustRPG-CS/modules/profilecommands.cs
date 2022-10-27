using Discord;
using Discord.Interactions;
using JustRPG_CS.Classes;

namespace JustRPG_CS.modules;

public class Profilecommands : InteractionModuleBase<SocketInteractionContext>
{
        private readonly DataBase _bases;

        public Profilecommands(DataBase service)
        {
            _bases = service;
        }

        [SlashCommand("profile", "da")]
        public async Task Profile(
            [Discord.Interactions.Summary(name: "user", description:"пользователь чей профиль хотете посмотерть")]
            Discord.IUser? needToFound = null)
        {
            if (needToFound == null)
                needToFound = Context.User;
            
            User? user = (User)_bases.GetFromDataBase(Bases.Users, needToFound.Id);
            
            if (user == null)
                await RespondAsync(embed: EmbedCreater.ErrorEmbed("Данный пользователь не найден"), ephemeral: true);
            else
                await RespondAsync(embed: EmbedCreater.UserProfile(user, needToFound));
        }
}