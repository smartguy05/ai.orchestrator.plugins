using Ai.Orchestrator.Models.Interfaces;

namespace Ai.Orchestrator.Plugins.HomeAssistantVoice.Models;

public class ServiceRequest: IPluginServiceRequest
{
    public string Method { get; set; }
    public string ToolCallId { get; set; }
    public string RequestingService { get; set; }
    public string Query { get; set; }
}