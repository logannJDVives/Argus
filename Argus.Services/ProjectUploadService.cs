using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Argus.Dto.Projects;
using Argus.Interfaces;
using Argus.Services.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Argus.Services
{
    public class ProjectUploadService : IProjectUploadService
    {
        /// <summary>Maximum accepted upload size: 50 MB.</summary>
        private const long MaxFileSizeBytes = 50L * 1024 * 1024;

        private readonly IProjectService _projectService;
        private readonly IHostEnvironment _env;

        public ProjectUploadService(IProjectService projectService, IHostEnvironment env)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _env            = env            ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task<UploadProjectResponseDto> UploadAndCreateProjectAsync(IFormFile zipFile, string userId)
        {
            // ── Validation ───────────────────────────────────────────────────
            if (zipFile is null)
                throw new ArgumentNullException(nameof(zipFile));

            if (zipFile.Length == 0)
                throw new UploadValidationException("The uploaded file is empty.");

            if (zipFile.Length > MaxFileSizeBytes)
                throw new UploadValidationException(
                    $"File size ({zipFile.Length / (1024 * 1024.0):F1} MB) exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

            if (string.IsNullOrWhiteSpace(zipFile.FileName))
                throw new UploadValidationException("The uploaded file has no name.");

            if (!zipFile.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                throw new UploadValidationException("Only ZIP archives (.zip) are accepted.");

            // ── Extract ──────────────────────────────────────────────────────
            var projectName  = Path.GetFileNameWithoutExtension(zipFile.FileName);
            var projectsRoot = Path.Combine(_env.ContentRootPath, "Projects");
            var extractPath  = Path.Combine(projectsRoot, projectName);
            Directory.CreateDirectory(extractPath);

            // Use a random name so concurrent uploads never collide in temp storage
            var tempZipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.zip");
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
