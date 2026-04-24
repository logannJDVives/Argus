using System;
using System.Threading.Tasks;
using Argus.Dto.Secrets;

namespace Argus.Interfaces
{
    public interface ISecretService
    {
        Task<PaginatedSecretsDto> GetSecretsByScanAsync(Guid scanId, string severity = null, string filePath = null, int page = 1, int pageSize = 10);
        Task ReviewSecretAsync(Guid id, bool isReviewed, bool isFalsePositive);
    }
}
