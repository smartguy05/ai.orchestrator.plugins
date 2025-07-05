using System.Text.RegularExpressions;
using Ai.Orchestrator.Models;
using Ai.Orchestrator.Models.Enums;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.Telegram.Models;
using Ai.Orchestrator.Models.Extensions;
using Telegram.Bot;

namespace Ai.Orchestrator.Plugins.Telegram;

public class TelegramCommand: CommandBase<ServiceRequest, ServiceConfig>, IConfirmationPlugin
{
    public override string Name => "Ai.Orchestrator.Plugins.Telegram";
    public override string Description => "A plugin to send and receive telegram messages";
    protected override INotificationService NotificationService { get; set; }
    private static TelegramService _service;
    private static TelegramBotClient _botClient;
    private static ServiceConfig _config;
    private static INotificationService _staticConfirmationService;

    protected override async Task<object> DoWork(ServiceRequest serviceRequest, ServiceConfig config, IEnumerable<ToolCall> availableToolCalls)
    {
        if (string.IsNullOrEmpty(config.BotToken))
            throw new ArgumentException("Bot token is required");

        if (string.IsNullOrWhiteSpace(serviceRequest.ChatId))
        {
            serviceRequest.ChatId = config.NotificationChatId;
        }

        _service = GetTelegramService(config, NotificationService, Log);

        return await _service.SendMessage(serviceRequest.MessageText, serviceRequest.ChatId);
    }

    public override async Task<object> Initialize(string configString, LogDelegate log, INotificationService notificationService)
    {
        NotificationService ??= notificationService;
        _staticConfirmationService = notificationService;
        await log(LogLevel.Info, "Initializing Telegram");
        try
        {
            var config = configString.ReadPluginConfig<ServiceConfig>();
            _config ??= config;
            _botClient ??= new TelegramBotClient(config.BotToken);
            var clientUpdates = await _botClient.GetUpdates();

            _service = GetTelegramService(config, notificationService, log);
            // _service = new TelegramService(_botClient, log, config, _confirmationService, Confirm);

            return _service.ListenForMessages(clientUpdates.ToList());
        }
        catch (Exception e)
        {
            await log(LogLevel.Error, "Error initializing Telegram", e);
            throw;
        }
    }

    private TelegramService GetTelegramService(ServiceConfig config, INotificationService notificationService, LogDelegate log)
    {
        _config ??= config;
        _botClient ??= new TelegramBotClient(config.BotToken);
        log ??= Log;
        NotificationService ??= notificationService;

        return new TelegramService(_botClient, log, config, notificationService, Confirm);
    }
    
    public Task<object> RequestConfirmation(Confirmation confirmation, OrchestratorRequest request)
    {
        // DIAGNOSTIC STEP: Add a check that fails loudly and tells us exactly what is null.
        if (_botClient == null || _config == null || _staticConfirmationService == null)
        {
            throw new InvalidOperationException(
                $"TelegramCommand is not fully initialized. Status: " +
                $"BotClient is {(_botClient == null ? "null" : "not null")}, " +
                $"Config is {(_config == null ? "null" : "not null")}, " +
                $"ConfirmationService is {(_staticConfirmationService == null ? "null" : "not null")}."
            );
        }
        
        TelegramService.PendingConfirmation = confirmation;
        _service ??= new TelegramService(_botClient, Log, _config, NotificationService, Confirm);
        return _service.SendMessage(
            GetConfirmationMessage(confirmation), 
            _config.NotificationChatId, 
            confirmation.Options?.Select(s => s.Key).ToArray() ?? []
            );
    }

    public async ValueTask<object> Confirm(string confirmationId, bool confirm)
    {
        var guid = Guid.Parse(confirmationId);
        return await _staticConfirmationService.Confirm(guid, confirm);
    }

    public Task Dispose()
    {
        _service?.Dispose();

        return Task.CompletedTask;
    }

    private string GetConfirmationMessage(Confirmation confirmation)
    {
        if (confirmation.Content is null)
        {
            return confirmation.ConfirmationMessage;
        }
        var content = confirmation.Content;
        if (Regex.IsMatch(content, @".*<html>.+</html>.*", RegexOptions.Singleline))
        {
            content = content.Replace("<html>", "");
            content = content.Replace("</html>", "");
            content = content.Replace("<body>", "");
            content = content.Replace("</body>", "");
        }
        if (!string.IsNullOrWhiteSpace(confirmation.Content))
        {
            content = $"<html><body>{confirmation.ConfirmationMessage} {Environment.NewLine} {content}</body></html>";
        }

        return content;
    }
}