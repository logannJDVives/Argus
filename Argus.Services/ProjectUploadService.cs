using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Argus.Dto.Projects;
using Argus.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Argus.Services
{
    public class ProjectUploadService : IProjectUploadService
    {
        private readonly IProjectService _projectService;
        private readonly IHostEnvironment _env;

        public ProjectUploadService(IProjectService projectService, IHostEnvironment env)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task<UploadProjectResponseDto> UploadAndCreateProjectAsync(IFormFile zipFile, string userId)
        {
            if (zipFile == null)
                throw new ArgumentNullException(nameof(zipFile));

            // Extract project name from file name (remove .zip extension)
            var projectName = System.IO.Path.GetFileNameWithoutExtension(zipFile.FileName);

            // Extract ZIP to a dedicated folder under the content root
            var projectsRoot = Path.Combine(_env.ContentRootPath, "Projects");
            var extractPath = Path.Combine(projectsRoot, projectName);
            Directory.CreateDirectory(extractPath);

            var tempZipPath = Path.Combine(Path.GetTempPath(), zipFile.FileName);
            try
            {
                await using (var stream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    await zipFile.CopyToAsync(stream);

                ZipFile.ExtractToDirectory(tempZipPath, extractPath, overwriteFiles: true);
            }
            finally
            {
                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);
            }

            // Path is now set — no more NULL on insert
            var createProjectDto = new CreateProjectDto { Name = projectName, Path = extractPath };
            var projectDto = await _projectService.CreateProjectAsync(createProjectDto, userId);

            // Return response
            return new UploadProjectResponseDto
            {
                ProjectId = projectDto.Id,
                Name = projectDto.Name,
                Message = "Project created successfully",
                CreatedAt = projectDto.CreatedAt
            };
        }
    }
}
