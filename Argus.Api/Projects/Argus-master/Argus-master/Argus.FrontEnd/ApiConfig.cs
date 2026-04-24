namespace Argus.FrontEnd
{
    /// <summary>
    /// API configuration constants for Argus SDK
    /// </summary>
    public static class ApiConfig
    {
        /// <summary>
        /// Base URL for the Argus API
        /// Configure this based on your environment:
        /// - WinUI local: https://127.0.0.1:7187 (ensure dev cert is trusted in cert store)
        /// - Android emulator: http://10.0.2.2:7187 or https://10.0.2.2:7187 (trust cert if HTTPS)
        /// - iOS simulator: http://localhost:7187 or use machine LAN IP
        /// - Physical device: use machine LAN IP (e.g., https://192.168.x.x:7187)
        /// </summary>
#if DEBUG
        // For local development on WinUI: use 127.0.0.1 instead of localhost
        // For Android emulator: use 10.0.2.2
        // For physical device: use your machine LAN IP
        public const string BaseUrl = "https://127.0.0.1:7187";
#else
        public const string BaseUrl = "https://api.argus.local"; // production endpoint
#endif
    }
}
