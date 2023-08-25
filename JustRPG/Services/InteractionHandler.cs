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
        private readonly DataBase _dataBase;

        public InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _dataBase = (DataBase)_services.GetService(typeof(DataBase))!;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.InteractionCreated += HandleInteraction;
            _client.ButtonExecuted += ButtonInteraction;
            _client.SelectMenuExecuted += SelectInteraction;
            _client.ModalSubmitted += ModalInteraction;
            
            _commands.InteractionExecuted += OnInteractionExecuted;
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            await _dataBase.UserDb.Cache(arg.User.Id);
            if (arg.Type == InteractionType.ApplicationCommand)
            {
                var context = new SocketInteractionContext(_client, arg);
                _ = Task.Run(() => { _commands.ExecuteCommandAsync(context, _services); });
            }
        }

        private async Task OnInteractionExecuted(ICommandInfo info, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess && (result.ErrorReason.StartsWith("Не так быстро") ||
                                      context.User.Id == 426986442632462347))
            {
                await context.Interaction.RespondAsync(embed: EmbedCreater.ErrorEmbed(result.ErrorReason),
                    ephemeral: true);
                Log.Debug("{reason}",result.ErrorReason);
            }
            else if (!result.IsSuccess)
            {
                await context.Interaction.RespondAsync(
                    embed: EmbedCreater.ErrorEmbed("Произошла неизвестная ошибка, попробуйте позже"), ephemeral: true);
                Log.Debug("{reason}",result.ErrorReason);
            }
        }

        private async Task ButtonInteraction(SocketMessageComponent component)
        {
            var context = new SocketInteractionContext<SocketMessageComponent>(_client, component);
            if (context.Interaction.Data.CustomId.Split('_')[1]==component.User.Id.ToString())
                    _ = Task.Run(async () => {await Execute(context); });
            else
                    await component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не можете с этим взаимодействовать"),
                                                   ephemeral: true);

        }

        private async Task SelectInteraction(SocketMessageComponent component)
        {
            var context = new SocketInteractionContext<SocketMessageComponent>(_client, component);
                if (context.Interaction.Data.CustomId.Split('_')[1]==component.User.Id.ToString())
                    _ = Task.Run(async () => {await Execute(context); });
                else
                    await component.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не можете с этим взаимодействовать"),
                    ephemeral: true);
        }

        private async Task Execute(IInteractionContext ctx)
        {
            try
            {
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception e)
            {
                Log.Debug(e.ToString());
            }
        }

        private Task ModalInteraction(SocketModal component)
        {
            _ = Task.Run(async () =>
            {
                await new ModalHandler(_client, component, _services).ModalDistributor();
            });
            return Task.CompletedTask;
        }
    }
}