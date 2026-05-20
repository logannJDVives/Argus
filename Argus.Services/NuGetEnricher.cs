using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Argus.Interfaces;
using Argus.Interfaces.Models;

namespace Argus.Services
{
    public class NuGetEnricher : INuGetEnricher
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private const string NuspecBaseUrl = "https://api.nuget.org/v3-flatcontainer";
        private const string RegistrationBaseUrl = "https://api.nuget.org/v3/registration5-gz-semver2";

        public NuGetEnricher(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<NuGetPackageMetadata> GetMetadataAsync(
            string            packageId,
            string            version,
            CancellationToken ct = default)
        {
            // Wildcard or empty versions can't be resolved to a specific nuspec.
            if (string.IsNullOrWhiteSpace(packageId) ||
                string.IsNullOrWhiteSpace(version)   ||
                version.Contains('*'))
                return new NuGetPackageMetadata();

            try
            {
                var id  = packageId.ToLowerInvariant();
                var ver = version.ToLowerInvariant();
                var url = $"{NuspecBaseUrl}/{id}/{ver}/{id}.nuspec";

                using var client   = _httpClientFactory.CreateClient("NuGet");
                using var response = await client.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                    return new NuGetPackageMetadata();

                var xml = await response.Content.ReadAsStringAsync(ct);
                return ParseNuspec(xml);
            }
            catch
            {
                // NuGet API unavailable or package not found – return empty metadata
                // so the scan can continue normally.
                return new NuGetPackageMetadata();
            }
        }

        public async Task<NuGetVersionInfo> GetLatestVersionInfoAsync(
            string            packageId,
            string            currentVersion,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                return new NuGetVersionInfo();

            try
            {
                var id = packageId.ToLowerInvariant();
                var registrationUrl = $"{RegistrationBaseUrl}/{id}/index.json";

                using var client = _httpClientFactory.CreateClient("NuGet");
                using var response = await client.GetAsync(registrationUrl, ct);

                if (!response.IsSuccessStatusCode)
                    return new NuGetVersionInfo();

                var json = await response.Content.ReadAsStringAsync(ct);
                return ParseRegistrationData(json, currentVersion);
            }
            catch
            {
                // NuGet API unavailable or package not found – return empty info
                return new NuGetVersionInfo();
            }
        }

        private static NuGetPackageMetadata ParseNuspec(string xml)
        {
            var doc  = XDocument.Parse(xml);
            var ns   = doc.Root?.Name.Namespace ?? XNamespace.None;
            var meta = doc.Root?.Element(ns + "metadata");

            if (meta is null)
                return new NuGetPackageMetadata();

            // License: prefer <license type="expression"> (SPDX), fall back to <licenseUrl>
            var licenseEl   = meta.Element(ns + "license");
            var licenseType = licenseEl?.Attribute("type")?.Value;
            var license     = licenseType == "file"
                ? string.Empty   // can't embed a file reference as a string
                : licenseEl?.Value
                  ?? meta.Element(ns + "licenseUrl")?.Value
                  ?? string.Empty;

            // Published date
            DateTime? published = null;
            var publishedStr = meta.Element(ns + "published")?.Value;
            if (!string.IsNullOrWhiteSpace(publishedStr) &&
                DateTime.TryParse(publishedStr, out var dt))
                published = dt;

            return new NuGetPackageMetadata
            {
                License       = license,
                Description   = meta.Element(ns + "description")?.Value  ?? string.Empty,
                Homepage      = meta.Element(ns + "projectUrl")?.Value   ?? string.Empty,
                Authors       = meta.Element(ns + "authors")?.Value      ?? string.Empty,
                PublishedDate = published
            };
        }

        private static NuGetVersionInfo ParseRegistrationData(string json, string currentVersion)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Get all catalog entries from items array
                if (!root.TryGetProperty("items", out var items))
                    return new NuGetVersionInfo { LatestVersion = currentVersion };

                string latestVersion = currentVersion;
                bool isDeprecated = false;
                string deprecationMessage = string.Empty;

                // Iterate through all pages
                foreach (var page in items.EnumerateArray())
                {
                    // First, check if this page has an 'upper' property which indicates the latest version in this page
                    if (page.TryGetProperty("upper", out var upperElement))
                    {
                        var upper = upperElement.GetString();
                        if (!string.IsNullOrEmpty(upper))
                        {
                            latestVersion = upper; // NuGet returns pages in order, so last upper is latest
                        }
                    }

                    // Check if items property exists in this page (inline catalog entries)
                    if (!page.TryGetProperty("items", out var catalogEntries))
                        continue;

                    // Iterate through catalog entries (individual versions)
                    foreach (var entry in catalogEntries.EnumerateArray())
                    {
                        if (!entry.TryGetProperty("catalogEntry", out var catalogEntry))
                            continue;

                        // Get version
                        if (catalogEntry.TryGetProperty("version", out var versionElement))
                        {
                            var version = versionElement.GetString();

                            // Track latest from inline entries
                            if (!string.IsNullOrEmpty(version))
                            {
                                // Check if this version is listed (not unlisted)
                                bool isListed = true;
                                if (catalogEntry.TryGetProperty("listed", out var listedElement))
                                {
                                    isListed = listedElement.GetBoolean();
                                }

                                if (isListed)
                                {
                                    latestVersion = version;
                                }
                            }

                            // Check if current version matches and is deprecated
                            if (version == currentVersion)
                            {
                                if (catalogEntry.TryGetProperty("deprecation", out var deprecation))
                                {
                                    isDeprecated = true;

                                    if (deprecation.TryGetProperty("message", out var messageElement))
                                    {
                                        deprecationMessage = messageElement.GetString() ?? string.Empty;
                                    }

                                    if (deprecation.TryGetProperty("reasons", out var reasons))
                                    {
                                        var reasonList = reasons.EnumerateArray()
                                            .Select(r => r.GetString())
                                            .Where(r => !string.IsNullOrEmpty(r))
                                            .ToList();

                                        if (reasonList.Any() && string.IsNullOrEmpty(deprecationMessage))
                                        {
                                            deprecationMessage = string.Join(", ", reasonList);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return new NuGetVersionInfo
                {
                    LatestVersion = latestVersion,
                    IsDeprecated = isDeprecated,
                    DeprecationMessage = deprecationMessage
                };
            }
            catch
            {
                return new NuGetVersionInfo { LatestVersion = currentVersion };
            }
        }
    }
}
