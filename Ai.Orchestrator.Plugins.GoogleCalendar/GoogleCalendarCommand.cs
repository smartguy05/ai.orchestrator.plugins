using Ai.Orchestrator.Common.Extensions;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Plugins.GoogleCalendar.Models;

namespace Ai.Orchestrator.Plugins.GoogleCalendar;

public class GoogleCalendarCommand : ICommand
{
    public string Name => "GoogleCalendar";
    public string Description  => "Integration with Google Calendar";

    public async Task<object> Execute(object request, string configString)
    {
        var serviceRequest = request.GetServiceRequest<ServiceRequest>();
        var config = configString.ReadConfig<ServiceConfig>();

        var calendarService = new CalService(config);

        return serviceRequest.Method.ToLower() switch
        {
            CalendarMethods.Events => await calendarService.GetEvents(serviceRequest),
            CalendarMethods.Event => await calendarService.GetEvent(serviceRequest),
            CalendarMethods.EditEvent => await calendarService.EditEvent(serviceRequest),
            CalendarMethods.Day => await calendarService.GetEventsForDay(serviceRequest),
            CalendarMethods.Calendars => await calendarService.GetCalendars(),
            CalendarMethods.Calendar => await calendarService.GetCalendar(serviceRequest),
            _ => throw new Exception("Invalid Google Calendar command specified")
        };
    }
}