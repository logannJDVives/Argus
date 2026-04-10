using System;

namespace Argus.Dto.Components
{
    public class SoftwareComponentDto
    {
        public Guid      Id                      { get; set; }
        public string    Name                    { get; set; }
        public string    Version                 { get; set; }
        public string    Type                    { get; set; }
        public string    PackageUrl              { get; set; }
        public string    License                 { get; set; }
        public bool      IsTransitive            { get; set; }
        public bool      HasKnownVulnerabilities { get; set; }
        public string    Description             { get; set; }
        public string    Homepage                { get; set; }
        public string    PublisherUrl            { get; set; }
        public DateTime? PublishedDate           { get; set; }
    }
}
