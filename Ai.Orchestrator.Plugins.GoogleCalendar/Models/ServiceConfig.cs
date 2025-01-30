using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;

namespace Ai.Orchestrator.Plugins.GoogleCalendar.Models;

public record ServiceConfig: IPluginConfig
{
    public string Name { get; set; }
    public object Contract { get; set; }
    public string Description { get; set; }
    public IEnumerable<ToolCall> Tools { get; set; }
    public IEnumerable<string> ToolFunctions { get; set; }
    public GoogleApiCredentials Credentials { get; set; }
    public string LocalApiUrl { get; set; }
}