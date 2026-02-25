using Ai.Orchestrator.Models.Enums;
using Ai.Orchestrator.Models.Extensions;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.SupportChannelKb.Models;

namespace Ai.Orchestrator.Plugins.SupportChannelKb;

public class SupportChannelKbCommand : CommandBase<ServiceRequest,ServiceConfig>
{
    public override string Name => "Ai.Orchestrator.Plugins.SupportChannelKb";
    public override string Description => "Plugin for interfacing with the Support Channel Knowledge Base API";
    protected override INotificationService NotificationService { get; set; }
    
    public override List<ToolCall> GetToolDefinitions()
    {
        return ServiceExtensions.GetServiceToolCalls<SupportChannelKbService>();
    }

    protected override async Task<object> DoWork(ServiceRequest serviceRequest, ServiceConfig config, IEnumerable<ToolCall> enumerableToolCalls)
    {
        try
        {
            var service = new SupportChannelKbService(config);
            return await service.ProcessRequest(serviceRequest, config, NotificationService);
        }
        catch (Exception ex)
        {
            await Log(LogLevel.Error, $"Error executing action '{serviceRequest.Method}'", ex);
            return new
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }
}