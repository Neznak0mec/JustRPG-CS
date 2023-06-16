using Discord.Interactions;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DataBase _bases;

        public InteractionModule(DataBase service)
        {
            _bases = service;
        }

        [SlashCommand("ping", "Reciave a ping message")]
        public async Task Ping()
        {
            await RespondAsync($"{( DateTimeOffset.Now - Context.Interaction.CreatedAt).TotalMilliseconds} ms to server");
        }
    }

}
