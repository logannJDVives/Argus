using System;

namespace Argus.Sdk
{
    /// <summary>
    /// Configuration options for ArgusApiClient
    /// </summary>
    public class ArgusApiClientOptions
    {
        /// <summary>
        /// Base URL of the Argus API (e.g., "https://localhost:7001")
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Initialize options with base URL
        /// </summary>
        public ArgusApiClientOptions(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl), "BaseUrl cannot be empty.");

            BaseUrl = baseUrl.TrimEnd('/');
        }
    }
}
