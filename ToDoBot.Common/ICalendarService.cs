namespace ToDoBot.Common;

public interface ICalendarService
{
    Task<string> DownloadCalendarAsync(string calendarUrl);
    List<ToDoItem> ParseCalendarDataToToDoItem(string calendarData);
}