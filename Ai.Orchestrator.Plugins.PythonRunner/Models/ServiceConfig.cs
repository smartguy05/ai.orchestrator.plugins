using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;

namespace Ai.Orchestrator.Plugins.PythonRunner.Models;

public class ServiceConfig: IPluginConfig
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<ToolCall> Tools { get; set; }
}