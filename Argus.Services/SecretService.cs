using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Argus.Dto.Secrets;
using Argus.Data;
using Argus.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Argus.Services
{
    public class SecretService : ISecretService
    {
        private readonly ArgusDbContext _context;

        public SecretService(ArgusDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PaginatedSecretsDto> GetSecretsByScanAsync(Guid scanId, string severity = null, string filePath = null, int page = 1, int pageSize = 10)
        {
            if (page < 1)
                page = 1;
            if (pageSize < 1)
                pageSize = 10;

            var scan = await _context.ScanRuns.FirstOrDefaultAsync(sr => sr.Id == scanId);
            if (scan == null)
                throw new KeyNotFoundException($"Scan with ID {scanId} not found.");

            var query = _context.DetectedSecrets
                .AsNoTracking()
                .Where(ds => ds.ScanRunId == scanId);

            if (!string.IsNullOrWhiteSpace(severity))
            {
                query = query.Where(ds => ds.Severity.ToString() == severity);
            }

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                query = query.Where(ds => ds.FilePath.Contains(filePath));
            }

            var totalCount = await query.CountAsync();

            var secrets = await query
                .OrderByDescending(ds => ds.Severity)
                .ThenByDescending(ds => ds.LineNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var hasNextPage = page < totalPages;
            var hasPreviousPage = page > 1;

            return new PaginatedSecretsDto
            {
                Items = secrets.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage
            };
        }

        public async Task ReviewSecretAsync(Guid id, bool isReviewed, bool isFalsePositive)
        {
            var secret = await _context.DetectedSecrets.FirstOrDefaultAsync(ds => ds.Id == id);

            if (secret == null)
                throw new KeyNotFoundException($"Secret with ID {id} not found.");

            secret.IsReviewed = isReviewed;
            secret.IsFalsePositive = isFalsePositive;

            _context.DetectedSecrets.Update(secret);
            await _context.SaveChangesAsync();
        }

        private static DetectedSecretDto MapToDto(Entities.DetectedSecret secret)
        {
            return new DetectedSecretDto
            {
                Id = secret.Id,
                Type = secret.Type,
                FilePath = secret.FilePath,
                LineNumber = secret.LineNumber,
                MaskedValue = secret.MaskedValue,
                Severity = secret.Severity.ToString(),
                RuleId = secret.RuleId,
                Confidence = secret.Confidence.ToString(),
                IsFalsePositive = secret.IsFalsePositive,
                IsReviewed = secret.IsReviewed
            };
        }
    }
}
