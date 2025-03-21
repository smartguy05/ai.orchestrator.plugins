using System.Net.Http.Headers;
using Ai.Orchestrator.Common.Extensions;
using Ai.Orchestrator.Models;
using Ai.Orchestrator.Models.Interfaces;
using Ai.Orchestrator.Models.Tools;
using Ai.Orchestrator.Plugins.UseMemos.Models;

namespace Ai.Orchestrator.Plugins.UseMemos;

public class UseMemosCommand: ICommand
{
    private readonly string[] _validTypes = { "read_memos", "edit_memo", "add_memo" };
    private readonly string[] _validGetTypes = { "memos", "resources" };
    
    public string Name => "UseMemos";
    public string Description  => "Integration with UseMemos server";

    public async Task<object> Execute(OrchestratorRequest request, string configString, IEnumerable<ToolCall> availableToolCalls)
    {
        var serviceRequest = request.ServiceRequest.GetServiceRequest<ServiceRequest>();
        var config = configString.ReadConfig<ServiceConfig>();
        
        if (serviceRequest is null)
        {
            throw new Exception("Unable to read usememos service request");
        }
        
        ValidateRequestType(serviceRequest.Method);

        object result;
        switch (serviceRequest.Method.ToLowerInvariant())
        {
            case "read_memos":
                result = await GetData(serviceRequest, config);
                break;
            case "edit_memo":
            case "add_memo":
                default:
                // todo: Implement add
                result = null;
                break;
        }
        
        if (!string.IsNullOrWhiteSpace(request.ToolCallId))
        {
            return request.ReturnNewOrchestratorRequest(serviceRequest.RequestingService, result);
        }

        return null;
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
