using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Argus.Entities;
using Argus.Interfaces;
using Argus.Interfaces.Models;

namespace Argus.Services.Detection
{
    public partial class RegexDetector : ISecretDetector
    {
        [GeneratedRegex(@"AKIA[0-9A-Z]{16}")]
        private static partial Regex AwsAccessKeyPattern();

        [GeneratedRegex(@"aws_secret_access_key\s*=\s*\S+", RegexOptions.IgnoreCase)]
        private static partial Regex AwsSecretKeyPattern();

        [GeneratedRegex(@"(api[_-]?key|apikey)\s*[:=]\s*[""']?\S{16,}", RegexOptions.IgnoreCase)]
        private static partial Regex GenericApiKeyPattern();

        [GeneratedRegex(@"(password|pwd)\s*=\s*[^;]{4,}", RegexOptions.IgnoreCase)]
        private static partial Regex ConnectionStringPattern();

        [GeneratedRegex(@"-----BEGIN (RSA|EC|OPENSSH) PRIVATE KEY-----")]
        private static partial Regex PrivateKeyPattern();

        [GeneratedRegex(@"(secret|token|password)\s*[:=]\s*[""']?\S{8,}", RegexOptions.IgnoreCase)]
        private static partial Regex GenericSecretPattern();

        [GeneratedRegex(@"gh[ps]_[A-Za-z0-9_]{36,}")]
        private static partial Regex GithubTokenPattern();

        [GeneratedRegex(@"(AccountKey|SharedAccessKey)\s*=\s*[A-Za-z0-9+/=]{20,}", RegexOptions.IgnoreCase)]
        private static partial Regex AzureKeyPattern();

        private static readonly IReadOnlyList<DetectionRule> Rules =
        [
            new("AWS_ACCESS_KEY",    AwsAccessKeyPattern(),     Severity.Critical),
            new("AWS_SECRET_KEY",    AwsSecretKeyPattern(),     Severity.Critical),
            new("GENERIC_API_KEY",   GenericApiKeyPattern(),    Severity.High),
            new("CONNECTION_STRING", ConnectionStringPattern(), Severity.High),
            new("PRIVATE_KEY",       PrivateKeyPattern(),       Severity.Critical),
            new("GENERIC_SECRET",    GenericSecretPattern(),    Severity.Medium),
            new("GITHUB_TOKEN",      GithubTokenPattern(),      Severity.Critical),
            new("AZURE_KEY",         AzureKeyPattern(),         Severity.Critical),
        ];

        public async Task<IReadOnlyList<SecretFinding>> DetectAsync(ScannedFile file)
        {
            var findings = new List<SecretFinding>();

            string[] lines;
            try
            {
                lines = await File.ReadAllLinesAsync(file.FullPath);
            }
            catch
            {
                return findings;
            }

            for (var lineNumber = 1; lineNumber <= lines.Length; lineNumber++)
            {
                var line = lines[lineNumber - 1];

                foreach (var rule in Rules)
                {
                    var match = rule.Pattern.Match(line);
                    if (!match.Success)
                        continue;

                    findings.Add(new SecretFinding
                    {
                        FilePath     = file.RelativePath,
                        LineNumber   = lineNumber,
                        LineContent  = line,
                        MatchedValue = match.Value,
                        RuleId       = rule.RuleId,
                        DetectorType = DetectorType.Regex,
                        Severity     = rule.Severity,
                        Confidence   = Confidence.Medium
                    });
                }
            }

            return findings;
        }

        private sealed record DetectionRule(string RuleId, Regex Pattern, Severity Severity);
    }
}
