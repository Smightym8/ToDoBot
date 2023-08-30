using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ToDoBot.Discord.Services;

namespace ToDoBot.Discord;

public class Bot
{
    private readonly string _token;
    private readonly DiscordSocketClient _client;
    
    public Bot(string token)
    {
        _token = token;
        _client = new DiscordSocketClient();
    }

    public async Task Start()
    {
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        
        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();

        // TODO: Make use of dependency injection
        var commandService = new CommandService();
        var commandHandler = new CommandHandler(_client, commandService);
        await commandHandler.InstallCommandsAsync();
        
        await Task.Delay(-1);
    }
    
    private Task ReadyAsync()
    {
        Console.WriteLine("Bot is ready!");

        return Task.CompletedTask;
    }
    
    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        
        return Task.CompletedTask;
    }
}