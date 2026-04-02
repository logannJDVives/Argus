using System.Threading.Tasks;
using Argus.Dto.Projects;
using Microsoft.AspNetCore.Http;

namespace Argus.Interfaces
{
    public interface IProjectUploadService
    {
        Task<UploadProjectResponseDto> UploadAndCreateProjectAsync(IFormFile zipFile);
    }
}
