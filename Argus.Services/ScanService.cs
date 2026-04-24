using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Argus.Data;
using Argus.Dto.Scans;
using Argus.Entities;
using Argus.Interfaces;
using Argus.Interfaces.Models;
using Argus.Services.Detection;
using Microsoft.EntityFrameworkCore;

namespace Argus.Services
{
    public class ScanService : IScanService
    {
        private readonly ArgusDbContext _context;
        private readonly IProjectFileScannerService _scanner;
        private readonly IEnumerable<ISecretDetector> _detectors;
        private readonly ICsprojParser _csprojParser;
        private readonly HeuristicFilter _filter;
        private readonly INuGetEnricher _nuGetEnricher;

        public ScanService(
            ArgusDbContext context,
            IProjectFileScannerService scanner,
            IEnumerable<ISecretDetector> detectors,
            ICsprojParser csprojParser,
            HeuristicFilter filter,
            INuGetEnricher nuGetEnricher)
        {
            _context       = context       ?? throw new ArgumentNullException(nameof(context));
            _scanner       = scanner       ?? throw new ArgumentNullException(nameof(scanner));
            _detectors     = detectors     ?? throw new ArgumentNullException(nameof(detectors));
            _csprojParser  = csprojParser  ?? throw new ArgumentNullException(nameof(csprojParser));
            _filter        = filter        ?? throw new ArgumentNullException(nameof(filter));
            _nuGetEnricher = nuGetEnricher ?? throw new ArgumentNullException(nameof(nuGetEnricher));
        }

        public async Task<ScanRunDto> StartScanAsync(Guid projectId)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");

            var scanRun = new ScanRun
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                CreatedAt = DateTime.UtcNow,
                Status = ScanStatus.InProgress,
                SecretCount = 0,
                ComponentCount = 0,
                ErrorMessage = null,
                Duration = null,
                FilesScanned = null
            };

            _context.ScanRuns.Add(scanRun);
            await _context.SaveChangesAsync();

