using System.Net.Http.Headers;

namespace Argus.FrontEnd.Services
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly AuthStateService _authState;

        public AuthorizationMessageHandler(AuthStateService authState)
        {
            _authState = authState;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_authState.Token is not null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.Token);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
