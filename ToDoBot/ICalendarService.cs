namespace ToDoBot;

public interface ICalendarService
{
    Task<string> DownloadCalendarAsync(string icalLink);
    List<ToDoItem> ParseCalendarDataToToDoItem(string calendarData);
}