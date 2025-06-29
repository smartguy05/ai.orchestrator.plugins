using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Ai.Orchestrator.Models.Enums;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.UseMemos.Models;

namespace Ai.Orchestrator.Plugins.UseMemos;

public class UseMemosCommand: CommandBase<ServiceRequest,ServiceConfig>
{
    public override string Name => "Ai.Orchestrator.Plugins.UseMemos";
    public override string Description  => "Integration with UseMemos server";
    protected override IConfirmationService ConfirmationService { get; set; }

    private readonly string[] _validTypes = { "read_memos", "add_memo", "edit_memo", "update_memo" };
    private readonly string[] _validGetTypes = { "memos", "resources" };
    
    protected override async Task<object> DoWork(ServiceRequest serviceRequest, ServiceConfig config, IEnumerable<ToolCall> enumerableToolCalls)
    {
        ValidateRequestType(serviceRequest.Method);

        switch (serviceRequest.Method.ToLowerInvariant())
        {
            case "read_memos":
                return await ReadMemos(serviceRequest, config);
            case "add_memo":
                return await AddMemo(serviceRequest, config);
            case "delete_memo":
            case "update_memo":
                return await UpdateMemo(serviceRequest, config);
            case "create_resource":
            case "delete_resource":
            case "update_resource":
            case "create_tag":
            case "delete_tag":
            default:
                // todo: Implement
                return null;
        }
    }

    private void ValidateRequestType(string method)
    {
        if (!_validTypes.Contains(method.ToLowerInvariant()))
        {
            throw new Exception("Invalid method specified");
        }
    }
    
    private void ValidateDataType(string method)
    {
        if (!_validGetTypes.Contains(method.ToLower()))
        {
            throw new Exception("Invalid Get method specified");
        }
    }

    private async Task<object> ReadMemos(ServiceRequest serviceRequest, ServiceConfig config)
    {
        ValidateDataType(serviceRequest.DataType);
        using var httpClient = new HttpClient(); 
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.MemoAccount.ApiKey);
        try
        {
            var location = serviceRequest.DataType.ToLower();
            var url = $"{config.MemoAccount.MemosUrl}/api/v1/{location}";
            if (!string.IsNullOrWhiteSpace(serviceRequest.Uid))
            {
                url += $":by-uid/{serviceRequest.Uid}";
            }
            var response = await httpClient.GetAsync(new Uri(url));
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            await Log(LogLevel.Error, $"An error occurred while attempting to get {serviceRequest.DataType}: {e}", e);
            throw;
        }
    }

    private async Task<object> AddMemo(ServiceRequest serviceRequest, ServiceConfig config)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.MemoAccount.ApiKey);

        var url = $"{config.MemoAccount.MemosUrl}/api/v1/memos";

        if (string.IsNullOrWhiteSpace(serviceRequest.Content))
        {
            throw new ArgumentException("Content cannot be empty for adding a memo.");
        }

        var visibility = !string.IsNullOrWhiteSpace(serviceRequest.Visibility) ? serviceRequest.Visibility.ToUpperInvariant() : "PRIVATE";
        // Validate visibility if necessary, e.g., against a list of valid visibilities: "PUBLIC", "PROTECTED", "PRIVATE"
        // For now, we assume the user provides a valid string or it defaults to PRIVATE.

        var memoPayload = new Dictionary<string, object>
        {
            { "content", serviceRequest.Content },
            { "visibility", visibility }
        };
        
        // Optionally, handle resourceIdList and relationList from serviceRequest.AdditionalParameters
        // Example for resourceIdList:
        // if (serviceRequest.AdditionalParameters != null && serviceRequest.AdditionalParameters.TryGetValue("resourceIdList", out var resourceIdsJson))
        // {
        //     try {
        //         var resourceIds = JsonSerializer.Deserialize<List<int>>(resourceIdsJson);
        //         if (resourceIds != null && resourceIds.Any()) {
        //            memoPayload.Add("resourceIdList", resourceIds);
        //         }
        //     } catch (JsonException ex) {
        //         Console.WriteLine($"Error deserializing resourceIdList: {ex.Message}");
        //         // Handle error or ignore
        //     }
        // }


        var jsonPayload = JsonSerializer.Serialize(memoPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(new Uri(url), content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            await Log(LogLevel.Error, $"An error occurred while attempting to add memo: {e}", e);
            throw;
        }
    }

    private async Task<object> UpdateMemo(ServiceRequest serviceRequest, ServiceConfig config)
    {
        if (string.IsNullOrWhiteSpace(serviceRequest.Uid))
        {
            throw new ArgumentException("Memo Uid (memoId) cannot be empty for updating a memo.");
        }

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.MemoAccount.ApiKey);

        var url = $"{config.MemoAccount.MemosUrl}/api/v1/memos/{serviceRequest.Uid}";
        
        var memoPatchPayload = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(serviceRequest.Content))
        {
            memoPatchPayload.Add("content", serviceRequest.Content);
        }

        if (!string.IsNullOrWhiteSpace(serviceRequest.Visibility))
        {
            // Validate visibility if necessary
            memoPatchPayload.Add("visibility", serviceRequest.Visibility.ToUpperInvariant());
        }
        
        // Optionally, handle resourceIdList and relationList updates from serviceRequest.AdditionalParameters
        // Similar to AddMemo, but be mindful that PATCH updates only specified fields.
        // If an empty list is provided for resourceIdList, it might clear existing resources.
        // The API docs should clarify this behavior.

        if (!memoPatchPayload.Any())
        {
            // Or return a message indicating nothing to update, or proceed if API handles empty patch as no-op
            throw new ArgumentException("No fields provided to update for the memo."); 
        }

        var jsonPayload = JsonSerializer.Serialize(memoPatchPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri(url))
            {
                Content = content
            };
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            await Log(LogLevel.Error, $"An error occurred while attempting to update memo {serviceRequest.Uid}: {e}", e);
            throw;
        }
    }

    private async Task<object> CreateResource(ServiceRequest serviceRequest, ServiceConfig config)
    {
        throw new NotImplementedException();
    }

    private async Task<object> DeleteResource(ServiceRequest serviceRequest, ServiceConfig config) {
        throw new NotImplementedException();
    }

    private async Task<object> UpdateResource(ServiceRequest serviceRequest, ServiceConfig config) {
        throw new NotImplementedException();
    }

    private async Task<object> CreateTag(ServiceRequest serviceRequest, ServiceConfig config) {
        throw new NotImplementedException();
    }

    private async Task<object> DeleteTag(ServiceRequest serviceRequest, ServiceConfig config) {
        throw new NotImplementedException();
    }
}
