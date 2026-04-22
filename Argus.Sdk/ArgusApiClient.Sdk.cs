using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Argus.Dto.Auth;
using Argus.Dto.Components;
using Argus.Dto.Projects;
using Argus.Dto.Scans;
using Argus.Dto.Secrets;

namespace Argus.Sdk
{
    public class ArgusApiClient
    {
        private readonly HttpClient _httpClient;

        public ArgusApiClient(HttpClient httpClient, ArgusApiClientOptions options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _ = options ?? throw new ArgumentNullException(nameof(options));
        }

        // ── Auth ────────────────────────────────────────────────────────────────

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", dto, ct);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<AuthResponseDto>(ct))!;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", dto, ct);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<AuthResponseDto>(ct))!;
        }

        // ── Health ──────────────────────────────────────────────────────────────

        public async Task<bool> CheckHealthAsync(CancellationToken ct = default)
        {
            using var response = await _httpClient.GetAsync("/health", ct);
            return response.IsSuccessStatusCode;
        }

        // ── Projects ────────────────────────────────────────────────────────────

        public async Task<List<ProjectDto>> GetProjectsAsync(CancellationToken ct = default)
        {
            var result = await _httpClient.GetFromJsonAsync<List<ProjectDto>>("api/projects", ct);
            return result ?? [];
        }

        public async Task<ProjectDto?> GetProjectAsync(Guid id, CancellationToken ct = default)
        {
            return await _httpClient.GetFromJsonAsync<ProjectDto>($"api/projects/{id}", ct);
        }

        public async Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectDto dto, CancellationToken ct = default)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/projects/{id}", dto, ct);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ProjectDto>(ct))!;
        }

        public async Task DeleteProjectAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _httpClient.DeleteAsync($"api/projects/{id}", ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task<UploadProjectResponseDto> UploadProjectAsync(
            string name,
            byte[] zipFileBytes,
            string fileName,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name required", nameof(name));
            if (zipFileBytes is null || zipFileBytes.Length == 0)
                throw new ArgumentNullException(nameof(zipFileBytes));
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "project.zip";

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(name), "projectName");
            var byteContent = new ByteArrayContent(zipFileBytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            content.Add(byteContent, "file", fileName);

            using var response = await _httpClient.PostAsync("api/projects/upload", content, ct);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<UploadProjectResponseDto>(ct))!;
        }

        // ── Scans ───────────────────────────────────────────────────────────────

        public async Task<ScanRunDto> StartScanAsync(Guid projectId, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsync($"api/projects/{projectId}/scans", null, ct);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ScanRunDto>(ct))!;
        }

        public async Task<List<ScanRunDto>> GetScansAsync(Guid projectId, CancellationToken ct = default)
        {
            var result = await _httpClient.GetFromJsonAsync<List<ScanRunDto>>($"api/projects/{projectId}/scans", ct);
            return result ?? [];
        }

        public async Task<ScanRunDto?> GetLatestScanAsync(Guid projectId, CancellationToken ct = default)
        {
            return await _httpClient.GetFromJsonAsync<ScanRunDto>($"api/projects/{projectId}/scans/latest", ct);
        }

        public async Task<ScanRunDto?> GetScanAsync(Guid projectId, Guid scanId, CancellationToken ct = default)
        {
            return await _httpClient.GetFromJsonAsync<ScanRunDto>($"api/projects/{projectId}/scans/{scanId}", ct);
        }

        // ── Secrets ─────────────────────────────────────────────────────────────

        public async Task<PaginatedSecretsDto> GetSecretsAsync(
            Guid scanId,
            string? severity = null,
            string? filePath = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            var url = $"api/scans/{scanId}/secrets?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(severity))
                url += $"&severity={Uri.EscapeDataString(severity)}";
            if (!string.IsNullOrWhiteSpace(filePath))
                url += $"&filePath={Uri.EscapeDataString(filePath)}";

            return (await _httpClient.GetFromJsonAsync<PaginatedSecretsDto>(url, ct))!;
        }

        public async Task ReviewSecretAsync(
            Guid id,
            bool isReviewed,
            bool isFalsePositive,
            CancellationToken ct = default)
        {
            var dto = new ReviewSecretDto { IsReviewed = isReviewed, IsFalsePositive = isFalsePositive };
            var response = await _httpClient.PatchAsJsonAsync($"api/secrets/{id}/review", dto, ct);
            response.EnsureSuccessStatusCode();
        }

        // ── Components (SBOM) ───────────────────────────────────────────────────

        public async Task<PaginatedComponentsDto> GetComponentsAsync(
            Guid   scanId,
            string? search   = null,
            int    page     = 1,
            int    pageSize = 25,
            CancellationToken ct = default)
        {
            var url = $"api/scans/{scanId}/components?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";

            return (await _httpClient.GetFromJsonAsync<PaginatedComponentsDto>(url, ct))!;
        }
    }
}
