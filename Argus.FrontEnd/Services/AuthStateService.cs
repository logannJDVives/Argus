using Argus.Dto.Auth;
using Argus.Sdk;

namespace Argus.FrontEnd.Services
{
    /// <summary>
    /// Singleton service that manages the JWT token for the current session.
    /// Persists the token via Preferences so the user stays logged in between app restarts.
    /// </summary>
    public class AuthStateService
    {
        private readonly ArgusApiClient _api;

        private const string TokenKey = "auth_token";
        private const string EmailKey = "auth_email";

        public string? Token    { get; private set; }
        public string? Email    { get; private set; }
        public bool IsAuthenticated => Token is not null;

        public event Action? AuthStateChanged;

        public AuthStateService(ArgusApiClient api)
        {
            _api = api;
            TryLoadPersistedSession();
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _api.LoginAsync(new LoginDto { Email = email, Password = password });
                ApplyToken(response);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string email, string password)
        {
            try
            {
                var response = await _api.RegisterAsync(new RegisterDto { Email = email, Password = password });
                ApplyToken(response);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Logout()
        {
            Token = null;
            Email = null;
            _api.ClearAuthToken();
            Preferences.Default.Remove(TokenKey);
            Preferences.Default.Remove(EmailKey);
            AuthStateChanged?.Invoke();
        }

        private void ApplyToken(AuthResponseDto response)
        {
            Token = response.Token;
            Email = response.Email;
            _api.SetAuthToken(Token);
            Preferences.Default.Set(TokenKey, Token);
            Preferences.Default.Set(EmailKey, Email);
            AuthStateChanged?.Invoke();
        }

        private void TryLoadPersistedSession()
        {
            var savedToken = Preferences.Default.Get(TokenKey, (string?)null);
            var savedEmail = Preferences.Default.Get(EmailKey, (string?)null);

            if (!string.IsNullOrEmpty(savedToken))
            {
                Token = savedToken;
                Email = savedEmail;
                _api.SetAuthToken(savedToken);
            }
        }
    }
}
