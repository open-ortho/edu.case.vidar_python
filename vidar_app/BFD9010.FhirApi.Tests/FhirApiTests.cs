using Xunit;
using Moq;
using BFD9010.FhirApi.Services;
using BFD9010.FhirApi.Models;
using Microsoft.Extensions.Logging;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using FhirCodeableConcept = Hl7.Fhir.Model.CodeableConcept;
using FhirQuantity = Hl7.Fhir.Model.Quantity;
using ApiCodeableConcept = BFD9010.FhirApi.Models.CodeableConcept;
using ApiQuantity = BFD9010.FhirApi.Models.Quantity;

namespace BFD9010.FhirApi.Tests;

/// <summary>
/// Tests for FHIR API compliance and functionality
/// </summary>
public class FhirApiTests
{
    private readonly Mock<ILogger<ScannerService>> _mockLogger;
    
    public FhirApiTests()
    {
        _mockLogger = new Mock<ILogger<ScannerService>>();
    }

    [Fact]
    public void FhirBundle_ShouldHaveCorrectResourceType()
    {
        // Arrange
        var bundle = new FhirBundle
        {
            ResourceType = "Bundle",
            Type = "collection"
        };

        // Act & Assert
        Assert.Equal("Bundle", bundle.ResourceType);
        Assert.Equal("collection", bundle.Type);
    }

