using Ai.Orchestrator.Models.Interfaces;

namespace Ai.Orchestrator.Plugins.PythonRunner.Models;

public class ServiceConfig: IPluginConfig
{
    public string Name { get; set; }
    public string Description { get; set; }
}