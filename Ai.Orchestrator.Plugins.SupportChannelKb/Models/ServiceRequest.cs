using Ai.Orchestrator.Models.Interfaces;

namespace Ai.Orchestrator.Plugins.SupportChannelKb.Models;

public record ServiceRequest: IPluginServiceRequest
{
    public string Method { get; set; }
    public string ToolCallId { get; set; }
    public string RequestingService { get; set; }
    
    public string SearchCriteria { get; set; }
    public string SupportChannel { get; set; }
}