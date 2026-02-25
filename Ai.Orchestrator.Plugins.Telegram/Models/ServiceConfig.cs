using Ai.Orchestrator.Models.Interfaces;

namespace Ai.Orchestrator.Plugins.Telegram.Models;

public class ServiceConfig: IPluginConfig
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string BotToken { get; set; }
    public string AiPlugin { get; set; }
    public string NotificationChatId { get; set; }
    public int PollingIntervalInMs { get; set; } = 1000;
}