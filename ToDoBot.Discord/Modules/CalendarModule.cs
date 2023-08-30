using Discord.Commands;

namespace ToDoBot.Discord.Modules;

public class CalendarModule : ModuleBase<SocketCommandContext>
{
    // TODO: Inject a repository to store that information in an SQLite db
    private static Dictionary<ulong, string> _calendars = new();
    
    [Command("setCalendar")]
    [Summary("Sets the url of the calendar")]
    public async Task SetCalendar(string calendarUrl)
    {
        if (string.IsNullOrEmpty(calendarUrl))
        {
            await ReplyAsync("You have to provide the url for the calendar.");
            return;
        }
        
        var userId = Context.User.Id;
        
        _calendars.Add(userId, calendarUrl);
        
        await ReplyAsync("Saved calendar url.");
    }
    
    [Command("removeCalendar")]
    [Summary("Removes the url of the calendar")]
    public async Task RemoveCalendar()
    {
        var userId = Context.User.Id;

        _calendars.Remove(userId);
        
        await ReplyAsync("Removed calendar url");
    }
}