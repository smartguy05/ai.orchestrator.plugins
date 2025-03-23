using System.Net.Http.Headers;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.UseMemos.Models;

namespace Ai.Orchestrator.Plugins.UseMemos;

public class UseMemosCommand: CommandBase<ServiceRequest,ServiceConfig>
{
    private readonly string[] _validTypes = { "read_memos" };
    private readonly string[] _validGetTypes = { "memos", "resources" };
    
    public override string Name => "UseMemos";
    public override string Description  => "Integration with UseMemos server";

    public override async Task<object> DoWork(ServiceRequest serviceRequest, ServiceConfig config, IEnumerable<ToolCall> enumerableToolCalls)
    {
        ValidateRequestType(serviceRequest.Method);

        switch (serviceRequest.Method.ToLowerInvariant())
        {
            case "read_memos":
                return await GetData(serviceRequest, config);
            case "edit_memo":
            case "add_memo":
            default:
                // todo: Implement
                return null;
        }
    }

    private void ValidateRequestType(string method)
    {
        if (!_validTypes.Contains(method.ToLower()))
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

    private async Task<object> GetData(ServiceRequest serviceRequest, ServiceConfig config)
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
            Console.WriteLine($"An error occurred while attempting to get {serviceRequest.DataType}: {e}");
            throw;
        }
    }
}
