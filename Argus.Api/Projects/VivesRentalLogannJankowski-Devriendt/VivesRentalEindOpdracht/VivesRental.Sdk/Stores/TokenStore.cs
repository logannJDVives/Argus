using Microsoft.JSInterop;

namespace VivesRental.Sdk.Stores;

public class TokenStore : ITokenStore
{
    private readonly IJSRuntime _jsRuntime;
    private const string TokenKey = "vivesrental_token";
    private string? _cachedToken;

    public TokenStore(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public string? GetToken()
    {
        return _cachedToken;
    }

    public void SetToken(string token)
    {
        _cachedToken = token;
        // Fire and forget - opslaan in localStorage
        _ = SetTokenAsync(token);
    }

    public void Clear()
    {
        _cachedToken = null;
        _ = ClearTokenAsync();
    }

    public async Task InitializeAsync()
    {
        try
        {
            _cachedToken = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        }
        catch
        {
            _cachedToken = null;
        }
    }

    private async Task SetTokenAsync(string token)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        }
        catch
        {
            // Ignore errors during prerendering
        }
    }

    private async Task ClearTokenAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        }
        catch
        {
            // Ignore errors during prerendering
        }
    }
}
