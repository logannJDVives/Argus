using System;
using System.Threading.Tasks;
using Argus.Dto.Components;

namespace Argus.Interfaces
{
    public interface IComponentService
    {
        Task<PaginatedComponentsDto> GetComponentsByScanAsync(
            Guid   scanId,
            string search   = null,
            int    page     = 1,
            int    pageSize = 25);
    }
}
