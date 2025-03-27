using System.Text.Json;
using Ai.Orchestrator.Models;
using Ai.Orchestrator.Models.Chat;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.Telegram.Models;
using Ai.Orchestrator.Models.Extensions;
using Ai.Orchestrator.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
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
            var clientUpdates = await _client.GetUpdates();

            return Task.Run(async () => {
                while (_client is not null)
                {
                    var updates = clientUpdates.ToList(); 
                    if (updates.Any())
                    {
                        var tryAgain = false;
                        List<Update> filteredUpdates = new ();
                        // only get updates sent in the last 5 minutes
                        foreach (var update in updates)
                        {
                            if ((DateTime.UtcNow - (update.Message?.Date ?? DateTime.UtcNow)).Minutes <= 5)
                            {
                                filteredUpdates.Add(update);
                            }
                        }

                        if (filteredUpdates.Any())
                        {
                            var lastMessage = filteredUpdates.Last();
                            var messages = filteredUpdates.Select(s => s.Message?.Text);
                            var concatenatedMessage = string.Join($". ", messages);
                            var conversationId = lastMessage.Message?.Chat.Username is not null
                                ? $"telegram-{lastMessage.Message?.Chat.Username}"
                                : null;
                            Console.WriteLine($"Telegram bot ${lastMessage.Message?.Chat.Id} received message: '{concatenatedMessage}'");

                            if (await ProcessSpecialCommands(conversationId, lastMessage.Message?.Chat.Id.ToString(), conversationId, concatenatedMessage))
                            {
                                clientUpdates = await _client.GetUpdates(updates.Last().Id + (tryAgain ? 0 : 1));
                                continue;
                            }
                            
                            var serviceRequest = new
                            {
                                SystemPrompt = (string)null,
                                UserPrompt = concatenatedMessage,
                                ConversationId = conversationId
                            };
                            var stringified = JsonSerializer.Serialize(serviceRequest);
                            var request = new OrchestratorRequest
                            {
                                Service = config.AiPlugin,
                                ServiceRequest = stringified
                            };

                            try
                            {
                                var orchestrator = ServiceResolver.GetService<IOrchestrator>();
                                var result = await orchestrator.ProcessRequest(request);
                        
                                var aiResponse = result?.GetType().GetProperty("Result");
                                var response = aiResponse?.GetValue(result) as string;
                                await SendMessage(config.BotToken, lastMessage.Message?.Chat.Id.ToString(), response);
                            }
                            catch (Exception e)
                            {
                                if (e.Message.ToLower()
                                    .Contains(
                                        "an assistant message with 'tool_calls' must be followed by tool messages"))
                                {
                                    var cachedMessages = await MessageCache.GetCachedMessages(conversationId);
                                    if (cachedMessages.Any())
                                    {
                                        var purgedMessages = RemoveNonUserMessagesFromEnd(cachedMessages);
                                        await MessageCache.SaveCachedMessages(conversationId, purgedMessages);
                                        Console.WriteLine($"Message cache polluted with toolcall error. Resetting message cache for id {lastMessage.Message.Chat.Id}");
                                        Console.WriteLine($"Original message list: {Environment.NewLine} {JsonSerializer.Serialize(cachedMessages)}");
                                        Console.WriteLine($"Purged message list: {Environment.NewLine} {JsonSerializer.Serialize(purgedMessages)}");
                                        tryAgain = true;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("An error occured while processing request for Telegram message", e);
                                    await SendMessage(config.BotToken, lastMessage.Message.Chat.Id.ToString(),
                                        $"An error occurred while processing request. Error: {e.Message}");
                                }
                            }
                        }

                        var offset = updates.Last().Id + (tryAgain ? 0 : 1);
                        clientUpdates = await _client.GetUpdates(offset);
                    }
                    else
                    {
                        clientUpdates = await _client.GetUpdates();
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

    private async Task<bool> ProcessSpecialCommands(string botToken, string chatId, string conversationId, string message)
    {
        if (message.StartsWith('/'))
        {
            string response = null;
            switch (message.ToLower())
            {
                case "/reset":
                    await MessageCache.ClearMessageCache(conversationId);
                    response = "Message cache reset";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(response))
            {
                await SendMessage(botToken, chatId, response);
            }
            return true;
        }

        return false;
    }
    
    private List<ChatMessageHistory> RemoveNonUserMessagesFromEnd(List<ChatMessageHistory> cachedMessages)
    {
        var modifiedMessages = new List<ChatMessageHistory>(cachedMessages);

        for (int i = modifiedMessages.Count - 1; i >= 0; i--)
        {
            // If we find a 'user' role message, stop removing
            if (modifiedMessages[i].Role == "user")
                break;

            // Remove messages that are not 'user' role
            modifiedMessages.RemoveAt(i);
        }

        return modifiedMessages;
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