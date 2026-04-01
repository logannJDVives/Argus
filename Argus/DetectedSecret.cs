using System;

namespace Argus.Entities
{
    public class DetectedSecret
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string MaskedValue { get; set; }
        public string Hash { get; set; }
        public Severity Severity { get; set; }

        // Detection Analysis
        public string RuleId { get; set; }
        public Confidence Confidence { get; set; }

        // Review & Validation
        public bool IsFalsePositive { get; set; }
        public bool IsReviewed { get; set; }

        // Foreign Key
        public Guid ScanRunId { get; set; }

        // Navigation property
        public ScanRun ScanRun { get; set; }
    }
}
