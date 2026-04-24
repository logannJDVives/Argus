using System;
using System.Collections.Generic;

namespace Argus.Entities
{
    public class ScanRun
    {
        public Guid Id { get; set; }

        // Scan metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public ScanStatus Status { get; set; }

        // Scan results
        public int SecretCount { get; set; }
        public int ComponentCount { get; set; }
        public string? ErrorMessage { get; set; }

        // Performance metrics
        public TimeSpan? Duration { get; set; }
        public long? FilesScanned { get; set; }

        // Foreign Key
        public Guid ProjectId { get; set; }

        // Navigation properties
        public Project Project { get; set; }
        public List<DetectedSecret> Secrets { get; set; } = new();
        public List<SoftwareComponent> Components { get; set; } = new();
    }
}
