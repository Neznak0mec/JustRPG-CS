using System.ComponentModel;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG_CS.Classes;
using IResult = Discord.Commands.IResult;

namespace JustRPG_CS.modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DataBase _bases;

        public InteractionModule(DataBase service)
        {
            _bases = service;
        }

        [SlashCommand("ping", "Receaving a ping message")]
        public async Task Ping(
            [Discord.Interactions.Summary(name: "number", description: "да")]
            int number = 1
        )
        {
            var ctx = Context;
            var buttin = new ButtonBuilder(label: "кнопочка", customId: $"{ctx.User.Id}-{ctx.Interaction.Id}");
            var components = new ComponentBuilder();
            components.WithButton(buttin);

            Console.WriteLine("получаю");
            var a = (User)_bases.GetFromDataBase(Bases.Users, "id",ctx.User.Id)!;
            Console.WriteLine("получил");

            await RespondAsync($"{ctx.User.Id} баба {a.cash}", components: components.Build());
        }
    }

}