    [Fact]
    public void FhirBinary_ShouldHaveCorrectStructure()
    {
        // Arrange
        var testData = Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47 }); // PNG header
        var binary = new FhirBinary
        {
            ResourceType = "Binary",
            ContentType = "image/png",
            Data = testData
        };

        // Act & Assert
        Assert.Equal("Binary", binary.ResourceType);
        Assert.Equal("image/png", binary.ContentType);
        Assert.Equal(testData, binary.Data);
    }

    [Fact]
    public void FhirOperationOutcome_Success_ShouldHaveInformationSeverity()
    {
        // Arrange
        var outcome = new FhirOperationOutcome
        {
            ResourceType = "OperationOutcome",
            Issue = new List<OperationOutcomeIssue>
            {
                new OperationOutcomeIssue
                {
                    Severity = "information",
                    Code = "informational",
                    Details = new IssueDetails { Text = "Scan completed successfully" }
                }
            }
        };

        // Act & Assert
        Assert.Equal("OperationOutcome", outcome.ResourceType);
        Assert.Single(outcome.Issue);
        Assert.Equal("information", outcome.Issue[0].Severity);
        Assert.Equal("informational", outcome.Issue[0].Code);
        Assert.Equal("Scan completed successfully", outcome.Issue[0].Details.Text);
    }

    [Fact]
    public void FhirOperationOutcome_Error_ShouldHaveErrorSeverity()
    {
        // Arrange
        var outcome = new FhirOperationOutcome
        {
            ResourceType = "OperationOutcome",
            Issue = new List<OperationOutcomeIssue>
            {
                new OperationOutcomeIssue
                {
                    Severity = "error",
                    Code = "exception",
                    Details = new IssueDetails { Text = "Scan failed" }
                }
            }
        };

        // Act & Assert
        Assert.Equal("error", outcome.Issue[0].Severity);
        Assert.Equal("exception", outcome.Issue[0].Code);
    }

    [Fact]
    public void FhirDevice_ShouldContainRequiredFields()
    {
        // Arrange
        var device = new FhirDevice
        {
            ResourceType = "Device",
            Id = "scanner-001",
            Manufacturer = "Vidar Systems Corporation",
            ModelNumber = "VXR-16 DosimetryPRO",
            SerialNumber = "ABC123",
            Version = new List<DeviceVersion>
            {
                new DeviceVersion
                {
                    Type = new ApiCodeableConcept { Text = "Firmware" },
                    Value = "2.01"
                }
            }
        };

        // Act & Assert
        Assert.Equal("Device", device.ResourceType);
        Assert.Equal("scanner-001", device.Id);
        Assert.Equal("Vidar Systems Corporation", device.Manufacturer);
        Assert.Equal("VXR-16 DosimetryPRO", device.ModelNumber);
        Assert.Equal("ABC123", device.SerialNumber);
        Assert.Single(device.Version);
        Assert.Equal("Firmware", device.Version[0].Type?.Text);
        Assert.Equal("2.01", device.Version[0].Value);
    }

    [Fact]
    public void ScanBundle_ShouldContainBinaryAndOperationOutcome()
    {
        // Arrange
        var testImageData = Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        var bundle = new FhirBundle
        {
            ResourceType = "Bundle",
            Type = "collection",
            Entry = new List<BundleEntry>
            {
                new BundleEntry
                {
                    Resource = new FhirBinary
                    {
                        ResourceType = "Binary",
                        ContentType = "image/png",
                        Data = testImageData
                    }
                },
                new BundleEntry
                {
                    Resource = new FhirOperationOutcome
                    {
                        ResourceType = "OperationOutcome",
                        Issue = new List<OperationOutcomeIssue>
                        {
                            new OperationOutcomeIssue
                            {
                                Severity = "information",
                                Code = "informational",
                                Details = new IssueDetails { Text = "Scan completed successfully" }
                            }
                        }
                    }
                }
            }
        };

        // Act & Assert
        Assert.Equal(2, bundle.Entry.Count);
        Assert.IsType<FhirBinary>(bundle.Entry[0].Resource);
        Assert.IsType<FhirOperationOutcome>(bundle.Entry[1].Resource);
    }

    [Fact]
    public void FhirBundle_WhenSerialized_ShouldBeValidJson()
    {
        // Arrange
        var bundle = new FhirBundle
        {
            ResourceType = "Bundle",
            Type = "collection",
            Entry = new List<BundleEntry>
            {
                new BundleEntry
                {
                    Resource = new FhirBinary
                    {
                        ResourceType = "Binary",
                        ContentType = "image/png",
                        Data = "dGVzdA=="
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(bundle);
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
        var base64Data = Convert.ToBase64String(originalData);

        var binary = new FhirBinary
        {
            ResourceType = "Binary",
            ContentType = "image/png",
            Data = base64Data
        };

        // Act
        var decodedData = Convert.FromBase64String(binary.Data!);

        // Assert
        Assert.Equal(originalData, decodedData);
    }

    [Fact]
    public void FhirDevice_Properties_ShouldSupportQuantity()
    {
        // Arrange
        var device = new FhirDevice
        {
            ResourceType = "Device",
            Property = new List<DeviceProperty>
            {
                new DeviceProperty
                {
                    Type = new ApiCodeableConcept { Text = "Resolution" },
                    ValueQuantity = new List<ApiQuantity>
                    {
                        new ApiQuantity { Value = 300, Unit = "dpi" }
                    }
                }
            }
        };

        // Act & Assert
        Assert.Single(device.Property);
        Assert.Equal("Resolution", device.Property[0].Type?.Text);
        Assert.Single(device.Property[0].ValueQuantity);
        Assert.Equal(300, device.Property[0].ValueQuantity[0].Value);
        Assert.Equal("dpi", device.Property[0].ValueQuantity[0].Unit);
    }

    [Fact]
    public void OperationOutcome_ShouldSupportMultipleIssues()
    {
        // Arrange
        var outcome = new FhirOperationOutcome
        {
            ResourceType = "OperationOutcome",
            Issue = new List<OperationOutcomeIssue>
            {
                new OperationOutcomeIssue
                {
                    Severity = "warning",
                    Code = "processing",
                    Details = new IssueDetails { Text = "Calibration recommended" }
                },
                new OperationOutcomeIssue
                {
                    Severity = "information",
                    Code = "informational",
                    Details = new IssueDetails { Text = "Scan completed" }
                }
            }
        };

        // Act & Assert
        Assert.Equal(2, outcome.Issue.Count);
        Assert.Equal("warning", outcome.Issue[0].Severity);
        Assert.Equal("information", outcome.Issue[1].Severity);
    }

    [Fact]
    public void FhirBundle_ErrorScenario_ShouldOnlyContainOperationOutcome()
    {
        // Arrange
        var bundle = new FhirBundle
        {
            ResourceType = "Bundle",
            Type = "collection",
            Entry = new List<BundleEntry>
            {
                new BundleEntry
                {
                    Resource = new FhirOperationOutcome
                    {
                        ResourceType = "OperationOutcome",
                        Issue = new List<OperationOutcomeIssue>
                        {
                            new OperationOutcomeIssue
                            {
                                Severity = "error",
                                Code = "exception",
                                Details = new IssueDetails { Text = "Scanner not initialized" }
                            }
                        }
                    }
                }
            }
        };

        // Act & Assert
        Assert.Single(bundle.Entry);
        Assert.IsType<FhirOperationOutcome>(bundle.Entry[0].Resource);
        var outcome = (FhirOperationOutcome)bundle.Entry[0].Resource!;
        Assert.Equal("error", outcome.Issue[0].Severity);
    }
}
