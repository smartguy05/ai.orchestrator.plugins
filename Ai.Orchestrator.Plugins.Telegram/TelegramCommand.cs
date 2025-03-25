using System.Text.Json;
using Ai.Orchestrator.Models;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.Telegram.Models;
using Ai.Orchestrator.Models.Extensions;
using Ai.Orchestrator.Services;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Ai.Orchestrator.Plugins.Telegram;

public class TelegramCommand: CommandBase<ServiceRequest, ServiceConfig>
{
    private TelegramBotClient _client;
    public override string Name => "Telegram";
    public override string Description => "A plugin to send and receive telegram messages";

    public override async Task<object> DoWork(ServiceRequest serviceRequest, ServiceConfig config, IEnumerable<ToolCall> availableToolCalls)
    {
        if (string.IsNullOrEmpty(config.BotToken))
            throw new ArgumentException("Bot token is required");
        
        serviceRequest.ChatId ??= config.NotificationChatId;

        return await SendMessage(config.BotToken, serviceRequest.ChatId, serviceRequest.MessageText);
    }

    public override async Task<object> Initialize(string configString)
    {
        Console.WriteLine("Initializing Telegram");
        try
        {
            var config = configString.ReadConfig<ServiceConfig>();
            _client = new TelegramBotClient(config.BotToken);
            var updates = await _client.GetUpdates();

            return Task.Run(async () => {
                while (_client is not null)
                {
                    if (updates.Any())
                    {
                        var lastMessage = updates.Last();
                        var messages = updates.Select(s => s.Message?.Text);
                        var concatenatedMessage = string.Join("\n", messages);
                        Console.WriteLine($"Telegram bot ${lastMessage.Message?.Chat.Id} received message: '{concatenatedMessage}'");

                        var serviceRequest = new
                        {
                            SystemPrompt = (string)null,
                            UserPrompt = concatenatedMessage,
                            ConversationId = lastMessage.Message?.Chat.Username is not null ? $"telegram-{lastMessage.Message?.Chat.Username}" : null
                        };
                        var stringified = JsonSerializer.Serialize(serviceRequest);
                        var request = new OrchestratorRequest
                        {
                            Service = config.AiPlugin,
                            ServiceRequest = stringified
                        };

                        var orchestrator = ServiceResolver.GetService<IOrchestrator>();
                        var result = await orchestrator.ProcessRequest(request);
                        
                        var aiResponse = result?.GetType().GetProperty("Result");
                        var response = aiResponse?.GetValue(result) as string;
                        var offset = updates.Last().Id + 1;
                        updates = await _client.GetUpdates(offset);
                        await SendMessage(config.BotToken, lastMessage.Message?.Chat.Id.ToString(), response);
                    }
                    else
                    {
                        updates = await _client.GetUpdates();
                    }
                }
            });
        }
        catch (Exception e)
        {
            Console.WriteLine("Error initializing Telegram");
            Console.WriteLine(e);
        }

        return null;
    }

    public Task Dispose()
    {
        if (_client is not null)
        {
            _client = null;
        }

        return Task.CompletedTask;
    }

    private async Task<object> SendMessage(string botToken, string chatId, string messageText)
    {
        if (string.IsNullOrEmpty(chatId))
            throw new ArgumentException("Chat ID is required");

        if (_client is null)
        {
            _client = new TelegramBotClient(botToken);
        }

        try 
        {
            var message = await _client.SendMessage(
                chatId: chatId,
                text: messageText,
                parseMode: ParseMode.Markdown
            );

            return new 
            {
                Success = true,
                message.MessageId,
                message.Text,
                SentAt = message.Date
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Telegram message send failed: {ex.Message}", ex);
            return new
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}