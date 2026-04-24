using System.Net.Http.Json;
using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Enums;
using VivesRental.Sdk.Interfaces;

namespace VivesRental.Sdk.Services;

public class ArticleSdk : IArticleSdk
{
    private readonly HttpClient _httpClient;

    public ArticleSdk(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IList<ArticleResult>> FindAsync(Guid? productId = null)
    {
        var url = productId.HasValue 
            ? $"api/articles?productId={productId.Value}" 
            : "api/articles";
        var result = await _httpClient.GetFromJsonAsync<IList<ArticleResult>>(url);
        return result ?? new List<ArticleResult>();
    }

    public async Task<ArticleResult?> GetAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/articles/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<ArticleResult>();
    }

    public async Task<ArticleResult?> GetFirstAvailableAsync(Guid productId)
    {
        var response = await _httpClient.GetAsync($"api/articles/available/{productId}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<ArticleResult>();
    }

    public async Task<ArticleResult?> CreateAsync(ArticleRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/articles", request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<ArticleResult>();
    }

    public async Task<bool> UpdateStatusAsync(Guid id, ArticleStatus status)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/articles/{id}/status", status);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/articles/{id}");
        return response.IsSuccessStatusCode;
    }
}
