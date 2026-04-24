using System.Net.Http.Json;
using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Sdk.Interfaces;

namespace VivesRental.Sdk.Services;

public class ArticleReservationSdk : IArticleReservationSdk
{
    private readonly HttpClient _httpClient;

    public ArticleReservationSdk(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IList<ArticleReservationResult>> FindAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<IList<ArticleReservationResult>>("api/articlereservations");
        return result ?? new List<ArticleReservationResult>();
    }

    public async Task<ArticleReservationResult?> GetAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/articlereservations/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<ArticleReservationResult>();
    }

    public async Task<ArticleReservationResult?> CreateAsync(ArticleReservationRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/articlereservations", request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<ArticleReservationResult>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/articlereservations/{id}");
        return response.IsSuccessStatusCode;
    }
}
