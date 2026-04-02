using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Argus.Entities;
using Argus.Interfaces;
using Argus.Interfaces.Models;

namespace Argus.Services.Detection
{
    public partial class EntropyDetector : ISecretDetector
    {
        private const int    MinTokenLength        = 16;
        private const double HexEntropyThreshold   = 3.0;
        private const double MixedEntropyThreshold = 4.5;

        // Extracts string literals ("..." of '...') en waarden na = of :
        [GeneratedRegex(@"[""']([^""']{16,})[""']|[:=]\s*([A-Za-z0-9+/=_\-.]{16,})")]
        private static partial Regex TokenExtractor();

        [GeneratedRegex(@"^[0-9a-fA-F]+$")]
        private static partial Regex HexPattern();

        public async Task<IReadOnlyList<SecretFinding>> DetectAsync(ScannedFile file)
        {
            var findings = new List<SecretFinding>();
            var lines = await File.ReadAllLinesAsync(file.FullPath);

            for (var lineNumber = 1; lineNumber <= lines.Length; lineNumber++)
            {
                var line = lines[lineNumber - 1];

                foreach (Match match in TokenExtractor().Matches(line))
                {
                    // Groep 1 = string literal inhoud, Groep 2 = waarde na = of :
                    var token = match.Groups[1].Success
                        ? match.Groups[1].Value
                        : match.Groups[2].Value;

                    if (token.Length < MinTokenLength)
                        continue;

                    var entropy  = CalculateShannonEntropy(token);
                    var threshold = HexPattern().IsMatch(token)
                        ? HexEntropyThreshold
                        : MixedEntropyThreshold;

                    if (entropy <= threshold)
                        continue;

                    findings.Add(new SecretFinding
                    {
                        FilePath     = file.RelativePath,
                        LineNumber   = lineNumber,
                        LineContent  = line,
                        MatchedValue = token,
                        RuleId       = "HIGH_ENTROPY",
                        DetectorType = DetectorType.Entropy,
                        Severity     = Severity.Medium,
                        Confidence   = Confidence.Medium
                    });
                }
            }

            return findings;
        }

        // H(X) = -Σ p(x) × log₂(p(x))
        private static double CalculateShannonEntropy(string value)
        {
            var length = (double)value.Length;
            return value
                .GroupBy(c => c)
                .Select(g => g.Count() / length)
                .Sum(p => -p * Math.Log2(p));
        }
    }
}
