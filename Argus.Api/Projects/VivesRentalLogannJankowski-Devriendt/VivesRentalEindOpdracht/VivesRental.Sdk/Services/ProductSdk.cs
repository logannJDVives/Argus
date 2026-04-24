using System.Net.Http.Json;
using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Sdk.Interfaces;

namespace VivesRental.Sdk.Services;

public class ProductSdk : IProductSdk
{
    private readonly HttpClient _httpClient;

    public ProductSdk(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IList<ProductResult>> FindAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<IList<ProductResult>>("api/products");
        return result ?? new List<ProductResult>();
    }

    public async Task<ProductResult?> GetAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/products/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<ProductResult>();
    }

    public async Task<ProductResult?> CreateAsync(ProductRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/products", request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<ProductResult>();
    }

    public async Task<ProductResult?> UpdateAsync(Guid id, ProductRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<ProductResult>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/products/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> GenerateArticlesAsync(Guid id, int amount)
    {
        var response = await _httpClient.PostAsync($"api/products/{id}/generate-articles?amount={amount}", null);
        return response.IsSuccessStatusCode;
    }
}
