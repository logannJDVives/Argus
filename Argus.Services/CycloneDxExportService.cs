using Argus.Data;
using Argus.Dto.Sbom;
using Argus.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Argus.Services
{
    public class CycloneDxExportService : ICycloneDxExportService
    {
        private readonly ArgusDbContext _context;

        public CycloneDxExportService(ArgusDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<string> GenerateCycloneDxJsonAsync(Guid scanId)
        {
            var scan = await _context.ScanRuns
                .AsNoTracking()
                .FirstOrDefaultAsync(sr => sr.Id == scanId)
                ?? throw new KeyNotFoundException($"Scan with ID {scanId} not found.");

            var components = await _context.SoftwareComponents
                .AsNoTracking()
                .Where(sc => sc.ScanRunId == scanId)
                .ToListAsync();

            var bom = new CycloneDxDto
            {
                SerialNumber = $"urn:uuid:{Guid.NewGuid()}",
                Metadata = new CycloneDxMetadataDto
                {
                    Timestamp = scan.CreatedAt.ToString("o")
                },
                Components = components.Select(sc => new CycloneDxComponentDto
                {
                    Type = sc.Type ?? "library",
                    Name = sc.Name,
                    Version = sc.Version,
                    Purl = sc.PackageUrl,
                    Description = sc.Description,
                    Licenses = string.IsNullOrWhiteSpace(sc.License) ? null :
                    [
                        new() { License = new CycloneDxLicenseDto { Id = sc.License } }
                    ],
                    ExternalReferences = BuildExternalRefs(sc.Homepage, sc.PublisherUrl)
                }).ToList()
            };

            return JsonSerializer.Serialize(bom, new JsonSerializerOptions { WriteIndented = true });
        }

        private static List<CycloneDxExternalReferenceDto>? BuildExternalRefs(string? homepage, string? publisherUrl)
        {
            var refs = new List<CycloneDxExternalReferenceDto>();

            if (!string.IsNullOrWhiteSpace(homepage))
                refs.Add(new() { Type = "website", Url = homepage });

            if (!string.IsNullOrWhiteSpace(publisherUrl))
                refs.Add(new() { Type = "distribution", Url = publisherUrl });

            return refs.Count > 0 ? refs : null;
        }
    }
}
