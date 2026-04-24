namespace VivesRental.Sdk.Stores;

public interface ITokenStore
{
    string? GetToken();
    void SetToken(string token);
    void Clear();
    Task InitializeAsync();
}
