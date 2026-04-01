using System;

namespace Argus.Dto.Scans
{
    public class ScanRunDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; }
        public int SecretCount { get; set; }
        public int ComponentCount { get; set; }
        public string ErrorMessage { get; set; }
        public TimeSpan? Duration { get; set; }
        public long? FilesScanned { get; set; }
    }
}
