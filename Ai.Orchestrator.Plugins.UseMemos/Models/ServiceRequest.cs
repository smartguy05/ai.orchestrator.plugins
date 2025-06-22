using Ai.Orchestrator.Models.Interfaces;

namespace Ai.Orchestrator.Plugins.UseMemos.Models;

public record ServiceRequest: IPluginServiceRequest
{
    public string Method { get; set; }
    public string ToolCallId { get; set; }
    public string RequestingService { get; set; }
    public string Uid { get; set; }
    public string DataType { get; set; } = "memos";
    public string Content { get; set; }
    public string Visibility { get; set; }
}