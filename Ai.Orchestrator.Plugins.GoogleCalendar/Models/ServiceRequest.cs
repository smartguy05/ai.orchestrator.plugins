using Ai.Orchestrator.Models.Interfaces;

namespace Ai.Orchestrator.Plugins.GoogleCalendar.Models;

public record ServiceRequest: IPluginServiceRequest
{
    public string Method { get; set; }
    public string ToolCallId { get; set; }
    public string RequestingService { get; set; }
    public string EventId { get; init; }
    public string CalendarId { get; init; }
    public string Summary { get; init; }
    public DateTime? Date { get; init; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}