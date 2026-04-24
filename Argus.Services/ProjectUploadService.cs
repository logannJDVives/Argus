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
        /// <summary>Maximum accepted compressed upload size: 200 MB.</summary>
        private const long MaxFileSizeBytes = 200L * 1024 * 1024;

        /// <summary>Streaming buffer: 80 KB — large enough for throughput, small enough for memory.</summary>
        private const int CopyBufferSize = 81_920;

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
            var tempZipPath  = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.zip");
            var projectName  = Path.GetFileNameWithoutExtension(zipFile.FileName);
            var projectsRoot = Path.Combine(_env.ContentRootPath, "Projects");
            var extractPath  = Path.Combine(projectsRoot, projectName);

            try
            {
                await using (var stream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    await zipFile.CopyToAsync(stream);

                // ── Secure streaming extraction ───────────────────────────────
                // Bytes are counted as they are ACTUALLY decompressed — not from
                // the ZIP headers, which a zip bomb can forge to appear small.
                Directory.CreateDirectory(extractPath);
                await ExtractAsync(tempZipPath, extractPath);

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
            catch
            {
                // Clean up any partially extracted files so disk space is not leaked.
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, recursive: true);

                throw;
            }
            finally
            {
                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);
            }
        }

        /// <summary>
        /// Reads the first 4 bytes of the upload stream and verifies the ZIP magic bytes (PK\x03\x04).
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
        /// Extracts the ZIP archive entry by entry using a streaming copy.
        /// Validates paths to prevent zip-slip attacks (../../etc/passwd).
        /// </summary>
        private static async Task ExtractAsync(string zipPath, string extractPath)
        {
            using var archive = ZipFile.OpenRead(zipPath);

            var    buffer   = new byte[CopyBufferSize];
            string rootPath = Path.GetFullPath(extractPath);

            foreach (var entry in archive.Entries)
            {
                // Path traversal (zip slip) guard — resolves any ".." sequences and verifies
                // the result still lives inside the intended extraction root.
                var destinationPath = Path.GetFullPath(Path.Combine(rootPath, entry.FullName));

                if (!destinationPath.StartsWith(rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                    && !destinationPath.Equals(rootPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UploadValidationException(
                        "The ZIP archive contains an entry with an invalid path and was rejected for security reasons.");
                }

                // Directory entries — just create the folder, nothing to decompress.
                if (entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\'))
                {
                    Directory.CreateDirectory(destinationPath);
                    continue;
                }

                // Ensure the parent directory exists before writing.
                var parentDir = Path.GetDirectoryName(destinationPath)!;
                Directory.CreateDirectory(parentDir);

                await using var entryStream = entry.Open();
                await using var outStream   = new FileStream(
                    destinationPath, FileMode.Create, FileAccess.Write,
                    FileShare.None, CopyBufferSize, useAsync: true);

                int bytesRead;
                while ((bytesRead = await entryStream.ReadAsync(buffer)) > 0)
                    await outStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            }
        }
    }
}
