using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Argus.Entities;
using Argus.Interfaces.Models;

namespace Argus.Services.Detection
{
    public partial class HeuristicFilter
    {
        // Bekende placeholder-waarden die nooit echte secrets zijn
        private static readonly HashSet<string> Placeholders = new(StringComparer.OrdinalIgnoreCase)
        {
            "todo", "xxx", "changeme", "your-key-here", "<placeholder>", "placeholder",
            "example", "sample", "dummy", "fake", "abc123", "password123", "test",
            "your_secret_here", "insert_key_here", "replace_me", "n/a", "none",
            "null", "undefined", "empty", "insert-your-key", "your-api-key-here"
        };

        // Bestandsextensies / namen die vertrouwelijk zijn → Confidence verhogen
        private static readonly HashSet<string> SensitiveExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".env", ".config"
        };

        // Commentaarregels: //, /*, *, #, <!--
        [GeneratedRegex(@"^\s*(//|/\*|\*|#|<!--)")]
        private static partial Regex CommentPattern();

        // URL: http://, https://, ftp://
        [GeneratedRegex(@"^https?://|^ftp://", RegexOptions.IgnoreCase)]
        private static partial Regex UrlPattern();

        // Bestandspad: /path/to of C:\path\to
        [GeneratedRegex(@"^[/\\]|^[a-zA-Z]:[/\\]")]
        private static partial Regex PathPattern();

        // Backreference: varName = "varName" → waarde is identiek aan variabelenaam
        [GeneratedRegex(@"\b(\w+)\s*[:=]\s*[""']?\1[""']?", RegexOptions.IgnoreCase)]
        private static partial Regex IdenticalNameValuePattern();

        // Gevoelig sleutelwoord in de regel → Confidence verhogen
        [GeneratedRegex(@"secret|key|token|password|credential|apikey|auth|passwd|private", RegexOptions.IgnoreCase)]
        private static partial Regex SensitiveKeywordPattern();

        public IReadOnlyList<SecretFinding> Filter(IReadOnlyList<SecretFinding> findings)
        {
            var result = new List<SecretFinding>(findings.Count);

            foreach (var finding in findings)
            {
                if (ShouldExclude(finding))
                    continue;

                result.Add(AdjustConfidence(finding));
            }

            return result;
        }

        private static bool ShouldExclude(SecretFinding finding)
        {
            // Commentaarregel
            if (CommentPattern().IsMatch(finding.LineContent))
                return true;

            // Bekende placeholder
            if (Placeholders.Contains(finding.MatchedValue))
                return true;

            // URL of bestandspad
            if (UrlPattern().IsMatch(finding.MatchedValue) || PathPattern().IsMatch(finding.MatchedValue))
                return true;

            // Test-map of test-bestand
            var normalizedPath = finding.FilePath.Replace('\\', '/');
            if (normalizedPath.Contains("/test/", StringComparison.OrdinalIgnoreCase)  ||
                normalizedPath.Contains("/tests/", StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.EndsWith(".Test.cs", StringComparison.OrdinalIgnoreCase))
                return true;

            // Waarde is identiek aan de variabelenaam: password = "password"
            if (IdenticalNameValuePattern().IsMatch(finding.LineContent))
                return true;

            return false;
        }

        private static SecretFinding AdjustConfidence(SecretFinding finding)
        {
            var confidence = finding.Confidence;

            // Gevoelig bestandstype → direct High
            var extension = Path.GetExtension(finding.FilePath);
            var fileName  = Path.GetFileName(finding.FilePath);

            if (SensitiveExtensions.Contains(extension) ||
                fileName.StartsWith(".env", StringComparison.OrdinalIgnoreCase))
                confidence = Confidence.High;

            // Gevoelig sleutelwoord in de regel → één niveau omhoog
            if (SensitiveKeywordPattern().IsMatch(finding.LineContent))
                confidence = BumpConfidence(confidence);

            return confidence == finding.Confidence
                ? finding
                : finding with { Confidence = confidence };
        }

        private static Confidence BumpConfidence(Confidence current)
            => current < Confidence.High ? (Confidence)((int)current + 1) : Confidence.High;
    }
}
