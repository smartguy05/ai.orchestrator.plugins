using Ai.Orchestrator.Models.Interfaces;

namespace Ai.Orchestrator.Plugins.UseMemos.Models;

public record ServiceConfig: IPluginConfig
{
    public object Contract { get; set; }
    public string Description { get; set; }
    public MemoAccount MemoAccount { get; set; }
}