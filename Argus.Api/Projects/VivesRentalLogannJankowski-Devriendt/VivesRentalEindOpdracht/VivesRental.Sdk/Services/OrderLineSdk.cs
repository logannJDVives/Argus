using System.Net.Http.Json;
using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Sdk.Interfaces;

namespace VivesRental.Sdk.Services;

public class OrderLineSdk : IOrderLineSdk
{
    private readonly HttpClient _httpClient;

    public OrderLineSdk(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IList<OrderLineResult>> FindAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<IList<OrderLineResult>>("api/orderlines");
        return result ?? new List<OrderLineResult>();
    }

    public async Task<OrderLineResult?> GetAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/orderlines/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<OrderLineResult>();
    }

    public async Task<bool> RentAsync(OrderLineRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/orderlines/rent", request);
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> RentMultipleAsync(Guid orderId, IList<Guid> articleIds)
    {
        var request = new { OrderId = orderId, ArticleIds = articleIds };
        var response = await _httpClient.PostAsJsonAsync("api/orderlines/rent-multiple", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ReturnAsync(Guid id, DateTime returnedAt)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/orderlines/{id}/return", returnedAt);
        return response.IsSuccessStatusCode;
    }
}
