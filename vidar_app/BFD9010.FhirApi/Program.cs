using BFD9010.FhirApi.Models;
using BFD9010.FhirApi.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ScannerService>();

// Configure CORS for wingate.case.edu
builder.Services.AddCors(options =>
{
    options.AddPolicy("WingatePolicy", policy =>
    {
        policy.WithOrigins("https://wingate.case.edu")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("WingatePolicy");

// Initialize scanner on startup
var scannerService = app.Services.GetRequiredService<ScannerService>();
var initTask = scannerService.InitializeAsync();
initTask.Wait(); // Block startup until scanner is initialized

// FHIR Device endpoint - returns scanner information
app.MapGet("/Device/{id}", (string id, [FromServices] ScannerService scanner) =>
{
    app.Logger.LogInformation("Device/{Id} endpoint called", id);
    
    if (!scanner.IsInitialized || scanner.ScannerData == null)
    {
        return Results.Json(new FhirOperationOutcome
        {
            Issue = new List<OperationOutcomeIssue>
            {
                new OperationOutcomeIssue
                {
                    Severity = "error",
                    Code = "not-found",
                    Details = new IssueDetails { Text = "Scanner not initialized" }
                }
            }
        }, statusCode: 500);
    }

    var data = scanner.ScannerData.Value;
    var device = new FhirDevice
    {
        Id = id,
        Manufacturer = "Vidar Systems Corporation",
        ModelNumber = data.modelName,
        SerialNumber = data.serialNumber,
        Version = new List<DeviceVersion>
        {
            new DeviceVersion
            {
                Type = new CodeableConcept { Text = "Firmware" },
                Value = data.firmwareVersionNumber
            },
            new DeviceVersion
            {
                Type = new CodeableConcept { Text = "Hardware" },
                Value = data.hardwareVersionNumber.ToString()
            }
        },
        Property = new List<DeviceProperty>
        {
            new DeviceProperty
            {
                Type = new CodeableConcept { Text = "Current Resolution" },
                ValueQuantity = new List<Quantity> { new Quantity { Value = data.currentResolution, Unit = "dpi" } }
            },
            new DeviceProperty
            {
                Type = new CodeableConcept { Text = "Optical Resolution" },
                ValueQuantity = new List<Quantity> { new Quantity { Value = data.opticalResolution, Unit = "dpi" } }
            },
            new DeviceProperty
            {
                Type = new CodeableConcept { Text = "Max Width" },
                ValueQuantity = new List<Quantity> { new Quantity { Value = (decimal)data.maxWidthInInches, Unit = "inches" } }
            },
            new DeviceProperty
            {
                Type = new CodeableConcept { Text = "Current Bit Depth" },
                ValueQuantity = new List<Quantity> { new Quantity { Value = data.currentBitDepth, Unit = "bits" } }
            }
        }
    };

    return Results.Json(device);
})
.WithName("GetDevice")
.WithOpenApi();

// FHIR $scan operation - performs scan and returns Bundle with Binary and OperationOutcome
app.MapPost("/Device/{id}/$scan", async (string id, [FromServices] ScannerService scanner) =>
{
    app.Logger.LogInformation("Device/{Id}/$scan endpoint called", id);

    if (!scanner.IsInitialized)
    {
        var errorOutcome = new FhirOperationOutcome
        {
            Issue = new List<OperationOutcomeIssue>
            {
                new OperationOutcomeIssue
                {
                    Severity = "error",
                    Code = "not-found",
                    Details = new IssueDetails { Text = "Scanner not initialized" }
                }
            }
        };

        return Results.Json(new FhirBundle
        {
            Entry = new List<BundleEntry>
            {
                new BundleEntry { Resource = errorOutcome }
            }
        }, statusCode: 500);
    }

    // Perform the scan
    var (status, imageBytes) = await scanner.ScanAsync();

    var bundle = new FhirBundle();

    if (status == 0 && imageBytes != null)
    {
        // Success - add Binary resource with base64-encoded image
        bundle.Entry.Add(new BundleEntry
        {
            Resource = new FhirBinary
            {
                ContentType = "image/png",
                Data = Convert.ToBase64String(imageBytes)
            }
        });

        // Add successful OperationOutcome
        bundle.Entry.Add(new BundleEntry
        {
            Resource = new FhirOperationOutcome
            {
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
        });

        return Results.Json(bundle);
    }
    else
    {
        // Failure - add error OperationOutcome
        bundle.Entry.Add(new BundleEntry
        {
            Resource = new FhirOperationOutcome
            {
                Issue = new List<OperationOutcomeIssue>
                {
                    new OperationOutcomeIssue
                    {
                        Severity = "error",
                        Code = "exception",
                        Details = new IssueDetails { Text = $"Scan failed with status code: {status}" }
                    }
                }
            }
        });

        return Results.Json(bundle, statusCode: 500);
    }
})
.WithName("ScanDevice")
.WithOpenApi();

// Optional: Calibrate endpoint
app.MapPost("/Device/{id}/$calibrate", async (string id, [FromServices] ScannerService scanner) =>
{
    app.Logger.LogInformation("Device/{Id}/$calibrate endpoint called", id);

    if (!scanner.IsInitialized)
    {
        return Results.Json(new FhirOperationOutcome
        {
            Issue = new List<OperationOutcomeIssue>
            {
                new OperationOutcomeIssue
                {
                    Severity = "error",
                    Code = "not-found",
                    Details = new IssueDetails { Text = "Scanner not initialized" }
                }
            }
        }, statusCode: 500);
    }

    int status = await scanner.CalibrateAsync();

    if (status == 0)
    {
        return Results.Json(new FhirOperationOutcome
        {
            Issue = new List<OperationOutcomeIssue>
            {
                new OperationOutcomeIssue
                {
                    Severity = "information",
                    Code = "informational",
                    Details = new IssueDetails { Text = "Calibration completed successfully" }
                }
            }
        });
    }
    else
    {
        return Results.Json(new FhirOperationOutcome
        {
            Issue = new List<OperationOutcomeIssue>
            {
                new OperationOutcomeIssue
                {
                    Severity = "error",
                    Code = "exception",
                    Details = new IssueDetails { Text = $"Calibration failed with status code: {status}" }
                }
            }
        }, statusCode: 500);
    }
})
.WithName("CalibrateDevice")
.WithOpenApi();

// Optional: Eject endpoint
app.MapPost("/Device/{id}/$eject", async (string id, [FromServices] ScannerService scanner) =>
{
    app.Logger.LogInformation("Device/{Id}/$eject endpoint called", id);

    if (!scanner.IsInitialized)
    {
        return Results.Json(new FhirOperationOutcome
        {
            Issue = new List<OperationOutcomeIssue>
            {
                new OperationOutcomeIssue
                {
                    Severity = "error",
                    Code = "not-found",
                    Details = new IssueDetails { Text = "Scanner not initialized" }
                }
            }
        }, statusCode: 500);
    }

    int status = await scanner.EjectAsync();

    if (status == 0)
    {
        return Results.Json(new FhirOperationOutcome
        {
            Issue = new List<OperationOutcomeIssue>
            {
                new OperationOutcomeIssue
                {
                    Severity = "information",
                    Code = "informational",
                    Details = new IssueDetails { Text = "Film ejected successfully" }
                }
            }
        });
    }
    else
    {
        return Results.Json(new FhirOperationOutcome
        {
            Issue = new List<OperationOutcomeIssue>
            {
                new OperationOutcomeIssue
                {
                    Severity = "error",
                    Code = "exception",
                    Details = new IssueDetails { Text = $"Eject failed with status code: {status}" }
                }
            }
        }, statusCode: 500);
    }
})
.WithName("EjectFilm")
.WithOpenApi();

app.Run();
