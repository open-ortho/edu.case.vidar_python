using Xunit;
using Moq;
using BFD9010.FhirApi.Services;
using Microsoft.Extensions.Logging;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Text.Json;

namespace BFD9010.FhirApi.Tests;

/// <summary>
/// Tests for FHIR API compliance and functionality
/// </summary>
public class FhirApiTests
{
    private readonly Mock<ILogger<ScannerService>> _mockLogger;
    private readonly FhirJsonSerializer _serializer;
    private readonly FhirJsonParser _parser;
    
    public FhirApiTests()
    {
        _mockLogger = new Mock<ILogger<ScannerService>>();
        _serializer = new FhirJsonSerializer(new SerializerSettings { Pretty = true });
        _parser = new FhirJsonParser();
    }

    [Fact]
    public void FhirBundle_ShouldHaveCorrectResourceType()
    {
        // Arrange
        var bundle = new Bundle
        {
            Type = Bundle.BundleType.Collection
        };

        // Act & Assert
        Assert.Equal("Bundle", bundle.TypeName.ToString());
        Assert.Equal(Bundle.BundleType.Collection, bundle.Type);
    }

    [Fact]
    public void FhirBinary_ShouldHaveCorrectStructure()
    {
        // Arrange
        var testData = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        var binary = new Binary
        {
            ContentType = "image/png",
            Data = testData
        };

        // Act & Assert
        Assert.Equal("Binary", binary.TypeName.ToString());
        Assert.Equal("image/png", binary.ContentType);
        Assert.Equal(testData, binary.Data);
    }

    [Fact]
    public void FhirOperationOutcome_Success_ShouldHaveInformationSeverity()
    {
        // Arrange
        var outcome = new OperationOutcome
        {
            Issue = new List<OperationOutcome.IssueComponent>
            {
                new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Information,
                    Code = OperationOutcome.IssueType.Informational,
                    Details = new CodeableConcept { Text = "Scan completed successfully" }
                }
            }
        };

