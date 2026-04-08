namespace Argus.Interfaces.Models
{
    public class ParsedPackageReference
    {
        public string Name { get; init; } = string.Empty;
        public string Version { get; init; } = string.Empty;
        public bool IsTransitive { get; init; }
    }
}
