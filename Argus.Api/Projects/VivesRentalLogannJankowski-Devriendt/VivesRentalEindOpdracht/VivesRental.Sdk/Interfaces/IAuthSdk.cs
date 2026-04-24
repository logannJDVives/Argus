using VivesRental.Sdk.Models;

namespace VivesRental.Sdk.Interfaces;

public interface IAuthSdk
{
    Task<LoginResponse?> LoginAsync(string email, string password);
    Task<RegisterResponse?> RegisterAsync(RegisterRequest request);
    void Logout();
    bool IsAuthenticated();
    string? GetUserEmail();
    string? GetUserRole();
    string? GetUserFirstName();
    string? GetUserLastName();
    Guid? GetUserId();
    bool IsAdmin();
}
