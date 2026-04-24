using System;

namespace Argus.Dto.Projects
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastScanDate { get; set; }
        public int ScanRunCount { get; set; }
    }
}
