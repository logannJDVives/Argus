using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Argus.Dto.Projects;
using Argus.Data;
using Argus.Entities;
using Argus.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Argus.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ArgusDbContext _context;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(ArgusDbContext context, ILogger<ProjectService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, string userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            _logger.LogInformation("Creating new project {ProjectName} for user {UserId}", dto.Name, userId);

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Path = dto.Path,
                CreatedAt = DateTime.UtcNow,
                LastScanDate = null,
                UserId = userId
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created project {ProjectId} with name {ProjectName}", project.Id, project.Name);

            return MapToDto(project);
        }

        public async Task<ProjectDto> GetProjectByIdAsync(Guid id)
        {
            _logger.LogDebug("Fetching project {ProjectId}", id);

            var project = await _context.Projects
                .AsNoTracking()
                .Include(p => p.ScanRuns)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                _logger.LogWarning("Project {ProjectId} not found", id);
                return null;
            }

            return MapToDto(project);
        }

        public async Task<List<ProjectDto>> GetAllProjectsAsync(string userId)
        {
            _logger.LogDebug("Fetching all projects for user {UserId}", userId);

            var projects = await _context.Projects
                .AsNoTracking()
                .Include(p => p.ScanRuns)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("Retrieved {ProjectCount} projects for user {UserId}", projects.Count, userId);

            return projects.Select(MapToDto).ToList();
        }

        public async Task DeleteProjectAsync(Guid id, string userId)
        {
            _logger.LogInformation("Attempting to delete project {ProjectId} for user {UserId}", id, userId);

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
            {
                _logger.LogWarning("Cannot delete project {ProjectId} - not found or unauthorized", id);
                throw new KeyNotFoundException($"Project with ID {id} not found.");
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted project {ProjectId} ({ProjectName})", id, project.Name);
        }

        public async Task<ProjectDto?> UpdateProjectAsync(Guid id, UpdateProjectDto dto, string userId)
        {
            _logger.LogInformation("Updating project {ProjectId} for user {UserId}", id, userId);

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
            {
                _logger.LogWarning("Cannot update project {ProjectId} - not found or unauthorized", id);
                return null;
            }

            var oldName = project.Name;
            project.Name = dto.Name;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated project {ProjectId}: renamed from {OldName} to {NewName}", id, oldName, dto.Name);

            return MapToDto(project);
        }

        private static ProjectDto MapToDto(Project project)
        {
            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Path = project.Path,
                CreatedAt = project.CreatedAt,
                LastScanDate = project.LastScanDate,
                ScanRunCount = project.ScanRuns?.Count ?? 0
            };
        }
    }
}
