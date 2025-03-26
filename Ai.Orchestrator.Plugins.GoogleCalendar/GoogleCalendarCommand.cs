using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.GoogleCalendar.Models;

namespace Ai.Orchestrator.Plugins.GoogleCalendar;

public class GoogleCalendarCommand : CommandBase<ServiceRequest,ServiceConfig>
{
    public override string Name => "GoogleCalendar";
    public override string Description  => "Integration with Google Calendar";

    public override async Task<object> DoWork(ServiceRequest serviceRequest, ServiceConfig config, IEnumerable<ToolCall> availableToolCalls)
    {
        try
        {
            var calendarService = new CalService(config);
            return serviceRequest.Method.ToLower() switch
            {
                CalendarMethods.Events => await calendarService.GetEvents(serviceRequest),
                CalendarMethods.Event => await calendarService.GetEvent(serviceRequest),
                CalendarMethods.EditEvent => await calendarService.EditEvent(serviceRequest),
                CalendarMethods.Day => await calendarService.GetEventsForDay(serviceRequest),
                CalendarMethods.Range => await calendarService.GetEventsForDateRange(serviceRequest),
                CalendarMethods.Calendars => await calendarService.GetCalendars(serviceRequest),
                CalendarMethods.Calendar => await calendarService.GetCalendar(serviceRequest),
                _ => throw new Exception("Invalid Google Calendar command specified")
            };
        }
        catch (Exception e)
        {
            Console.Write($"Error executing action '{serviceRequest.Method}'", e);
            return new
            {
                Success = false, 
                Message = $"Error: {e.Message}"
            };
        }
    }
}