using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Argus.Dto.Projects;

namespace Argus.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto);
        Task<ProjectDto> GetProjectByIdAsync(Guid id);
        Task<List<ProjectDto>> GetAllProjectsAsync();
        Task DeleteProjectAsync(Guid id);
    }
}
