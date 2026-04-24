using System.Collections.Generic;
using System.Threading.Tasks;
using Argus.Interfaces.Models;

namespace Argus.Interfaces
{
    public interface ICsprojParser
    {
        Task<IReadOnlyList<ParsedPackageReference>> ParseAsync(string csprojPath);
    }
}
