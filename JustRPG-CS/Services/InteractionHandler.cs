using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Features.Cooldown;
using JustRPG.Generators;
using Serilog;

namespace JustRPG.Services
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;
        
        public InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.InteractionCreated += HandleInteraction;
            _client.ButtonExecuted += ButtonInteraction;
            _client.SelectMenuExecuted += SelectInteraction;
            _commands.InteractionExecuted += OnInteractionExecuted;
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            if (arg.Type == InteractionType.ApplicationCommand)
                try
                {
                    var context = new SocketInteractionContext(_client, arg);
                    await _commands.ExecuteCommandAsync(context, _services);
                }
                catch (Exception e)
                {
                    Log.Debug(e.ToString());
                }
        }

        private async Task OnInteractionExecuted(ICommandInfo command, IInteractionContext context, IResult result)
        {
            if(!result.IsSuccess && result.ErrorReason.StartsWith("Не так быстро") || context.User.Id == 426986442632462347)
            {
                await context.Interaction.RespondAsync(embed: EmbedCreater.ErrorEmbed(result.ErrorReason), ephemeral: true);
            }
            else if(!result.IsSuccess)
            {
                await context.Interaction.RespondAsync(embed: EmbedCreater.ErrorEmbed("Произошла неизвестная ошибка, попробуйте позже"), ephemeral: true);
            }
            else
            {
                return;
            }
        }

        private async Task ButtonInteraction(SocketMessageComponent component)
        {
            await new ButtonHandler(_client, component, _services.GetService(typeof(DataBase))!).ButtonDistributor();
        }

        private async Task SelectInteraction(SocketMessageComponent component)
        {
            await new SelectHandler(_client, component, _services.GetService(typeof(DataBase))!).SelectDistributor();
        }
    }
} 