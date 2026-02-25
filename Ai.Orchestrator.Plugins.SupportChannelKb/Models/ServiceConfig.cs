using Ai.Orchestrator.Models.Interfaces;

namespace Ai.Orchestrator.Plugins.SupportChannelKb.Models;

public record ServiceConfig: IPluginConfig
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string SupportChannelKbUrl { get; set; }
    public string DefaultSaveChannel { get; set; }
    public string DefaultChannelApiKey { get; set; }
}