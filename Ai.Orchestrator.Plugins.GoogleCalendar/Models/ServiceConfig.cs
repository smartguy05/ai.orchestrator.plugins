using Ai.Orchestrator.Models.Interfaces;

namespace Ai.Orchestrator.Plugins.GoogleCalendar.Models;

public record ServiceConfig: IPluginConfig
{
    public object Contract { get; set; }
    public string Description { get; set; }
    public GoogleApiCredentials Credentials { get; set; }
    public string LocalApiUrl { get; set; }
}