namespace Argus.Interfaces.Models
{
    public class NuGetVersionInfo
    {
        public string LatestVersion { get; set; } = string.Empty;
        public bool IsDeprecated { get; set; }
        public string DeprecationMessage { get; set; } = string.Empty;
    }
}
