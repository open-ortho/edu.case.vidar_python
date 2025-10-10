using BFD9010.FhirApi.Models;
using BFD9010.FhirApi.Services;
using BFD9010.Scanner;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BFD9010.FhirApi;

/// <summary>
/// Shared configuration for FHIR API server that can be used by both CLI and GUI applications
/// </summary>
public static class FhirServerConfiguration
{
    /// <summary>
    /// Configure services for the FHIR API server
    /// </summary>
    public static void ConfigureServices(WebApplicationBuilder builder, ScanConfig? config = null)
    {
        // Add services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<ScannerService>();

        // Get CORS origins from config or use default
        string corsOrigins = config?.CorsOrigin ?? "https://wingate.case.edu";
        var allowedOrigins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Configure CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("WingatePolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
    }

    /// <summary>
    /// Configure the HTTP request pipeline and register FHIR endpoints
    /// </summary>
    public static void ConfigureEndpoints(WebApplication app)
    {
        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("WingatePolicy");

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
                    },
                    new DeviceProperty
                    {
                        Type = new CodeableConcept { Text = "Max Films" },
                        ValueQuantity = new List<Quantity> { new Quantity { Value = data.maxFilms, Unit = "films" } }
                    },
                    new DeviceProperty
                    {
                        Type = new CodeableConcept { Text = "Dark Enhance" },
                        ValueCodeableConcept = new List<CodeableConcept> { new CodeableConcept { Text = data.darkEnhance } }
                    },
                    new DeviceProperty
                    {
                        Type = new CodeableConcept { Text = "Line Filter" },
                        ValueCodeableConcept = new List<CodeableConcept> { new CodeableConcept { Text = data.lineFilter } }
                    },
                    new DeviceProperty
                    {
                        Type = new CodeableConcept { Text = "Film Backup" },
                        ValueCodeableConcept = new List<CodeableConcept> { new CodeableConcept { Text = data.filmBackup } }
                    },
                    new DeviceProperty
                    {
                        Type = new CodeableConcept { Text = "Unload Medium" },
                        ValueCodeableConcept = new List<CodeableConcept> { new CodeableConcept { Text = data.unloadMedium } }
                    },
                    new DeviceProperty
                    {
                        Type = new CodeableConcept { Text = "Limited Scans" },
                        ValueCodeableConcept = new List<CodeableConcept> { new CodeableConcept { Text = data.limitedScans } }
                    },
                    new DeviceProperty
                    {
                        Type = new CodeableConcept { Text = "Line Time" },
                        ValueCodeableConcept = new List<CodeableConcept> { new CodeableConcept { Text = data.lineTime } }
                    },
                    new DeviceProperty
                    {
                        Type = new CodeableConcept { Text = "Feeder Type" },
                        ValueCodeableConcept = new List<CodeableConcept> { new CodeableConcept { Text = data.feederType } }
                    },
                    new DeviceProperty
                    {
                        Type = new CodeableConcept { Text = "Lamp Type" },
                        ValueCodeableConcept = new List<CodeableConcept> { new CodeableConcept { Text = data.lampType } }
                    },
                    new DeviceProperty
                    {
                        Type = new CodeableConcept { Text = "Translation Table" },
                        ValueCodeableConcept = new List<CodeableConcept> { new CodeableConcept { Text = data.translationTable } }
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
            var (status, imageBytes, errorMessage) = await scanner.ScanAsync();

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
                // Failure - add error OperationOutcome with detailed message
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
                                Details = new IssueDetails { Text = errorMessage ?? $"Scan failed with status code: {status}" }
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

            var (status, errorMessage) = await scanner.CalibrateAsync();

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
                            Details = new IssueDetails { Text = errorMessage ?? $"Calibration failed with status code: {status}" }
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

            var (status, errorMessage) = await scanner.EjectAsync();

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
                            Details = new IssueDetails { Text = errorMessage ?? $"Eject failed with status code: {status}" }
                        }
                    }
                }, statusCode: 500);
            }
        })
        .WithName("EjectFilm")
        .WithOpenApi();
    }

    /// <summary>
    /// Initialize the scanner service on startup
    /// </summary>
    public static async Task<bool> InitializeScannerAsync(WebApplication app)
    {
        var scannerService = app.Services.GetRequiredService<ScannerService>();
        var status = await scannerService.InitializeAsync();
        return status == 0;
    }
}
