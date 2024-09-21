using System.Globalization;
using System.Reflection;
using Ai.Orchestrator.Plugins.GoogleCalendar.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Ai.Orchestrator.Plugins.GoogleCalendar;

public class CalService
{
    private readonly CalendarService _calendarService;

    public CalService(ServiceConfig config)
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
        
        credential ??= GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = config.Credentials.ClientId,
                ClientSecret = config.Credentials.ClientSecret
            }, new[] { CalendarService.Scope.Calendar },
            config.Credentials.GoogleUser,
            CancellationToken.None,
            new FileDataStore(directory, true),
            new LocalServerCodeReceiver(config.LocalApiUrl)).Result;

        _calendarService = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Ai Orchestrator - Google Calendar Plugin",
        });
    }

    public async Task<object> GetEvents(ServiceRequest serviceRequest)
    {
        var calendarId = !string.IsNullOrWhiteSpace(serviceRequest.CalendarId) 
            ? serviceRequest.CalendarId 
            : "primary";
        try
        {
            return await _calendarService.Events.List(calendarId).ExecuteAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching event: {ex.Message}");
        }
    }
    
    public async Task<object> GetCalendar(ServiceRequest serviceRequest)
    {
        var calendarId = !string.IsNullOrWhiteSpace(serviceRequest.CalendarId) 
            ? serviceRequest.CalendarId 
            : "primary";
        try
        {
            return await _calendarService.Calendars.Get(calendarId).ExecuteAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching event: {ex.Message}");
        }
    }

    public async Task<object> GetCalendars()
    {
        try
        {
            return await _calendarService.CalendarList.List().ExecuteAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching event: {ex.Message}");
        }
    }
    
    public async Task<object> GetEvent(ServiceRequest serviceRequest)
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
            return await _calendarService.Events.Get(calendarId, eventId).ExecuteAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching event: {ex.Message}");
        }
    }

    // Edit event details on Google Calendar
    public async Task<object> EditEvent(ServiceRequest serviceRequest)
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
            var eventDetail = await _calendarService.Events.Get(calendarId, eventId).ExecuteAsync();
            eventDetail.Summary = summary;

            // Optionally edit other fields (start, end, etc.)

            var updatedEvent = await _calendarService.Events.Update(eventDetail, calendarId, eventId).ExecuteAsync();

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
    
    public async Task<object> GetEventsForDay(ServiceRequest serviceRequest)
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
            var request = _calendarService.Events.List(calendarId);
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