using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Argus.Interfaces;
using Argus.Interfaces.Models;

namespace Argus.Services.Parsing
{
    public class CsprojPackageParser : ICsprojParser
    {
        public async Task<IReadOnlyList<ParsedPackageReference>> ParseAsync(string csprojPath)
        {
            if (!File.Exists(csprojPath))
                return [];

            using var stream = File.OpenRead(csprojPath);
            var doc = await XDocument.LoadAsync(stream, LoadOptions.None, default);

            return doc.Descendants("PackageReference")
                .Select(el => new ParsedPackageReference
                {
                    Name = el.Attribute("Include")?.Value ?? string.Empty,
                    Version = el.Attribute("Version")?.Value
                              ?? el.Element("Version")?.Value
                              ?? string.Empty,
                    IsTransitive = false
                })
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .ToList();
        }
    }
}