        // Act & Assert
        Assert.Equal("OperationOutcome", outcome.TypeName.ToString());
        Assert.Single(outcome.Issue);
        Assert.Equal(OperationOutcome.IssueSeverity.Information, outcome.Issue[0].Severity);
        Assert.Equal(OperationOutcome.IssueType.Informational, outcome.Issue[0].Code);
        Assert.Equal("Scan completed successfully", outcome.Issue[0].Details.Text);
    }

    [Fact]
    public void FhirOperationOutcome_Error_ShouldHaveErrorSeverity()
    {
        // Arrange
        var outcome = new OperationOutcome
        {
            Issue = new List<OperationOutcome.IssueComponent>
            {
                new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.Exception,
                    Details = new CodeableConcept { Text = "Scan failed" }
                }
            }
        };

        // Act & Assert
        Assert.Equal(OperationOutcome.IssueSeverity.Error, outcome.Issue[0].Severity);
        Assert.Equal(OperationOutcome.IssueType.Exception, outcome.Issue[0].Code);
    }

    [Fact]
    public void FhirDevice_ShouldContainRequiredFields()
    {
        // Arrange
        var device = new Device
        {
            Id = "scanner-001",
            Manufacturer = "Vidar Systems Corporation",
            ModelNumber = "VXR-16 DosimetryPRO",
            SerialNumber = "ABC123"
        };

        device.Version.Add(new Device.VersionComponent
        {
            Type = new CodeableConcept { Text = "Firmware" },
            Value = "2.01"
        });

        // Act & Assert
        Assert.Equal("Device", device.TypeName.ToString());
        Assert.Equal("scanner-001", device.Id);
        Assert.Equal("Vidar Systems Corporation", device.Manufacturer);
        Assert.Equal("VXR-16 DosimetryPRO", device.ModelNumber);
        Assert.Equal("ABC123", device.SerialNumber);
        Assert.Single(device.Version);
        Assert.Equal("Firmware", device.Version[0].Type.Text);
        Assert.Equal("2.01", device.Version[0].Value);
    }

    [Fact]
    public void ScanBundle_ShouldContainBinaryAndOperationOutcome()
    {
        // Arrange
        var testImageData = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var bundle = new Bundle
        {
            Type = Bundle.BundleType.Collection
        };

        bundle.Entry.Add(new Bundle.EntryComponent
        {
            Resource = new Binary
            {
                ContentType = "image/png",
                Data = testImageData
            }
        });

        bundle.Entry.Add(new Bundle.EntryComponent
        {
            Resource = new OperationOutcome
            {
                Issue = new List<OperationOutcome.IssueComponent>
                {
                    new OperationOutcome.IssueComponent
                    {
                        Severity = OperationOutcome.IssueSeverity.Information,
                        Code = OperationOutcome.IssueType.Informational,
                        Details = new CodeableConcept { Text = "Scan completed successfully" }
                    }
                }
            }
        });

        // Act & Assert
        Assert.Equal(2, bundle.Entry.Count);
        Assert.IsType<Binary>(bundle.Entry[0].Resource);
        Assert.IsType<OperationOutcome>(bundle.Entry[1].Resource);
    }

    [Fact]
    public void FhirBundle_WhenSerialized_ShouldBeValidJson()
    {
        // Arrange
        var bundle = new Bundle
        {
            Type = Bundle.BundleType.Collection
        };

        bundle.Entry.Add(new Bundle.EntryComponent
        {
            Resource = new Binary
            {
                ContentType = "image/png",
                Data = new byte[] { 0x74, 0x65, 0x73, 0x74 } // "test"
            }
        });

        // Act
        var json = _serializer.SerializeToString(bundle);
        var parsed = JsonDocument.Parse(json);

        // Assert
        Assert.NotNull(parsed);
        Assert.Equal("Bundle", parsed.RootElement.GetProperty("resourceType").GetString());
        Assert.Equal("collection", parsed.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public void FhirBinary_DataShouldBeBase64Encoded()
    {
        // Arrange
        var originalData = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG signature

        var binary = new Binary
        {
            ContentType = "image/png",
            Data = originalData
        };

        // Act - serialize and deserialize
        var json = _serializer.SerializeToString(binary);
        var deserialized = _parser.Parse<Binary>(json);

        // Assert
        Assert.Equal(originalData, deserialized.Data);
    }

    [Fact]
    public void FhirDevice_Properties_ShouldSupportQuantity()
    {
        // Arrange
        var device = new Device();
        
        device.Property.Add(new Device.PropertyComponent
        {
            Type = new CodeableConcept { Text = "Resolution" },
            Value = new Quantity(300, "dpi")
        });

        // Act & Assert
        Assert.Single(device.Property);
        Assert.Equal("Resolution", device.Property[0].Type.Text);
        Assert.NotNull(device.Property[0].Value);
        Assert.IsType<Quantity>(device.Property[0].Value);
        var quantity = (Quantity)device.Property[0].Value;
        Assert.Equal(300, quantity.Value);
        Assert.Equal("dpi", quantity.Unit);
    }

    [Fact]
    public void OperationOutcome_ShouldSupportMultipleIssues()
    {
        // Arrange
        var outcome = new OperationOutcome();
        
        outcome.Issue.Add(new OperationOutcome.IssueComponent
        {
            Severity = OperationOutcome.IssueSeverity.Warning,
            Code = OperationOutcome.IssueType.Processing,
            Details = new CodeableConcept { Text = "Calibration recommended" }
        });

        outcome.Issue.Add(new OperationOutcome.IssueComponent
        {
            Severity = OperationOutcome.IssueSeverity.Information,
            Code = OperationOutcome.IssueType.Informational,
            Details = new CodeableConcept { Text = "Scan completed" }
        });

        // Act & Assert
        Assert.Equal(2, outcome.Issue.Count);
        Assert.Equal(OperationOutcome.IssueSeverity.Warning, outcome.Issue[0].Severity);
        Assert.Equal(OperationOutcome.IssueSeverity.Information, outcome.Issue[1].Severity);
    }

    [Fact]
    public void FhirBundle_ErrorScenario_ShouldOnlyContainOperationOutcome()
    {
        // Arrange
        var bundle = new Bundle
        {
            Type = Bundle.BundleType.Collection
        };

        bundle.Entry.Add(new Bundle.EntryComponent
        {
            Resource = new OperationOutcome
            {
                Issue = new List<OperationOutcome.IssueComponent>
                {
                    new OperationOutcome.IssueComponent
                    {
                        Severity = OperationOutcome.IssueSeverity.Error,
                        Code = OperationOutcome.IssueType.Exception,
                        Details = new CodeableConcept { Text = "Scanner not initialized" }
                    }
                }
            }
        });

        // Act & Assert
        Assert.Single(bundle.Entry);
        Assert.IsType<OperationOutcome>(bundle.Entry[0].Resource);
        var outcome = (OperationOutcome)bundle.Entry[0].Resource;
        Assert.Equal(OperationOutcome.IssueSeverity.Error, outcome.Issue[0].Severity);
    }
}
