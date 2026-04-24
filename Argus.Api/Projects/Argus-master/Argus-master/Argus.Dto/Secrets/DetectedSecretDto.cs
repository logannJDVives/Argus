namespace Argus.Dto.Secrets
{
    public class DetectedSecretDto
    {
        public System.Guid Id { get; set; }
        public string Type { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string MaskedValue { get; set; }
        public string Severity { get; set; }
        public string RuleId { get; set; }
        public string Confidence { get; set; }
        public bool IsFalsePositive { get; set; }
        public bool IsReviewed { get; set; }
    }
}
