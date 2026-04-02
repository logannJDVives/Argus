using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Argus.Dto.Scans;
using Argus.Data;
using Argus.Entities;
using Argus.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Argus.Services
{
    public class ScanService : IScanService
    {
        private readonly ArgusDbContext _context;
        private readonly IProjectFileScannerService _scanner;

        public ScanService(ArgusDbContext context, IProjectFileScannerService scanner)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
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

                scanRun.FilesScanned = files.Count;
                scanRun.Status = ScanStatus.Completed;
                scanRun.CompletedAt = DateTime.UtcNow;
                scanRun.Duration = scanRun.CompletedAt - scanRun.CreatedAt;
            }
            catch (Exception ex)
            {
                scanRun.Status = ScanStatus.Failed;
                scanRun.ErrorMessage = ex.Message;
                scanRun.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
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
