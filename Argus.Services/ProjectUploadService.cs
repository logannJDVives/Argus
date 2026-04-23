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
        /// <summary>Maximum accepted compressed upload size: 100 MB.</summary>
        private const long MaxFileSizeBytes = 100L * 1024 * 1024;

        /// <summary>Maximum total uncompressed size of all ZIP entries: 500 MB.</summary>
        private const long MaxUncompressedBytes = 500L * 1024 * 1024;

        /// <summary>
        /// Maximum ratio of uncompressed / compressed size.
        /// Legitimate archives rarely exceed 10×; malicious zip bombs can be millions×.
        /// </summary>
        private const double MaxCompressionRatio = 10.0;

        // Every valid ZIP file starts with the local-file-header signature PK\x03\x04.
        private static readonly byte[] ZipMagicBytes = [0x50, 0x4B, 0x03, 0x04];

        private readonly IProjectService _projectService;
        private readonly IHostEnvironment _env;

        public ProjectUploadService(IProjectService projectService, IHostEnvironment env)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _env            = env            ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task<UploadProjectResponseDto> UploadAndCreateProjectAsync(IFormFile zipFile, string userId)
        {
            // ── Basic validation ─────────────────────────────────────────────
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

            // ── Magic bytes check (real ZIP signature) ───────────────────────
            await ValidateZipSignatureAsync(zipFile);

            // ── Write to temp file ───────────────────────────────────────────
            // Use a random name so concurrent uploads never collide in temp storage.
            var tempZipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.zip");
            try
            {
                await using (var stream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    await zipFile.CopyToAsync(stream);

                // ── Zip bomb check ───────────────────────────────────────────
                ValidateZipContents(tempZipPath, zipFile.Length);

                // ── Extract ──────────────────────────────────────────────────
                var projectName  = Path.GetFileNameWithoutExtension(zipFile.FileName);
                var projectsRoot = Path.Combine(_env.ContentRootPath, "Projects");
                var extractPath  = Path.Combine(projectsRoot, projectName);
                Directory.CreateDirectory(extractPath);

                ZipFile.ExtractToDirectory(tempZipPath, extractPath, overwriteFiles: true);

                // ── Persist ──────────────────────────────────────────────────
                var createProjectDto = new CreateProjectDto { Name = projectName, Path = extractPath };
                var projectDto       = await _projectService.CreateProjectAsync(createProjectDto, userId);

                return new UploadProjectResponseDto
                {
                    ProjectId = projectDto.Id,
                    Name      = projectDto.Name,
                    Message   = "Project created successfully",
                    CreatedAt = projectDto.CreatedAt
                };
            }
            finally
            {
                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);
            }
        }

        /// <summary>
        /// Reads the first 4 bytes of the upload stream and verifies the ZIP magic bytes (PK\x03\x04).
        /// Resets the stream position afterwards so the file can still be copied to disk.
        /// </summary>
        private static async Task ValidateZipSignatureAsync(IFormFile zipFile)
        {
            var header = new byte[4];

            await using var stream = zipFile.OpenReadStream();
            var bytesRead = await stream.ReadAsync(header);

            if (bytesRead < 4 || !header.AsSpan().SequenceEqual(ZipMagicBytes))
                throw new UploadValidationException(
                    "The file does not appear to be a valid ZIP archive (invalid file signature).");
        }

        /// <summary>
        /// Iterates over every entry in the ZIP without extracting it and checks that the
        /// total uncompressed size stays within safe bounds, protecting against zip bombs.
        /// </summary>
        private static void ValidateZipContents(string zipPath, long compressedSize)
        {
            using var archive = ZipFile.OpenRead(zipPath);

            long totalUncompressed = 0;

            foreach (var entry in archive.Entries)
            {
                totalUncompressed += entry.Length; // Length = uncompressed size

                if (totalUncompressed > MaxUncompressedBytes)
                    throw new UploadValidationException(
                        $"The ZIP archive exceeds the maximum allowed uncompressed size of {MaxUncompressedBytes / (1024 * 1024)} MB.");

                if (compressedSize > 0 && (double)totalUncompressed / compressedSize > MaxCompressionRatio)
                    throw new UploadValidationException(
                        $"The ZIP archive has a suspicious compression ratio and was rejected for security reasons.");
            }
        }
    }
}
