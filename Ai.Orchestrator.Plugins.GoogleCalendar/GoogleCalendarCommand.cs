using Ai.Orchestrator.Models.Enums;
using Ai.Orchestrator.Models.Extensions;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.GoogleCalendar.Models;

namespace Ai.Orchestrator.Plugins.GoogleCalendar;

public class GoogleCalendarCommand : CommandBase<ServiceRequest,ServiceConfig>
{
    public override string Name => "Ai.Orchestrator.Plugins.GoogleCalendar";
    public override string Description  => "Integration with Google Calendar";
    protected override INotificationService NotificationService { get; set; }

    public override List<ToolCall> GetToolDefinitions()
    {
        return ServiceExtensions.GetServiceToolCalls<CalService>();
    }

    protected override async Task<object> DoWork(ServiceRequest serviceRequest, ServiceConfig config, IEnumerable<ToolCall> availableToolCalls)
    {
        try
        {
            var calendarService = new CalService(config, Logger);
            return await calendarService.ProcessRequest(serviceRequest, config, NotificationService);
        }
        catch (Exception e)
        {
            await Log(LogLevel.Error, $"Error executing action '{serviceRequest.Method}'", e);
            return new
            {
                Success = false,
                Message = $"Error: {e.Message}"
            };
        }
    }
}