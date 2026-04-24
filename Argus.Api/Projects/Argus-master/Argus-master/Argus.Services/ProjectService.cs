using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Argus.Dto.Projects;
using Argus.Data;
using Argus.Entities;
using Argus.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Argus.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ArgusDbContext _context;

        public ProjectService(ArgusDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, string userId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

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

            return MapToDto(project);
        }

        public async Task<ProjectDto> GetProjectByIdAsync(Guid id)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Include(p => p.ScanRuns)
                .FirstOrDefaultAsync(p => p.Id == id);

            return project == null ? null : MapToDto(project);
        }

        public async Task<List<ProjectDto>> GetAllProjectsAsync(string userId)
        {
            var projects = await _context.Projects
                .AsNoTracking()
                .Include(p => p.ScanRuns)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return projects.Select(MapToDto).ToList();
        }

        public async Task DeleteProjectAsync(Guid id, string userId)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }

        public async Task<ProjectDto?> UpdateProjectAsync(Guid id, UpdateProjectDto dto, string userId)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
                return null;

            project.Name = dto.Name;
            await _context.SaveChangesAsync();

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
