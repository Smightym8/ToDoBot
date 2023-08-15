using Discord;
using Discord.WebSocket;

namespace ToDoBot;

public class Bot
{
    private readonly string _token;
    private readonly ulong _channelId;
    private readonly string _calendarUrl;
    private readonly DiscordSocketClient _client;
    private readonly ICalendarService _calendarService;

    public Bot(string token, ulong channelId, string calendarUrl, ICalendarService calendarService)
    {
        _token = token;
        _channelId = channelId;
        _calendarUrl = calendarUrl;
        _calendarService = calendarService;
        _client = new DiscordSocketClient();
    }

    public async Task Start()
    {
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;

        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
        
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