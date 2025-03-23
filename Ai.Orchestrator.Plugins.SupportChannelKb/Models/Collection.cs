using System.Text.Json.Serialization;

namespace Ai.Orchestrator.Plugins.SupportChannelKb.Models;

public class Collection
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Created { get; set; }
    [JsonPropertyName("api_key")]
    public string ApiKey { get; set; }
}