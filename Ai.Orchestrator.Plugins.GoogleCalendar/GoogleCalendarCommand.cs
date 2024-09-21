using System.Globalization;
using System.Reflection;
using Ai.Orchestrator.Common.Extensions;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Plugins.GoogleCalendar.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Ai.Orchestrator.Plugins.GoogleCalendar;

public class GoogleCalendarCommand : ICommand
{
    public string Name => "GoogleCalendar";
    public string Description  => "Integration with Google Calendar";

    public async Task<object> Execute(object request, string configString)
    {
        var serviceRequest = request.GetServiceRequest<ServiceRequest>();
        var config = configString.ReadConfig<ServiceConfig>();
        
        var calendarService = await GetCalendarService(config);
        
        switch (serviceRequest.Method.ToLower())
        {
            case "get":
                return await GetEventAsync(calendarService, serviceRequest);
            case "edit":
                return await EditEventAsync(calendarService, serviceRequest);
            case "get-day":
                return await GetEventsForDayAsync(calendarService, serviceRequest);
            default:
                throw new Exception("Invalid Google Calendar command specified");
        }
    }

    private async Task<CalendarService> GetCalendarService(ServiceConfig config)
    {
        var directory = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/GoogleCalendar";
        var files = Directory.GetFiles(directory, "*.TokenResponse-user");
        ICredential credential = null;

        if (files.Any())
        {
            if (files.Length == 1)
            {
                var fileName = files[0];
                credential = GoogleCredential.FromFile($"{directory}/{fileName}")
                    .CreateScoped(CalendarService.Scope.Calendar);   
            }
            else
            {
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
        }
        
        credential ??= await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = config.Credentials.ClientId,
                ClientSecret = config.Credentials.ClientSecret
            }, new[] { CalendarService.Scope.Calendar },
            config.Credentials.GoogleUser,
            CancellationToken.None,
            new FileDataStore(directory, true),
            new LocalServerCodeReceiver(config.LocalApiUrl));

        return new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Ai Orchestrator - Google Calendar Plugin",
        });
    }

    // Fetch event details from Google Calendar
    private async Task<object> GetEventAsync(CalendarService calendarService, ServiceRequest serviceRequest)
    {
        if (string.IsNullOrWhiteSpace(serviceRequest.EventId))
        {
            throw new Exception("Missing parameter: eventId.");
        }

        var eventId = serviceRequest.EventId;
        var calendarId = !string.IsNullOrWhiteSpace(serviceRequest.CalendarId) 
            ? serviceRequest.CalendarId 
            : "primary";

        try
        {
            return await calendarService.Events.Get(calendarId, eventId).ExecuteAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching event: {ex.Message}");
        }
    }

    // Edit event details on Google Calendar
    private async Task<object> EditEventAsync(CalendarService calendarService, ServiceRequest serviceRequest)
    {
        if (string.IsNullOrWhiteSpace(serviceRequest.EventId) || string.IsNullOrWhiteSpace(serviceRequest.Summary))
        {
            throw new Exception("Missing required parameters: eventId, summary.");
        }

        var eventId = serviceRequest.EventId;
        var summary = serviceRequest.Summary;
        var calendarId = !string.IsNullOrWhiteSpace(serviceRequest.CalendarId) 
            ? serviceRequest.CalendarId
            : "primary";

        try
        {
            var eventDetail = await calendarService.Events.Get(calendarId, eventId).ExecuteAsync();
            eventDetail.Summary = summary;

            // Optionally edit other fields (start, end, etc.)

            var updatedEvent = await calendarService.Events.Update(eventDetail, calendarId, eventId).ExecuteAsync();

            return new
            {
                EventId = updatedEvent.Id,
                updatedEvent.Summary,
                Start = updatedEvent.Start.DateTimeRaw,
                End = updatedEvent.End.DateTimeRaw
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating event: {ex.Message}");
        }
    }
    
    private async Task<object> GetEventsForDayAsync(CalendarService calendarService, ServiceRequest serviceRequest)
    {
        if (string.IsNullOrWhiteSpace("date"))
        {
            throw new Exception("Missing parameter: date.");
        }

        var calendarId = !string.IsNullOrWhiteSpace(serviceRequest.CalendarId) 
            ? serviceRequest.CalendarId
            : "primary";
        
        // Parse the input date
        if (!DateTime.TryParseExact(serviceRequest.Date.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            throw new Exception("Invalid date format. Use 'yyyy-MM-dd'.");
        }

        try
        {
            // Set the timeMin and timeMax to filter events for the specific day
            var request = calendarService.Events.List(calendarId);
            request.TimeMin = date;
            request.TimeMax = date.AddDays(1).AddTicks(-1); // End of the day
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var events = await request.ExecuteAsync();
            if (events.Items == null || events.Items.Count == 0)
            {
                throw new Exception("No events found for the specified day.");
            }

            // Format the event list
            return events.Items.Select(e => new
            {
                EventId = e.Id,
                Summary = e.Summary,
                Start = e.Start.DateTimeRaw ?? e.Start.Date, // In case it's an all-day event
                End = e.End.DateTimeRaw ?? e.End.Date
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching events: {ex.Message}");
        }
    }
}