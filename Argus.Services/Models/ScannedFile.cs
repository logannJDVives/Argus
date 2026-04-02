namespace Argus.Services.Models
{
    public class ScannedFile
    {
        public string FullPath { get; init; }
        public string RelativePath { get; init; }
        public string Extension { get; init; }
        public long SizeInBytes { get; init; }
        public FileCategory Category { get; init; }
    }
}
