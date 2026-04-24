using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Argus.Dto.Projects;

namespace Argus.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, string userId);
        Task<ProjectDto?> GetProjectByIdAsync(Guid id);
        Task<List<ProjectDto>> GetAllProjectsAsync(string userId);
        Task<ProjectDto?> UpdateProjectAsync(Guid id, UpdateProjectDto dto, string userId);
        Task DeleteProjectAsync(Guid id, string userId);
    }
}
