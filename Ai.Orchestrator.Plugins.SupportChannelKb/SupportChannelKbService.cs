using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ai.Orchestrator.Plugins.SupportChannelKb.Models;

namespace Ai.Orchestrator.Plugins.SupportChannelKb;

public class SupportChannelKbService
{
    private readonly ServiceConfig _config;

    public SupportChannelKbService(ServiceConfig  config)
    {
        _config = config;
    }

    public async Task<string[]> SearchKnowledgeBase(ServiceRequest request)
    {
        FilterOutDumbAiApiPlaceholders(request);
        using var httpClient = new HttpClient();
        
        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", request.ApiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var requestBody = new { text = request.SearchCriteria };
        
        var response = await httpClient.PostAsJsonAsync(
            $"{_config.SupportChannelKbUrl}/search/{request.SupportChannel}", 
            requestBody
        );

        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<string[]>();
    }
    
    public async Task<object> GetCollections()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.GetAsync($"{_config.SupportChannelKbUrl}/collections");
        response.EnsureSuccessStatusCode();

        var collections = await response.Content.ReadFromJsonAsync<Collection[]>();
        return collections;
    }

    public async Task<object> AddCollection(ServiceRequest request)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var requestBody = new 
        { 
            name = request.SupportChannel, 
            description = request.Description 
        };
        
        var response = await httpClient.PostAsJsonAsync(
            $"{_config.SupportChannelKbUrl}/collections", 
            requestBody
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<object>();
    }

    public async Task<object> AddTextToCollection(ServiceRequest request)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var requestBody = new 
        { 
            text = request.NewInformation, 
            data = request.Description,
            metaData = request.NewInformationMetaData
        };
        
        var response = await httpClient.PostAsJsonAsync(
            $"{_config.SupportChannelKbUrl}/text/{request.SupportChannel}", 
            requestBody
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<object>();
    }
    
    public async Task<object> HealthCheck()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.GetAsync($"{_config.SupportChannelKbUrl}/healthcheck");
        response.EnsureSuccessStatusCode();

        return response.Content.ReadFromJsonAsync<dynamic>();
    }

    private void FilterOutDumbAiApiPlaceholders(ServiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            throw new Exception("API Key is missing. Get API key using get_support_channel_collections");
        }

        var requestApiKey = request.ApiKey.ToLower();

        if (!Guid.TryParse(requestApiKey, out _))
        {
            throw new Exception("API Key is incorrect. Get API key using get_support_channel_collections");
        }
    }
}