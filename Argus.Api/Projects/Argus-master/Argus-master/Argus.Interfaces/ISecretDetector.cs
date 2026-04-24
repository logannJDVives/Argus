using System.Collections.Generic;
using System.Threading.Tasks;
using Argus.Interfaces.Models;

namespace Argus.Interfaces
{
    public interface ISecretDetector
    {
        Task<IReadOnlyList<SecretFinding>> DetectAsync(ScannedFile file);
    }
}
