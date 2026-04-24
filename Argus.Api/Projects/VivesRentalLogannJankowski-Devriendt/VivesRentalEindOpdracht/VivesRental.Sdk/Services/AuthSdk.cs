using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using VivesRental.Sdk.Interfaces;
using VivesRental.Sdk.Models;
using VivesRental.Sdk.Stores;

namespace VivesRental.Sdk.Services;

public class AuthSdk : IAuthSdk
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStore _tokenStore;

    public AuthSdk(HttpClient httpClient, ITokenStore tokenStore)
    {
        _httpClient = httpClient;
        _tokenStore = tokenStore;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var request = new LoginRequest { Email = email, Password = password };
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        
        if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
        {
            _tokenStore.SetToken(loginResponse.Token);
        }

        return loginResponse;
    }

    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        return await response.Content.ReadFromJsonAsync<RegisterResponse>();
    }

    public void Logout()
    {
        _tokenStore.Clear();
    }

    public bool IsAuthenticated()
    {
        var token = _tokenStore.GetToken();
        if (string.IsNullOrEmpty(token)) return false;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    public string? GetUserEmail()
    {
        var token = _tokenStore.GetToken();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        }
        catch
        {
            return null;
        }
    }

    public string? GetUserRole()
    {
        var token = _tokenStore.GetToken();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        }
        catch
        {
            return null;
        }
    }

    public string? GetUserFirstName()
    {
        var token = _tokenStore.GetToken();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
        }
        catch
        {
            return null;
        }
    }

    public string? GetUserLastName()
    {
        var token = _tokenStore.GetToken();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
        }
        catch
        {
            return null;
        }
    }

    public Guid? GetUserId()
    {
        var token = _tokenStore.GetToken();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (Guid.TryParse(subClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public bool IsAdmin()
    {
        return GetUserRole() == "Admin";
    }
}
