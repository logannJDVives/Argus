using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Argus.Data;
using Argus.Dto.Components;
using Argus.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Argus.Services
{
    public class ComponentService : IComponentService
    {
        private readonly ArgusDbContext _context;

        public ComponentService(ArgusDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PaginatedComponentsDto> GetComponentsByScanAsync(
            Guid   scanId,
            string search   = null,
            int    page     = 1,
            int    pageSize = 25)
        {
            if (page < 1)     page     = 1;
            if (pageSize < 1) pageSize = 25;

            var scan = await _context.ScanRuns.FirstOrDefaultAsync(sr => sr.Id == scanId);
            if (scan == null)
                throw new KeyNotFoundException($"Scan with ID {scanId} not found.");

            var query = _context.SoftwareComponents
                .AsNoTracking()
                .Where(sc => sc.ScanRunId == scanId);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(sc => sc.Name.Contains(search) || sc.PackageUrl.Contains(search));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var items = await query
                .OrderBy(sc => sc.Name)
                .ThenBy(sc => sc.Version)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedComponentsDto
            {
                Items           = items.Select(MapToDto).ToList(),
                TotalCount      = totalCount,
                PageNumber      = page,
                PageSize        = pageSize,
                TotalPages      = totalPages,
                HasNextPage     = page < totalPages,
                HasPreviousPage = page > 1
            };
        }

        private static SoftwareComponentDto MapToDto(Entities.SoftwareComponent sc) => new()
        {
            Id                      = sc.Id,
            Name                    = sc.Name,
            Version                 = sc.Version,
            Type                    = sc.Type,
            PackageUrl              = sc.PackageUrl,
            License                 = sc.License,
            IsTransitive            = sc.IsTransitive,
            HasKnownVulnerabilities = sc.HasKnownVulnerabilities,
            Description             = sc.Description,
            Homepage                = sc.Homepage,
            PublisherUrl            = sc.PublisherUrl,
            PublishedDate           = sc.PublishedDate
        };
    }
}
