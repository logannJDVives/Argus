using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Argus.Interfaces;
using Argus.Interfaces.Models;
using Microsoft.Extensions.Logging;

namespace Argus.Services
{
    public class ProjectFileScannerService : IProjectFileScannerService
    {
        private readonly ILogger<ProjectFileScannerService> _logger;

        private static readonly HashSet<string> ExcludedDirectories = new(System.StringComparer.OrdinalIgnoreCase)
        {
            "bin", "obj", ".git", ".vs", "node_modules", "packages"
        };

        private static readonly Dictionary<string, FileCategory> ExtensionMap = new(System.StringComparer.OrdinalIgnoreCase)
        {
            [".cs"]         = FileCategory.SourceCode,
            [".js"]         = FileCategory.SourceCode,
            [".ts"]         = FileCategory.SourceCode,
            [".csproj"]     = FileCategory.ProjectFile,
            [".sln"]        = FileCategory.ProjectFile,
            [".props"]      = FileCategory.ProjectFile,
            [".targets"]    = FileCategory.ProjectFile,
            [".json"]       = FileCategory.Config,
            [".xml"]        = FileCategory.Config,
            [".config"]     = FileCategory.Config,
            [".yaml"]       = FileCategory.CI,
            [".yml"]        = FileCategory.CI,
            [".env"]        = FileCategory.Environment,
            [".dockerfile"] = FileCategory.Docker,
        };

        public ProjectFileScannerService(ILogger<ProjectFileScannerService> logger)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public Task<IReadOnlyList<ScannedFile>> ScanProjectAsync(string projectPath)
        {
            _logger.LogInformation("Starting file scan for project path: {ProjectPath}", projectPath);

            if (!Directory.Exists(projectPath))
            {
                _logger.LogError("Project path does not exist: {ProjectPath}", projectPath);
                throw new DirectoryNotFoundException($"Project path not found: {projectPath}");
            }

            var results = new List<ScannedFile>();
            ScanDirectory(new DirectoryInfo(projectPath), projectPath, results);

            _logger.LogInformation("File scan completed. Found {FileCount} scannable files in {ProjectPath}", 
                results.Count, projectPath);

            return Task.FromResult<IReadOnlyList<ScannedFile>>(results);
        }

        private void ScanDirectory(DirectoryInfo directory, string projectRoot, List<ScannedFile> results)
        {
            _logger.LogDebug("Scanning directory: {DirectoryPath}", directory.FullName);

            try
            {
                foreach (var file in directory.EnumerateFiles())
                {
                    var category = ResolveCategory(file);
                    if (category is null)
                        continue;

                    if (IsBinaryFile(file))
                    {
                        _logger.LogDebug("Skipping binary file: {FilePath}", file.FullName);
                        continue;
                    }

                    results.Add(new ScannedFile
                    {
                        FullPath     = file.FullName,
                        RelativePath = Path.GetRelativePath(projectRoot, file.FullName),
                        Extension    = file.Extension,
                        SizeInBytes  = file.Length,
                        Category     = category.Value
                    });
                }

                foreach (var subdirectory in directory.EnumerateDirectories())
                {
                    if (ExcludedDirectories.Contains(subdirectory.Name))
                    {
                        _logger.LogDebug("Skipping excluded directory: {DirectoryName}", subdirectory.Name);
                        continue;
                    }

                    ScanDirectory(subdirectory, projectRoot, results);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Access denied to directory: {DirectoryPath}", directory.FullName);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "I/O error scanning directory: {DirectoryPath}", directory.FullName);
            }
        }

        private static FileCategory? ResolveCategory(FileInfo file)
        {
            // Dockerfile heeft geen extensie
            if (file.Name.Equals("Dockerfile", System.StringComparison.OrdinalIgnoreCase))
                return FileCategory.Docker;

            // .env.development, .env.production, etc.
            if (file.Name.StartsWith(".env", System.StringComparison.OrdinalIgnoreCase))
                return FileCategory.Environment;

            if (ExtensionMap.TryGetValue(file.Extension, out var category))
                return category;

            return null;
        }

        /// <summary>
        /// Returns true when the first 8 KB of the file contains a null byte,
        /// which is a reliable heuristic for binary content.
        /// </summary>
        private bool IsBinaryFile(FileInfo file)
        {
            const int sampleSize = 8 * 1024; // 8 KB

            try
            {
                using var fs     = file.OpenRead();
                var       buffer = new byte[sampleSize];
                var       read   = fs.Read(buffer, 0, sampleSize);

                for (var i = 0; i < read; i++)
                {
                    if (buffer[i] == 0x00)
                        return true;
                }

                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Access denied reading file for binary check: {FilePath}", file.FullName);
                return true;
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "I/O error reading file for binary check: {FilePath}", file.FullName);
                return true;
            }
        }
    }
}

