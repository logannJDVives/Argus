using System;

namespace Argus.Interfaces.Models
{
    public class NuGetPackageMetadata
    {
        public string    License       { get; init; } = string.Empty;
        public string    Description   { get; init; } = string.Empty;
        public string    Homepage      { get; init; } = string.Empty;
        public string    Authors       { get; init; } = string.Empty;
        public DateTime? PublishedDate { get; init; }
    }
}
