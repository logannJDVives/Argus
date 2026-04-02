using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Argus.Entities;
using Argus.Interfaces;
using Argus.Interfaces.Models;

namespace Argus.Services.Detection
{
    public class RegexDetector : ISecretDetector
    {
        private static readonly IReadOnlyList<DetectionRule> Rules =
        [
            new("AWS_ACCESS_KEY",    @"AKIA[0-9A-Z]{16}",                                               Severity.Critical),
            new("AWS_SECRET_KEY",    @"(?i)aws_secret_access_key\s*=\s*\S+",                            Severity.Critical),
            new("GENERIC_API_KEY",   @"(?i)(api[_-]?key|apikey)\s*[:=]\s*[""']?\S{16,}",               Severity.High),
            new("CONNECTION_STRING", @"(?i)(password|pwd)\s*=\s*[^;]{4,}",                              Severity.High),
            new("PRIVATE_KEY",       @"-----BEGIN (RSA|EC|OPENSSH) PRIVATE KEY-----",                   Severity.Critical),
            new("GENERIC_SECRET",    @"(?i)(secret|token|password)\s*[:=]\s*[""']?\S{8,}",              Severity.Medium),
            new("GITHUB_TOKEN",      @"gh[ps]_[A-Za-z0-9_]{36,}",                                       Severity.Critical),
            new("AZURE_KEY",         @"(?i)(AccountKey|SharedAccessKey)\s*=\s*[A-Za-z0-9+/=]{20,}",    Severity.Critical),
        ];

        public async Task<IReadOnlyList<SecretFinding>> DetectAsync(ScannedFile file)
        {
            var findings = new List<SecretFinding>();
            var lines = await File.ReadAllLinesAsync(file.FullPath);

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

        private sealed record DetectionRule(string RuleId, Regex Pattern, Severity Severity)
        {
            public DetectionRule(string ruleId, string pattern, Severity severity)
                : this(ruleId, new Regex(pattern, RegexOptions.Compiled), severity) { }
        }
    }
}
