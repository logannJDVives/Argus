using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Argus.Interfaces;
using Argus.Interfaces.Models;

namespace Argus.Services
{
    public class ProjectFileScannerService : IProjectFileScannerService
    {
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

        public Task<IReadOnlyList<ScannedFile>> ScanProjectAsync(string projectPath)
        {
            var results = new List<ScannedFile>();
            ScanDirectory(new DirectoryInfo(projectPath), projectPath, results);
            return Task.FromResult<IReadOnlyList<ScannedFile>>(results);
        }

        private static void ScanDirectory(DirectoryInfo directory, string projectRoot, List<ScannedFile> results)
        {
            foreach (var file in directory.EnumerateFiles())
            {
                var category = ResolveCategory(file);
                if (category is null)
                    continue;

                if (IsBinaryFile(file))
                    continue;

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
                    continue;

                ScanDirectory(subdirectory, projectRoot, results);
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
        private static bool IsBinaryFile(FileInfo file)
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
            catch
            {
                // If we can't read the file, treat it as binary to skip safely.
                return true;
            }
        }
    }
}

