namespace Ai.Orchestrator.Plugins.UseMemos.Models;

public record ServiceRequest
{
    public string Method { get; set; }
    public string Uid { get; set; }
    public string DataType { get; set; } = "memo";
}