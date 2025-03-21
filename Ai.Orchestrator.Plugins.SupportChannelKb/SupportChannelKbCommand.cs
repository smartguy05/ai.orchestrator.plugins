using Ai.Orchestrator.Common.Extensions;
using Ai.Orchestrator.Models;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.SupportChannelKb.Models;

namespace Ai.Orchestrator.Plugins.SupportChannelKb;

public class SupportChannelKbCommand : ICommand
{
    public string Name => "Support Channel KB";
    public string Description => "Plugin for interfacing with the Support Channel Knowledge Base API";

    public async Task<object> Execute(OrchestratorRequest request, string configString, IEnumerable<ToolCall> availableToolCalls)
    {
        var serviceRequest = request.ServiceRequest.GetServiceRequest<ServiceRequest>();
        var config = configString.ReadConfig<ServiceConfig>();
        object result;

        var service = new SupportChannelKbService(config);
        
        try
        {
            result = serviceRequest.Method.ToLower() switch
            {
                "search" => await service.SearchKnowledgeBase(serviceRequest),
                "get_collections" => await service.GetCollections(),
                "add_collection" => await service.AddCollection(serviceRequest),
                "health_check" => await service.HealthCheck(),
                _ => new  { Success = false, Message = $"Action '{serviceRequest.Method}' not supported" }
            };
        }
        catch (Exception ex)
        {
            Console.Write($"Error executing action '{serviceRequest.Method}'", ex);
            return new
            {
                Success = false, 
                Message = $"Error: {ex.Message}"
            };
        }

        if (!string.IsNullOrWhiteSpace(request.ToolCallId))
        {
            return request.ReturnNewOrchestratorRequest(serviceRequest.RequestingService, result);
        }

        return result;
    }
}