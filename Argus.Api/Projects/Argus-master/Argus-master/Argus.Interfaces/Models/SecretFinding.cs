using Argus.Entities;

namespace Argus.Interfaces.Models
{
    public record SecretFinding
    {
        public string FilePath { get; init; }
        public int LineNumber { get; init; }
        public string LineContent { get; init; }
        public string MatchedValue { get; init; }
        public string RuleId { get; init; }
        public DetectorType DetectorType { get; init; }
        public Severity Severity { get; init; }
        public Confidence Confidence { get; init; }
    }
}
