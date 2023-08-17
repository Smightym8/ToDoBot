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
    private Timer _dailyTimer;

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
        
        SetupDailyTimer();
        
        return Task.CompletedTask;
    }
    
    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        
        return Task.CompletedTask;
    }
    
    private void SetupDailyTimer()
    {
        // Set up the timer to execute the SendToDos method
        _dailyTimer = new Timer(
            async _ => await SendToDos(),
            null,
            TimeSpan.Zero, // Initial delay (start immediately)
            TimeSpan.FromHours(24) // Repeat every 24 hours
        );
    }
    
    private async Task SendToDos()
    {
        var today = DateTime.Now.DayOfWeek;

        if (today != DayOfWeek.Monday && today != DayOfWeek.Wednesday && today != DayOfWeek.Friday) return;

        if (_client.GetChannel(_channelId) is not IMessageChannel channel) return;
        
        var calendarData = await _calendarService.DownloadCalendarAsync(_calendarUrl);
        var toDoItems = _calendarService.ParseCalendarDataToToDoItem(calendarData);
        
        var startDate = DateTime.Today;
        var endDate = startDate.AddDays(7);
        
        // Filter the ToDo items based on the date range
        var toDosWithinNext7Days = toDoItems.Where(todo => 
            todo.SubmissionDate >= startDate && todo.SubmissionDate <= endDate
        ).ToList();
        
        var embed = CreateEmbed(toDosWithinNext7Days);
        
        await channel.SendMessageAsync("", false, embed);
    }
    
    private Embed CreateEmbed(List<ToDoItem> toDoItems)
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = "ToDos",
            Description = "The ToDos for the next 7 days",
            Color = Color.Blue
        };

        foreach (var toDoItem in toDoItems)
        {
            embedBuilder.AddField("Assignment", toDoItem.Title, true);
            embedBuilder.AddField("Submission Date", toDoItem.SubmissionDate.ToString("dd.MM.yyyy"), true);
            embedBuilder.AddField("Submission Time", toDoItem.SubmissionDate.ToString("HH:mm:ss"), true);
            embedBuilder.AddField("\u200b", "\u200b"); // Empty field to place other two fields next to each other
        }
        
        return embedBuilder.WithCurrentTimestamp().Build();
    }
}