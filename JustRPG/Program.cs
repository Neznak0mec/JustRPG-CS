﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Features;
using JustRPG.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using DotNetEnv;

namespace JustRPG;

public class Program
{
    DataBase? _shareDataBase;
    DiscordSocketClient? _client;
    private Background? _background;
    public static Task Main(string[] args) => new Program().MainAsync();

    private async Task MainAsync()
    {
        Env.Load(".env");
        _shareDataBase = new DataBase();
        _client = new DiscordSocketClient(new DiscordSocketConfig()
        {
            GatewayIntents = Discord.GatewayIntents.None,
            AlwaysDownloadUsers = false,
        });
        _background = new Background(_shareDataBase, _client);

        using IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
                services
                    .AddSingleton<DiscordSocketClient>(_client)
                    .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                    .AddSingleton<InteractionHandler>()
                    .AddSingleton<DataBase>(_shareDataBase)
                    .AddSingleton<Background>(_background)
            ).Build();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        _client.Log += LogAsync;

        await Task.WhenAll(RunAsync(host), RunBackgroundTaskAsync());
    }

    private async Task RunBackgroundTaskAsync()
    {
        await _background!.BackgroundMaster();
    }

    private async Task RunAsync(IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;

        var sCommands = provider.GetRequiredService<InteractionService>();
        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

        sCommands.Log += (LogMessage msg) => Task.CompletedTask;

        _client!.Ready += async () =>
        {
            Log.Information("{CurrentUserId} is login!", _client.CurrentUser.Id);
            await sCommands.RegisterCommandsGloballyAsync();
            Log.Information("commands are loaded");
        };

        #if DEBUG
            await _client.LoginAsync(TokenType.Bot, Env.GetString("BotToken-OpenTest"));
        #else
            await _client.LoginAsync(TokenType.Bot, Env.GetString("BotToken"));
        #endif
        
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