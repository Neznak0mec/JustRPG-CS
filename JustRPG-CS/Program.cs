using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG_CS;

using MongoDB.Bson;
using MongoDB.Driver;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        using IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
                services
                    .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig()
                    {
                        GatewayIntents = Discord.GatewayIntents.None,
                        AlwaysDownloadUsers = true
                    }))
                    .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                    .AddSingleton<InteractionHandler>()
                    .AddSingleton<DataBase>()
                ).Build();


        await RunAsync(host);
    }

    public async Task RunAsync(IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;

        var _client = provider.GetRequiredService<DiscordSocketClient>();
        var sCommands = provider.GetRequiredService<InteractionService>();
        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

        _client.Log += async (LogMessage msg) => { Console.WriteLine(msg.Message); };
        sCommands.Log += async (LogMessage msg) => { Console.WriteLine(msg.Message); };

        _client.Ready += async () =>
        {
            Console.WriteLine(_client.CurrentUser.Id + " is logined!");
            await sCommands.RegisterCommandsGloballyAsync();
            Console.WriteLine("commands are loaded");
        };

        await _client.LoginAsync(Discord.TokenType.Bot, Environment.GetEnvironmentVariable("BotToken"));
        await _client.StartAsync();

        await Task.Delay(-1);
    }
}