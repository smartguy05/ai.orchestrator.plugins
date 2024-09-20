namespace Ai.Orchestrator.Plugins.GoogleCalendar.Models;

public record ServiceRequest
{
    public string Method { get; init; }
    public string EventId { get; init; }
    public string CalendarId { get; init; }
    public string Summary { get; init; }
    public DateTime Date { get; init; }
}