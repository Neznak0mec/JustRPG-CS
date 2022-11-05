using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG_CS;
using Serilog;
using Serilog.Events;

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
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        
        _client.Log += LogAsync;
        sCommands.Log += (LogMessage msg) =>
        {
            Log.Information(msg.Message);
            return Task.CompletedTask;
        };

        _client.Ready += async () =>
        {
            Log.Information("{currentUserId} is logined!", _client.CurrentUser.Id);
            await sCommands.RegisterCommandsGloballyAsync();
            Log.Information("commands are loaded");
        };

        await _client.LoginAsync(Discord.TokenType.Bot, Environment.GetEnvironmentVariable("BotToken"));
        await _client.StartAsync();

        await Task.Delay(-1);
    }
    
    private static async Task LogAsync(LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };
        Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
        await Task.CompletedTask;
    }
}