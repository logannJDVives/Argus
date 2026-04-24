using System;
using System.Net.Http;
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
    }
}
