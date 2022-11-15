using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;

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
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                var ctx = new SocketInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
