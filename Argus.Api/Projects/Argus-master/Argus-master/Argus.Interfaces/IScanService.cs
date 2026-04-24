using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Argus.Dto.Scans;

namespace Argus.Interfaces
{
    public interface IScanService
    {
        Task<ScanRunDto> StartScanAsync(Guid projectId);
        Task<List<ScanRunDto>> GetScansByProjectAsync(Guid projectId);
        Task<ScanRunDto> GetScanByIdAsync(Guid scanId);
        Task<ScanRunDto> GetLatestScanAsync(Guid projectId);
    }
}
