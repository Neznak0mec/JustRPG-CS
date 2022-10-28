using Discord.Interactions;
using Discord.WebSocket;
using JustRPG_CS.Classes;
using Serilog;

namespace JustRPG_CS;

public class ButtonHandler
{
    private DiscordSocketClient _client;
    private SocketMessageComponent _component;
    private DataBase _dataBase;


    public ButtonHandler(DiscordSocketClient client, SocketMessageComponent component, object? service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service!;
    }
    
}