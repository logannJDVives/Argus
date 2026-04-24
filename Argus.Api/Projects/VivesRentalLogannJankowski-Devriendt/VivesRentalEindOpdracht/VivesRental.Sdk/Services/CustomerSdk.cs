using System.Net.Http.Json;
using VivesRental.Dto.Requests;
using VivesRental.Dto.Results;
using VivesRental.Sdk.Interfaces;

namespace VivesRental.Sdk.Services;

public class CustomerSdk : ICustomerSdk
{
    private readonly HttpClient _httpClient;

    public CustomerSdk(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IList<CustomerResult>> FindAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<IList<CustomerResult>>("api/customers");
        return result ?? new List<CustomerResult>();
    }

    public async Task<CustomerResult?> GetAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/customers/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<CustomerResult>();
    }

    public async Task<CustomerResult?> CreateAsync(CustomerRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/customers", request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<CustomerResult>();
    }

    public async Task<CustomerResult?> UpdateAsync(Guid id, CustomerRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/customers/{id}", request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<CustomerResult>();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/customers/{id}");
        return response.IsSuccessStatusCode;
    }
}
