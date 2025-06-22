using Ai.Orchestrator.Models.Enums;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.SupportChannelKb.Models;

namespace Ai.Orchestrator.Plugins.SupportChannelKb;

public class SupportChannelKbCommand : CommandBase<ServiceRequest,ServiceConfig>
{
    public override string Name => "Support Channel KB";
    public override string Description => "Plugin for interfacing with the Support Channel Knowledge Base API";

    protected override async Task<object> DoWork(ServiceRequest serviceRequest, ServiceConfig config, IEnumerable<ToolCall> enumerableToolCalls)
    {
        try
        {
            var service = new SupportChannelKbService(config);
            return serviceRequest.Method.ToLower() switch
            {
                "search_support_channels" => await service.SearchKnowledgeBase(serviceRequest),
                "get_support_channel_collections" => await service.GetCollections(),
                "add_support_channel_collection" => await service.AddCollection(serviceRequest),
                "save_support_channel_information" => await service.AddTextToCollection(serviceRequest),
                "support_channel_health_check" => await service.HealthCheck(),
                _ => new  { Success = false, Message = $"Action '{serviceRequest.Method}' not supported" }
            };
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