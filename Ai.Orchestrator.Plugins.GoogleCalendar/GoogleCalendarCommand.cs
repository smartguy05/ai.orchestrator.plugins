using System.Text.Json;
using Ai.Orchestrator.Common.Extensions;
using Ai.Orchestrator.Models;
using Ai.Orchestrator.Models.Chat;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.GoogleCalendar.Models;

namespace Ai.Orchestrator.Plugins.GoogleCalendar;

public class GoogleCalendarCommand : ICommand
{
    public string Name => "GoogleCalendar";
    public string Description  => "Integration with Google Calendar";

    public async Task<object> Execute(OrchestratorRequest request, string configString, IEnumerable<ToolCall> availableToolCalls)
    {
        var serviceRequest = request.ServiceRequest.GetServiceRequest<ServiceRequest>();
        var config = configString.ReadConfig<ServiceConfig>();

        if (serviceRequest is null)
        {
            throw new Exception("Unable to read google calendar service request");
        }
        
        var calendarService = new CalService(config);
        object result;
        try
        {
            result = serviceRequest.Method.ToLower() switch
            {
                CalendarMethods.Events => await calendarService.GetEvents(serviceRequest),
                CalendarMethods.Event => await calendarService.GetEvent(serviceRequest),
                CalendarMethods.EditEvent => await calendarService.EditEvent(serviceRequest),
                CalendarMethods.Day => await calendarService.GetEventsForDay(serviceRequest),
                CalendarMethods.Calendars => await calendarService.GetCalendars(serviceRequest),
                CalendarMethods.Calendar => await calendarService.GetCalendar(serviceRequest),
                _ => throw new Exception("Invalid Google Calendar command specified")
            };
        }
        catch (Exception e)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.ToolCallId))
        {
            return request.ReturnNewOrchestratorRequest(serviceRequest.RequestingService, result);
        }

        return result;
    }
}