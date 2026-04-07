using Argus.FrontEnd.Services;
using Argus.Sdk;
using Microsoft.Extensions.Logging;

namespace Argus.FrontEnd
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Register Argus SDK with HttpClientFactory
            builder.Services.AddSingleton(new ArgusApiClientOptions(ApiConfig.BaseUrl));
            builder.Services.AddHttpClient<ArgusApiClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<ArgusApiClientOptions>();
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromMinutes(5); // allow large uploads
            });

            builder.Services.AddSingleton<AuthStateService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
