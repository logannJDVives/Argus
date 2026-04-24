using System.Threading;
using System.Threading.Tasks;
using Argus.Interfaces.Models;

namespace Argus.Interfaces
{
    public interface INuGetEnricher
    {
        Task<NuGetPackageMetadata> GetMetadataAsync(
            string           packageId,
            string           version,
            CancellationToken ct = default);
    }
}
