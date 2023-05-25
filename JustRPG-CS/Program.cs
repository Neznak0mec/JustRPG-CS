using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG_CS.Features;
using JustRPG.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace JustRPG;

public class Program
{
    DataBase _shareDataBase;
    public static Task Main(string[] args) => new Program().MainAsync();

    private async Task MainAsync()
    {
        _shareDataBase = new DataBase();
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
                    .AddSingleton<DataBase>(_shareDataBase)
            ).Build();

        Task backgroundTask = RunBackgroundTaskAsync();

        await RunAsync(host);

        await backgroundTask;
    }

    private async Task RunBackgroundTaskAsync()
    {
        while (true)
        {
            Background.BackgroundMaster(_shareDataBase);
            await Task.Delay(30000);
        }
    }

    private async Task RunAsync(IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;

        var client = provider.GetRequiredService<DiscordSocketClient>();
        var sCommands = provider.GetRequiredService<InteractionService>();
        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        
        client.Log += LogAsync;
        sCommands.Log += (LogMessage msg) =>
        {
            Log.Information(msg.Message);
            return Task.CompletedTask;
        };

        client.Ready += async () =>
        {
            Log.Information("{currentUserId} is logined!", client.CurrentUser.Id);
            await sCommands.RegisterCommandsGloballyAsync();
            Log.Information("commands are loaded");
        };

        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("BotToken"));
        await client.StartAsync();

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