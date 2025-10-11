using System.Text.Json.Serialization;

namespace BFD9010.FhirApi.Models;

/// <summary>
/// FHIR Bundle resource
/// </summary>
public class FhirBundle
{
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "Bundle";
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "collection";
    
    [JsonPropertyName("entry")]
    public List<BundleEntry> Entry { get; set; } = new();
}

public class BundleEntry
{
    [JsonPropertyName("resource")]
    public object? Resource { get; set; }
}

/// <summary>
/// FHIR Binary resource for image data
/// </summary>
public class FhirBinary
{
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "Binary";
    
    [JsonPropertyName("contentType")]
    public string ContentType { get; set; } = "image/png";
    
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}

/// <summary>
/// FHIR OperationOutcome resource
/// </summary>
public class FhirOperationOutcome
{
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "OperationOutcome";
    
    [JsonPropertyName("issue")]
    public List<OperationOutcomeIssue> Issue { get; set; } = new();
}

public class OperationOutcomeIssue
{
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "information";
    
    [JsonPropertyName("code")]
    public string Code { get; set; } = "informational";
    
    [JsonPropertyName("details")]
    public IssueDetails Details { get; set; } = new();
}

public class IssueDetails
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

/// <summary>
/// FHIR Device resource
/// </summary>
public class FhirDevice
{
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "Device";
    
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("identifier")]
    public List<DeviceIdentifier> Identifier { get; set; } = new();
    
    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; set; }
    
    [JsonPropertyName("modelNumber")]
    public string? ModelNumber { get; set; }
    
    [JsonPropertyName("serialNumber")]
    public string? SerialNumber { get; set; }
    
    [JsonPropertyName("version")]
    public List<DeviceVersion> Version { get; set; } = new();
    
    [JsonPropertyName("property")]
    public List<DeviceProperty> Property { get; set; } = new();
}

public class DeviceIdentifier
{
    [JsonPropertyName("system")]
    public string? System { get; set; }
    
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

public class DeviceVersion
{
    [JsonPropertyName("type")]
    public CodeableConcept? Type { get; set; }
    
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

public class DeviceProperty
{
    [JsonPropertyName("type")]
    public CodeableConcept? Type { get; set; }
    
    [JsonPropertyName("valueQuantity")]
    public List<Quantity> ValueQuantity { get; set; } = new();
    
    [JsonPropertyName("valueCodeableConcept")]
    public List<CodeableConcept> ValueCodeableConcept { get; set; } = new();
}

public class CodeableConcept
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class Quantity
{
    [JsonPropertyName("value")]
    public decimal? Value { get; set; }
    
    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
}
