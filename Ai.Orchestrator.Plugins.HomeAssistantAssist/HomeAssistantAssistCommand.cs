using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Ai.Orchestrator.Models.Enums;
using Ai.Orchestrator.Models.Extensions;
using Ai.Orchestrator.Models.Helpers;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.HomeAssistantVoice.Models;

namespace Ai.Orchestrator.Plugins.HomeAssistantVoice;

public class HomeAssistantAssistCommand : CommandBase<ServiceRequest, ServiceConfig>
{
    public override string Name => "Ai.Orchestrator.Plugins.HomeAssistantAssist";
    public override string Description => "A plugin to send natural language commands to Home Assistant";
    protected override INotificationService NotificationService { get; set; }

    public override List<ToolCall> GetToolDefinitions()
    {
        return this.GetServiceToolCalls();
    }

    protected override async Task<object> DoWork(ServiceRequest serviceRequest, ServiceConfig config, IEnumerable<ToolCall> availableToolCalls)
    {
        return await this.ProcessRequest(serviceRequest, config, NotificationService);
    }

    [Display(Name = "home_assistant_command")]
    [Description("Sends a natural language command to Home Assistant to control smart home devices")]
    [Parameters("""{"type":"object","properties":{"query":{"type":"string","description":"The natural language command to send to Home Assistant"}},"required":["query"]}""")]
    public async Task<object> HomeAssistantCommand(ServiceConfig config, ServiceRequest serviceRequest)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(config.HomeAssistApiKey))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.HomeAssistApiKey);
        }

        var body = new
        {
            text = serviceRequest.Query,
            language = "en"
        };
        var jsonBody = JsonSerializer.Serialize(body);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(config.HomeAssistUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            await Log(LogLevel.Warning, "Unable to execute Home Assistant command");
            await Log(LogLevel.Info, await response.Content.ReadAsStringAsync());
            return new
            {
                Success = false
            };
        }

        return await response.Content.ReadAsStringAsync();
    }
}
