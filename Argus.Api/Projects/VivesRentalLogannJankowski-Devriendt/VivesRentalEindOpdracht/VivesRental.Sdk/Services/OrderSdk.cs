using System.Net.Http.Json;
using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Sdk.Interfaces;

namespace VivesRental.Sdk.Services;

public class OrderSdk : IOrderSdk
{
    private readonly HttpClient _httpClient;

    public OrderSdk(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IList<OrderResult>> FindAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<IList<OrderResult>>("api/orders");
        return result ?? new List<OrderResult>();
    }

    public async Task<OrderResult?> GetAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/orders/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<OrderResult>();
    }

    public async Task<OrderResult?> CreateAsync(OrderRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/orders", request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<OrderResult>();
    }

    public async Task<bool> ReturnAsync(Guid id, DateTime returnedAt)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/orders/{id}/return", returnedAt);
        return response.IsSuccessStatusCode;
    }
}
