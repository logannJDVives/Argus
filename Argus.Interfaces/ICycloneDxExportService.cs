using System;
using System.Threading.Tasks;

namespace Argus.Interfaces
{
    public interface ICycloneDxExportService
    {
        Task<string> GenerateCycloneDxJsonAsync(Guid scanId);
    }
}
