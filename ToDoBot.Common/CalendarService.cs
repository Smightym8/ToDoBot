using System.Globalization;
using System.Text.RegularExpressions;

namespace ToDoBot.Common;

public class CalendarService : ICalendarService
{
    public async Task<string> DownloadCalendarAsync(string calendarUrl)
    {
        using var httpClient = new HttpClient();
        
        var calendar = await httpClient.GetStringAsync(calendarUrl);
        return calendar;
    }

    public List<ToDoItem> ParseCalendarDataToToDoItem(string calendarData)
    {
        var toDoItems = new List<ToDoItem>();
        var vEventPattern = @"BEGIN:VEVENT(?<vevent>.*?)END:VEVENT";
        
        var matches = Regex.Matches(calendarData, vEventPattern, RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            if (!match.Success) continue;
            
            var vEvent = match.Groups["vevent"].Value;
            var vEventComponents = vEvent.Split("\r\n");

            string? title = null;
            string? submissionDate = null;

            foreach (var component in vEventComponents)
            {
                if (component.Contains("DTSTART"))
                {
                    var value = component.Split(":")[1];
                    submissionDate = value;
                }
                else if (component.Contains("SUMMARY"))
                {
                    var value = component.Split(":")[1];
                    title = value;
                }
            }

            if (title == null || submissionDate == null) continue;

            var parsedSubmissionDate = ParseIcsDateTime(submissionDate);
            var toDoItem = new ToDoItem(title, parsedSubmissionDate);
            toDoItems.Add(toDoItem);
        }

        return toDoItems;
    }

    private DateTime ParseIcsDateTime(string icsDateTime)
    {
        string[] formats = {
            "yyyyMMddTHHmmssZ",          // Default format in ICS
            "yyyy-MM-ddTHH:mm:ssZ",      // ISO 8601-Format
            "yyyy-MM-dd HH:mm:ss",       
            "yyyyMMdd",
            "yyyyMMddTHHmmss"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(icsDateTime, format, CultureInfo.InvariantCulture, 
                    DateTimeStyles.AssumeUniversal, out DateTime parsedDate))
            {
                return parsedDate.ToLocalTime();
            }
        }
        
        throw new ArgumentException($"Unsupported ICS-DateTime format: {icsDateTime}", nameof(icsDateTime));
    }
}