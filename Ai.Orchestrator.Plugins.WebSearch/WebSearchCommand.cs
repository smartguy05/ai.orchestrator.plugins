using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ai.Orchestrator.Models.Enums;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.WebSearch.Models;

namespace Ai.Orchestrator.Plugins.WebSearch;

public class WebSearchCommand: CommandBase<ServiceRequest, ServiceConfig>
{
    public override string Name => "WebSearch";
    public override string Description => "A plugin to allow searching the web";

    protected override async Task<object> DoWork(ServiceRequest serviceRequest, ServiceConfig config, IEnumerable<ToolCall> availableToolCalls)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        if (!string.IsNullOrWhiteSpace(config.WebSearchApiKey))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", config.WebSearchApiKey);
        }

        var url = config.WebSearchUrl;
        url += url.Contains('?') ? "&" : "?";
        url += $"q={Uri.EscapeDataString(serviceRequest.Query)}&limit={serviceRequest.MaxResults}";

        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            await Log(LogLevel.Warning, "Unable to get web search results");
            await Log(LogLevel.Info, await response.Content.ReadAsStringAsync());
            return new
            {
                Success = false
            };
        }

        return await response.Content.ReadFromJsonAsync<dynamic>();
    }
}