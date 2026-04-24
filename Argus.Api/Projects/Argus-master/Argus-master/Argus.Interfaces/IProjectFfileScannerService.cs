using System.Collections.Generic;
using System.Threading.Tasks;
using Argus.Interfaces.Models;

namespace Argus.Interfaces
{
    public interface IProjectFileScannerService
    {
        Task<IReadOnlyList<ScannedFile>> ScanProjectAsync(string projectPath);
    }
}