            try
            {
                var files = await _scanner.ScanProjectAsync(project.Path);

                var detectedSecrets = new List<DetectedSecret>();

                foreach (var file in files)
                {
                    try
                    {
                        var allFindings = new List<SecretFinding>();

                        foreach (var detector in _detectors)
                        {
                            var findings = await detector.DetectAsync(file);
                            allFindings.AddRange(findings);
                        }

                        var filtered = _filter.Filter(allFindings);

                        foreach (var finding in filtered)
                        {
                            detectedSecrets.Add(new DetectedSecret
                            {
                                Id              = Guid.NewGuid(),
                                ScanRunId       = scanRun.Id,
                                Type            = finding.DetectorType.ToString(),
                                FilePath        = finding.FilePath,
                                LineNumber      = finding.LineNumber,
                                MaskedValue     = SecretMasker.MaskValue(finding.MatchedValue),
                                Hash            = SecretMasker.HashValue(finding.MatchedValue),
                                RuleId          = finding.RuleId,
                                Severity        = finding.Severity,
                                Confidence      = finding.Confidence,
                                IsFalsePositive = false,
                                IsReviewed      = false
                            });
                        }
                    }
                    catch
                    {
                        // Skip files that cause unexpected errors so one bad file
                        // never aborts the entire scan run.
                    }
                }

                var uniqueSecrets = detectedSecrets
                    .GroupBy(s => s.Hash)
                    .Select(g => g.First())
                    .GroupBy(s => new { s.FilePath, s.LineNumber })
                    .Select(g => g.First())
                    .ToList();

                if (uniqueSecrets.Count > 0)
                    _context.DetectedSecrets.AddRange(uniqueSecrets);

                // ── Component parsing (.csproj) ─────────────────────────────
                var csprojFiles = files
                    .Where(f => f.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var components = new List<SoftwareComponent>();

                foreach (var csproj in csprojFiles)
                {
                    try
                    {
                        var packages = await _csprojParser.ParseAsync(csproj.FullPath);

                        foreach (var pkg in packages)
                        {
                            components.Add(new SoftwareComponent
                            {
                                Id           = Guid.NewGuid(),
                                ScanRunId    = scanRun.Id,
                                Name         = pkg.Name,
                                Version      = pkg.Version,
                                Type         = "NuGet",
                                IsTransitive = pkg.IsTransitive,
                                License      = string.Empty,
                                PackageUrl   = $"pkg:nuget/{pkg.Name}@{pkg.Version}",
                                Description  = string.Empty,
                                Homepage     = string.Empty,
                                PublisherUrl = string.Empty
                            });
                        }
                    }
                    catch
                    {
                        // Skip csproj files that fail to parse so one bad file
                        // never aborts the entire component scan.
                    }
                }

                var uniqueComponents = components
                    .GroupBy(c => c.PackageUrl)
                    .Select(g => g.First())
                    .ToList();

                // ── NuGet metadata enrichment ───────────────────────────────
                // Fetch license, description, homepage and publish date from the
                // NuGet API for every unique component. Requests run concurrently
                // (max 10 at a time) so the scan stays fast even for large projects.
                var semaphore = new System.Threading.SemaphoreSlim(10);
                var enrichTasks = uniqueComponents.Select(async component =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var meta = await _nuGetEnricher.GetMetadataAsync(component.Name, component.Version);
                        component.License       = meta.License;
                        component.Description   = meta.Description;
                        component.Homepage      = meta.Homepage;
                        component.PublisherUrl  = meta.Authors;
                        component.PublishedDate = meta.PublishedDate;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
                await Task.WhenAll(enrichTasks);

                if (uniqueComponents.Count > 0)
                    _context.SoftwareComponents.AddRange(uniqueComponents);

                scanRun.FilesScanned  = files.Count;
                scanRun.SecretCount   = uniqueSecrets.Count;
                scanRun.ComponentCount = uniqueComponents.Count;
                scanRun.Status       = ScanStatus.Completed;
                scanRun.CompletedAt  = DateTime.UtcNow;
                scanRun.Duration     = scanRun.CompletedAt - scanRun.CreatedAt;
            }
            catch (Exception ex)
            {
                scanRun.Status = ScanStatus.Failed;
                scanRun.ErrorMessage = ex.Message;
                scanRun.CompletedAt = DateTime.UtcNow;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                scanRun.Status       = ScanStatus.Failed;
                scanRun.ErrorMessage = ex.Message;
                scanRun.CompletedAt  = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return MapToDto(scanRun);
        }

        public async Task<List<ScanRunDto>> GetScansByProjectAsync(Guid projectId)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");

            var scans = await _context.ScanRuns
                .AsNoTracking()
                .Where(sr => sr.ProjectId == projectId)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();

            return scans.Select(MapToDto).ToList();
        }

        public async Task<ScanRunDto> GetScanByIdAsync(Guid scanId)
        {
            var scan = await _context.ScanRuns
                .AsNoTracking()
                .FirstOrDefaultAsync(sr => sr.Id == scanId);

            if (scan == null)
                throw new KeyNotFoundException($"Scan with ID {scanId} not found.");

            return MapToDto(scan);
        }

        public async Task<ScanRunDto> GetLatestScanAsync(Guid projectId)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");

            var latestScan = await _context.ScanRuns
                .AsNoTracking()
                .Where(sr => sr.ProjectId == projectId)
                .OrderByDescending(sr => sr.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestScan == null)
                throw new KeyNotFoundException($"No scans found for project with ID {projectId}.");

            return MapToDto(latestScan);
        }

        private static ScanRunDto MapToDto(ScanRun scanRun)
        {
            return new ScanRunDto
            {
                Id = scanRun.Id,
                ProjectId = scanRun.ProjectId,
                CreatedAt = scanRun.CreatedAt,
                CompletedAt = scanRun.CompletedAt,
                Status = scanRun.Status.ToString(),
                SecretCount = scanRun.SecretCount,
                ComponentCount = scanRun.ComponentCount,
                ErrorMessage = scanRun.ErrorMessage,
                Duration = scanRun.Duration,
                FilesScanned = scanRun.FilesScanned
            };
        }
    }
}
