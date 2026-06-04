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
using Microsoft.Extensions.Logging;

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
        private readonly IOsvVulnerabilityService _osvService;
        private readonly ILogger<ScanService> _logger;

        public ScanService(
            ArgusDbContext context,
            IProjectFileScannerService scanner,
            IEnumerable<ISecretDetector> detectors,
            ICsprojParser csprojParser,
            HeuristicFilter filter,
            INuGetEnricher nuGetEnricher,
            IOsvVulnerabilityService osvService,
            ILogger<ScanService> logger)
        {
            _context       = context       ?? throw new ArgumentNullException(nameof(context));
            _scanner       = scanner       ?? throw new ArgumentNullException(nameof(scanner));
            _detectors     = detectors     ?? throw new ArgumentNullException(nameof(detectors));
            _csprojParser  = csprojParser  ?? throw new ArgumentNullException(nameof(csprojParser));
            _filter        = filter        ?? throw new ArgumentNullException(nameof(filter));
            _nuGetEnricher = nuGetEnricher ?? throw new ArgumentNullException(nameof(nuGetEnricher));
            _osvService    = osvService    ?? throw new ArgumentNullException(nameof(osvService));
            _logger        = logger        ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ScanRunDto> StartScanAsync(Guid projectId)
        {
            _logger.LogInformation("Starting scan for project {ProjectId}", projectId);

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                _logger.LogWarning("Project {ProjectId} not found", projectId);
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");
            }

            _logger.LogDebug("Found project {ProjectName} at path {ProjectPath}", project.Name, project.Path);

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

            _logger.LogInformation("Created scan run {ScanRunId} for project {ProjectId}", scanRun.Id, projectId);

            try
            {
                var files = await _scanner.ScanProjectAsync(project.Path);
                _logger.LogInformation("Discovered {FileCount} files to scan", files.Count);

                var detectedSecrets = new List<DetectedSecret>();
                var filesWithErrors = 0;

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
                            // Skip findings with empty/null values to prevent duplicate hash issues
                            if (string.IsNullOrWhiteSpace(finding.MatchedValue))
                                continue;

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
                    catch (Exception ex)
                    {
                        filesWithErrors++;
                        _logger.LogWarning(ex, "Failed to scan file {FilePath}: {ErrorMessage}", 
                            file.RelativePath, ex.Message);
                    }
                }

                if (filesWithErrors > 0)
                {
                    _logger.LogWarning("Encountered errors in {ErrorCount} files during secret scanning", filesWithErrors);
                }

                // Deduplicate by Hash only (one secret per unique value per scan)
                // This aligns with IX_DetectedSecrets_ScanRunId_Hash constraint
                var uniqueSecrets = detectedSecrets
                    .GroupBy(s => new { s.ScanRunId, s.Hash })
                    .Select(g => g.First())
                    .GroupBy(s => new { s.FilePath, s.LineNumber })
                    .Select(g => g.First())
                    .ToList();

                _logger.LogInformation("Found {SecretCount} unique secrets (filtered from {TotalCount} total findings)", 
                    uniqueSecrets.Count, detectedSecrets.Count);

                if (uniqueSecrets.Count > 0)
                    _context.DetectedSecrets.AddRange(uniqueSecrets);

                // ── Component parsing (.csproj) ─────────────────────────────
                var csprojFiles = files
                    .Where(f => f.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                _logger.LogInformation("Found {CsprojCount} .csproj files for component analysis", csprojFiles.Count);

                var components = new List<SoftwareComponent>();
                var csprojErrors = 0;

                foreach (var csproj in csprojFiles)
                {
                    try
                    {
                        var packages = await _csprojParser.ParseAsync(csproj.FullPath);
                        _logger.LogDebug("Parsed {PackageCount} packages from {CsprojFile}", 
                            packages.Count, csproj.RelativePath);

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
                    catch (Exception ex)
                    {
                        csprojErrors++;
                        _logger.LogWarning(ex, "Failed to parse .csproj file {CsprojPath}: {ErrorMessage}", 
                            csproj.RelativePath, ex.Message);
                    }
                }

                if (csprojErrors > 0)
                {
                    _logger.LogWarning("Failed to parse {ErrorCount} .csproj files", csprojErrors);
                }

                var uniqueComponents = components
                    .GroupBy(c => c.PackageUrl)
                    .Select(g => g.First())
                    .ToList();

                _logger.LogInformation("Identified {ComponentCount} unique components (from {TotalCount} total)", 
                    uniqueComponents.Count, components.Count);

                // ── NuGet metadata enrichment ───────────────────────────────
                // Fetch license, description, homepage and publish date from the
                // NuGet API for every unique component. Requests run concurrently
                // (max 10 at a time) so the scan stays fast even for large projects.
                _logger.LogInformation("Starting NuGet metadata enrichment for {ComponentCount} components", uniqueComponents.Count);

                var semaphore = new System.Threading.SemaphoreSlim(10);
                var enrichmentErrors = 0;
                var enrichTasks = uniqueComponents.Select(async component =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        // Fetch package metadata (license, description, etc.)
                        var meta = await _nuGetEnricher.GetMetadataAsync(component.Name, component.Version);
                        component.License       = meta.License;
                        component.Description   = meta.Description;
                        component.Homepage      = meta.Homepage;
                        component.PublisherUrl  = meta.Authors;
                        component.PublishedDate = meta.PublishedDate;

                        // Fetch version info (latest version and deprecated status)
                        var versionInfo = await _nuGetEnricher.GetLatestVersionInfoAsync(component.Name, component.Version);
                        component.LatestVersion = versionInfo.LatestVersion;
                        component.IsDeprecated  = versionInfo.IsDeprecated;

                        if (versionInfo.IsDeprecated)
                        {
                            _logger.LogWarning("Package {PackageName}@{PackageVersion} is DEPRECATED: {DeprecationMessage}",
                                component.Name, component.Version, versionInfo.DeprecationMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Threading.Interlocked.Increment(ref enrichmentErrors);
                        _logger.LogWarning(ex, "Failed to enrich metadata for {PackageName}@{PackageVersion}: {ErrorMessage}", 
                            component.Name, component.Version, ex.Message);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
                await Task.WhenAll(enrichTasks);

                if (enrichmentErrors > 0)
                {
                    _logger.LogWarning("Failed to enrich metadata for {ErrorCount} components", enrichmentErrors);
                }
                else
                {
                    _logger.LogInformation("Successfully enriched all {ComponentCount} components", uniqueComponents.Count);
                }

                // ── Vulnerability scanning ──────────────────────────────────
                // Check each component for known vulnerabilities using OSV.dev
                _logger.LogInformation("Starting vulnerability scanning for {ComponentCount} components", uniqueComponents.Count);

                var vulnSemaphore = new System.Threading.SemaphoreSlim(5); // Max 5 concurrent requests
                var vulnErrors = 0;
                var totalVulnerabilities = 0;
                var vulnTasks = uniqueComponents.Select(async component =>
                {
                    await vulnSemaphore.WaitAsync();
                    try
                    {
                        var vulnerabilities = await _osvService.CheckPackageAsync(component.Name, component.Version);

                        if (vulnerabilities.Count > 0)
                        {
                            component.HasKnownVulnerabilities = true;
                            System.Threading.Interlocked.Add(ref totalVulnerabilities, vulnerabilities.Count);

                            _logger.LogWarning("Found {VulnCount} vulnerabilities for {PackageName}@{PackageVersion}", 
                                vulnerabilities.Count, component.Name, component.Version);

                            // Create Vulnerability records
                            foreach (var vuln in vulnerabilities)
                            {
                                _context.Vulnerabilities.Add(new Vulnerability
                                {
                                    Id = Guid.NewGuid(),
                                    SoftwareComponentId = component.Id,
                                    CveId = vuln.Id,
                                    Description = vuln.Summary,
                                    Severity = Enum.TryParse<Severity>(vuln.Severity, true, out var sev) ? sev : Severity.Medium,
                                    CvssScore = string.Empty,
                                    CvssVector = string.Empty,
                                    PublishedDate = vuln.PublishedDate,
                                    ReferenceUrl = vuln.References?.FirstOrDefault() ?? string.Empty,
                                    Source = "OSV"
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Threading.Interlocked.Increment(ref vulnErrors);
                        _logger.LogWarning(ex, "Failed to check vulnerabilities for {PackageName}@{PackageVersion}: {ErrorMessage}", 
                            component.Name, component.Version, ex.Message);
                    }
                    finally
                    {
                        vulnSemaphore.Release();
                    }
                });
                await Task.WhenAll(vulnTasks);

                if (vulnErrors > 0)
                {
                    _logger.LogWarning("Failed to check vulnerabilities for {ErrorCount} components", vulnErrors);
                }

                _logger.LogInformation("Vulnerability scan complete: found {VulnCount} total vulnerabilities", totalVulnerabilities);

                if (uniqueComponents.Count > 0)
                    _context.SoftwareComponents.AddRange(uniqueComponents);

                scanRun.FilesScanned  = files.Count;
                scanRun.SecretCount   = uniqueSecrets.Count;
                scanRun.ComponentCount = uniqueComponents.Count;
                scanRun.Status       = ScanStatus.Completed;
                scanRun.CompletedAt  = DateTime.UtcNow;
                scanRun.Duration     = scanRun.CompletedAt - scanRun.CreatedAt;

                _logger.LogInformation(
                    "Scan {ScanRunId} completed successfully: {FileCount} files, {SecretCount} secrets, {ComponentCount} components in {Duration}ms",
                    scanRun.Id, files.Count, uniqueSecrets.Count, uniqueComponents.Count, scanRun.Duration?.TotalMilliseconds ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scan {ScanRunId} failed: {ErrorMessage}", scanRun.Id, ex.Message);

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
