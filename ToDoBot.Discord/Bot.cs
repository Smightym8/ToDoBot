using System.Net;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ToDoBot.Common;
using ToDoBot.Discord.Services;

namespace ToDoBot.Discord;

public class Bot
{
    private readonly string _token;
    private readonly ulong _channelId;
    private readonly string _calendarUrl;
    private readonly DiscordSocketClient _client;
    private readonly ICalendarService _calendarService;
    private Timer _dailyTimer = default!;
    
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

        // TODO: Make use of dependency injection
        var commandService = new CommandService();
        var commandHandler = new CommandHandler(_client, commandService);
        await commandHandler.InstallCommandsAsync();
        
        await Task.Delay(-1);
    }
    
    private async Task ReadyAsync()
    {
        Console.WriteLine("Bot is ready!");
        
        //SetupDailyTimer();
    }
    
    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        
        return Task.CompletedTask;
    }
    
    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        await command.RespondAsync($"You executed {command.Data.Name}");
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

        try
        {
            var calendarData = await _calendarService.DownloadCalendarAsync(_calendarUrl);
            var toDoItems = _calendarService.ParseCalendarDataToToDoItem(calendarData);
        
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(7);
        
            var toDosWithinNext7Days = toDoItems.Where(todo => 
                todo.SubmissionDate >= startDate && todo.SubmissionDate <= endDate
            ).ToList();

            if (toDosWithinNext7Days.Count > 0)
            {
                var embed = CreateEmbed(toDosWithinNext7Days);
                await channel.SendMessageAsync("", false, embed);
            
                return;
            }
        
            await channel.SendMessageAsync("Nothing to do for the next 7 days");
        }
        catch (HttpRequestException httpRequestException)
        {
            switch (httpRequestException.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    await channel.SendMessageAsync("Could not download calendar. Invalid calendar url.");
                    break;
                default:
                    await channel.SendMessageAsync("An error occured.");
                    break;
            }
        }
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