using System;
using System.Collections.Generic;

namespace Argus.Entities
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastScanDate { get; set; }

        // Navigation properties
        public List<ScanRun> ScanRuns { get; set; } = new();
    }
}
