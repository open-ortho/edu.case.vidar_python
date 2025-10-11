namespace BFD9010.FhirApi.Models;

/// <summary>
/// FHIR Bundle resource
/// </summary>
public class FhirBundle
{
    public string ResourceType { get; set; } = "Bundle";
    public string Type { get; set; } = "collection";
    public List<BundleEntry> Entry { get; set; } = new();
}

public class BundleEntry
{
    public object? Resource { get; set; }
}

/// <summary>
/// FHIR Binary resource for image data
/// </summary>
public class FhirBinary
{
    public string ResourceType { get; set; } = "Binary";
    public string ContentType { get; set; } = "image/png";
    public string? Data { get; set; }
}

/// <summary>
/// FHIR OperationOutcome resource
/// </summary>
public class FhirOperationOutcome
{
    public string ResourceType { get; set; } = "OperationOutcome";
    public List<OperationOutcomeIssue> Issue { get; set; } = new();
}

public class OperationOutcomeIssue
{
    public string Severity { get; set; } = "information";
    public string Code { get; set; } = "informational";
    public IssueDetails Details { get; set; } = new();
}

public class IssueDetails
{
    public string? Text { get; set; }
}

/// <summary>
/// FHIR Device resource
/// </summary>
public class FhirDevice
{
    public string ResourceType { get; set; } = "Device";
    public string? Id { get; set; }
    public List<DeviceIdentifier> Identifier { get; set; } = new();
    public string? Manufacturer { get; set; }
    public string? ModelNumber { get; set; }
    public string? SerialNumber { get; set; }
    public List<DeviceVersion> Version { get; set; } = new();
    public List<DeviceProperty> Property { get; set; } = new();
}

public class DeviceIdentifier
{
    public string? System { get; set; }
    public string? Value { get; set; }
}

public class DeviceVersion
{
    public CodeableConcept? Type { get; set; }
    public string? Value { get; set; }
}

public class DeviceProperty
{
    public CodeableConcept? Type { get; set; }
    public List<Quantity> ValueQuantity { get; set; } = new();
    public List<CodeableConcept> ValueCodeableConcept { get; set; } = new();
}

public class CodeableConcept
{
    public string? Text { get; set; }
}

public class Quantity
{
    public decimal? Value { get; set; }
    public string? Unit { get; set; }
}
