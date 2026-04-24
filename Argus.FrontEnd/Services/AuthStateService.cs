using Argus.Dto.Auth;

namespace Argus.FrontEnd.Services
{
    /// <summary>
    /// Singleton service that manages the JWT token for the current session.
    /// Persists the token via Preferences so the user stays logged in between app restarts.
    /// </summary>
    public class AuthStateService
    {
        private const string TokenKey = "auth_token";
        private const string EmailKey = "auth_email";

        public string? Token    { get; private set; }
        public string? Email    { get; private set; }
        public bool IsAuthenticated => Token is not null;

        public event Action? AuthStateChanged;

        public AuthStateService()
        {
            TryLoadPersistedSession();
        }

        public void SetSession(AuthResponseDto response)
        {
            Token = response.Token;
            Email = response.Email;
            Preferences.Default.Set(TokenKey, Token);
            Preferences.Default.Set(EmailKey, Email);
            AuthStateChanged?.Invoke();
        }
        //sk-9fA7xQ2LmZ8vP1rT6YcK4dNwB3uH5jE0sGqR7XyUoCzVhM1LpI
        public void Logout()
        {
            Token = null;
            Email = null;
            Preferences.Default.Remove(TokenKey);
            Preferences.Default.Remove(EmailKey);
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
            }
        }
    }
}

