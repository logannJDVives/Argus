using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Argus.Sdk
{
    /// <summary>
    /// Minimal HTTP client for the Argus API
    /// </summary>
    public class ArgusApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private static readonly JsonSerializerOptions JsonOptions = new() 
        { 
            PropertyNameCaseInsensitive = true 
        };

        /// <summary>
        /// Initialize the Argus API client
        /// </summary>
        /// <param name="httpClient">HttpClient instance (inject via DI)</param>
        /// <param name="options">Configuration options including base URL</param>
        public ArgusApiClient(HttpClient httpClient, ArgusApiClientOptions options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            options = options ?? throw new ArgumentNullException(nameof(options));
            _baseUrl = options.BaseUrl;
        }

        /// <summary>
        /// Upload a ZIP file containing project source code
        /// </summary>
        /// <param name="name">Project name</param>
        /// <param name="zipFileBytes">ZIP file content</param>
        /// <param name="fileName">Original file name (used for metadata)</param>
        /// <returns>ProjectId (GUID) of the created project</returns>
        public async Task<Guid> UploadProjectAsync(string name, byte[] zipFileBytes, string fileName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name required", nameof(name));

            if (zipFileBytes == null || zipFileBytes.Length == 0)
                throw new ArgumentNullException(nameof(zipFileBytes));

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "project.zip";

            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(name), "projectName");

            var byteContent = new ByteArrayContent(zipFileBytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

            content.Add(byteContent, "file", fileName);

            using var response = await _httpClient.PostAsync("api/projects/upload", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"API error: {response.StatusCode}: {responseText}");

            var result = JsonSerializer.Deserialize<UploadResponse>(responseText, JsonOptions)
                ?? throw new InvalidOperationException("Invalid response from API");

            return result.ProjectId;
        }

        /// <summary>
        /// Response from upload endpoint (internal DTO)
        /// </summary>
        private record UploadResponse(Guid ProjectId, string Name, string Message, DateTime CreatedAt);

        /// <summary>
        /// Simple health check to validate API connectivity from MAUI
        /// </summary>
        public async Task<bool> CheckHealthAsync()
        {
            using var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
    }
}
