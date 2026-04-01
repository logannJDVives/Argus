using System;
using System.Collections.Generic;

namespace Argus.Entities
{
    public class SoftwareComponent
    {
        // Identificatie
        public Guid Id { get; set; }

        // Component informatie (SBOM)
        public string Name { get; set; }
        public string Version { get; set; }
        public string Type { get; set; }
        public string License { get; set; }
        public string PackageUrl { get; set; }

        // Dependency informatie
        public bool IsTransitive { get; set; }
        public string Description { get; set; }
        public string Homepage { get; set; }

        // Security informatie
        public string PublisherUrl { get; set; }
        public DateTime? PublishedDate { get; set; }
        public bool HasKnownVulnerabilities { get; set; }

        // Foreign Key to ScanRun
        public Guid ScanRunId { get; set; }

        // Self-referencing FK for dependency tree
        public Guid? ParentComponentId { get; set; }

        // Navigation properties
        public ScanRun ScanRun { get; set; }

        // Self-referencing navigation for dependency tree
        public SoftwareComponent ParentComponent { get; set; }
        public List<SoftwareComponent> ChildComponents { get; set; } = new();

        // Vulnerability navigation
        public List<Vulnerability> Vulnerabilities { get; set; } = new();
    }
}
