using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Argus.Dto.Sbom
{
    public class CycloneDxDto
    {
        [JsonPropertyName("bomFormat")]
        public string BomFormat { get; set; } = "CycloneDX";

        [JsonPropertyName("specVersion")]
        public string SpecVersion { get; set; } = "1.4";

        [JsonPropertyName("serialNumber")]
        public string SerialNumber { get; set; } = $"urn:uuid:{Guid.NewGuid()}";

        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        [JsonPropertyName("metadata")]
        public CycloneDxMetadataDto Metadata { get; set; } = new();

        [JsonPropertyName("components")]
        public List<CycloneDxComponentDto> Components { get; set; } = [];
    }

    public class CycloneDxMetadataDto
    {
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");

        [JsonPropertyName("tools")]
        public List<CycloneDxToolDto> Tools { get; set; } =
        [
            new() { Vendor = "Argus", Name = "Argus Secret & SBOM Scanner", Version = "1.0.0" }
        ];
    }

    public class CycloneDxToolDto
    {
        [JsonPropertyName("vendor")]
        public string Vendor { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }

    public class CycloneDxComponentDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "library";

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("purl")]
        public string? Purl { get; set; }

        [JsonPropertyName("licenses")]
        public List<CycloneDxLicenseWrapperDto>? Licenses { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("externalReferences")]
        public List<CycloneDxExternalReferenceDto>? ExternalReferences { get; set; }
    }

    public class CycloneDxLicenseWrapperDto
    {
        [JsonPropertyName("license")]
        public CycloneDxLicenseDto License { get; set; } = new();
    }

    public class CycloneDxLicenseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    public class CycloneDxExternalReferenceDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
