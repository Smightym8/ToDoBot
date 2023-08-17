namespace ToDoBot.Common;

public class ToDoItem
{
    public string Title { get; }
    public DateTime SubmissionDate { get; }

    public ToDoItem(string title, DateTime submissionDate)
    {
        Title = title;
        SubmissionDate = submissionDate;
    }
}