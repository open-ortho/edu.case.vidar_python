using BFD9010.FhirApi.Services;
using BFD9010.Scanner;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
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
    private static readonly FhirJsonSerializer _fhirSerializer = new(new SerializerSettings
    {
        Pretty = true
    });

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
                var errorOutcome = new OperationOutcome
                {
                    Text = new Narrative
                    {
                        Status = Narrative.NarrativeStatus.Generated,
                        Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Scanner not initialized</p></div>"
                    },
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Code = OperationOutcome.IssueType.NotFound,
                            Details = new CodeableConcept { Text = "Scanner not initialized" }
                        }
                    }
                };

                return Results.Content(_fhirSerializer.SerializeToString(errorOutcome), "application/fhir+json", statusCode: 500);
            }

            var data = scanner.ScannerData.Value;
            var device = new Device
            {
                Id = id,
                Manufacturer = "Vidar Systems Corporation",
                ModelNumber = data.modelName,
                SerialNumber = data.serialNumber,
                Text = new Narrative
                {
                    Status = Narrative.NarrativeStatus.Generated,
                    Div = $"<div xmlns=\"http://www.w3.org/1999/xhtml\"><p><b>Scanner Device</b></p>" +
                          $"<p>Manufacturer: Vidar Systems Corporation</p>" +
                          $"<p>Model: {data.modelName}</p>" +
                          $"<p>Serial Number: {data.serialNumber}</p>" +
                          $"<p>Firmware: {data.firmwareVersionNumber}</p>" +
                          $"<p>Resolution: {data.currentResolution} dpi</p></div>"
                }
            };

            // Add firmware version
            device.Version.Add(new Device.VersionComponent
            {
                Type = new CodeableConcept { Text = "Firmware" },
                Value = data.firmwareVersionNumber
            });

            // Add hardware version
            device.Version.Add(new Device.VersionComponent
            {
                Type = new CodeableConcept { Text = "Hardware" },
                Value = data.hardwareVersionNumber.ToString()
            });

            // Add properties with quantities
            device.Property.Add(new Device.PropertyComponent
            {
                Type = new CodeableConcept { Text = "Current Resolution" },
                Value = new Quantity((decimal)data.currentResolution, "dpi")
            });

            device.Property.Add(new Device.PropertyComponent
            {
                Type = new CodeableConcept { Text = "Optical Resolution" },
                Value = new Quantity((decimal)data.opticalResolution, "dpi")
            });

            device.Property.Add(new Device.PropertyComponent
            {
                Type = new CodeableConcept { Text = "Max Width" },
                Value = new Quantity((decimal)data.maxWidthInInches, "[in_us]")
            });

            device.Property.Add(new Device.PropertyComponent
            {
                Type = new CodeableConcept { Text = "Current Bit Depth" },
                Value = new Quantity(data.currentBitDepth, "bits")
            });

            device.Property.Add(new Device.PropertyComponent
            {
                Type = new CodeableConcept { Text = "Max Films" },
                Value = new Quantity(data.maxFilms, "films")
            });

            // Add string properties only if they have values
            if (!string.IsNullOrEmpty(data.darkEnhance))
            {
                device.Property.Add(new Device.PropertyComponent
                {
                    Type = new CodeableConcept { Text = "Dark Enhance" },
                    Value = new CodeableConcept { Text = data.darkEnhance }
                });
            }

            if (!string.IsNullOrEmpty(data.lineFilter))
            {
                device.Property.Add(new Device.PropertyComponent
                {
                    Type = new CodeableConcept { Text = "Line Filter" },
                    Value = new CodeableConcept { Text = data.lineFilter }
                });
            }

            if (!string.IsNullOrEmpty(data.filmBackup))
            {
                device.Property.Add(new Device.PropertyComponent
                {
                    Type = new CodeableConcept { Text = "Film Backup" },
                    Value = new CodeableConcept { Text = data.filmBackup }
                });
            }

            if (!string.IsNullOrEmpty(data.unloadMedium))
            {
                device.Property.Add(new Device.PropertyComponent
                {
                    Type = new CodeableConcept { Text = "Unload Medium" },
                    Value = new CodeableConcept { Text = data.unloadMedium }
                });
            }

            if (!string.IsNullOrEmpty(data.limitedScans))
            {
                device.Property.Add(new Device.PropertyComponent
                {
                    Type = new CodeableConcept { Text = "Limited Scans" },
                    Value = new CodeableConcept { Text = data.limitedScans }
                });
            }

            if (!string.IsNullOrEmpty(data.lineTime))
            {
                device.Property.Add(new Device.PropertyComponent
                {
                    Type = new CodeableConcept { Text = "Line Time" },
                    Value = new CodeableConcept { Text = data.lineTime }
                });
            }

            if (!string.IsNullOrEmpty(data.feederType))
            {
                device.Property.Add(new Device.PropertyComponent
                {
                    Type = new CodeableConcept { Text = "Feeder Type" },
                    Value = new CodeableConcept { Text = data.feederType }
                });
            }

            if (!string.IsNullOrEmpty(data.lampType))
            {
                device.Property.Add(new Device.PropertyComponent
                {
                    Type = new CodeableConcept { Text = "Lamp Type" },
                    Value = new CodeableConcept { Text = data.lampType }
                });
            }

            if (!string.IsNullOrEmpty(data.translationTable))
            {
                device.Property.Add(new Device.PropertyComponent
                {
                    Type = new CodeableConcept { Text = "Translation Table" },
                    Value = new CodeableConcept { Text = data.translationTable }
                });
            }

            return Results.Content(_fhirSerializer.SerializeToString(device), "application/fhir+json");
        })
        .WithName("GetDevice")
        .WithOpenApi();

        // FHIR $scan operation - performs scan and returns Bundle with Binary and OperationOutcome
        app.MapPost("/Device/{id}/$scan", async (string id, [FromServices] ScannerService scanner) =>
        {
            app.Logger.LogInformation("Device/{Id}/$scan endpoint called", id);

            if (!scanner.IsInitialized)
            {
                var errorOutcome = new OperationOutcome
                {
                    Text = new Narrative
                    {
                        Status = Narrative.NarrativeStatus.Generated,
                        Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Scanner not initialized</p></div>"
                    },
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Code = OperationOutcome.IssueType.NotFound,
                            Details = new CodeableConcept { Text = "Scanner not initialized" }
                        }
                    }
                };

                var errorBundle = new Bundle
                {
                    Type = Bundle.BundleType.Collection
                };
                errorBundle.Entry.Add(new Bundle.EntryComponent 
                { 
                    FullUrl = $"urn:uuid:{Guid.NewGuid()}",
                    Resource = errorOutcome 
                });

                return Results.Content(_fhirSerializer.SerializeToString(errorBundle), "application/fhir+json", statusCode: 500);
            }

            // Perform the scan
            var (status, imageBytes, errorMessage) = await scanner.ScanAsync();

            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Collection
            };

            if (status == 0 && imageBytes != null)
            {
                // Success - add Binary resource with base64-encoded image
                var binary = new Binary
                {
                    ContentType = "image/png",
                    Data = imageBytes
                };
                bundle.Entry.Add(new Bundle.EntryComponent 
                { 
                    FullUrl = $"urn:uuid:{Guid.NewGuid()}",
                    Resource = binary 
                });

                // Add successful OperationOutcome
                var successOutcome = new OperationOutcome
                {
                    Text = new Narrative
                    {
                        Status = Narrative.NarrativeStatus.Generated,
                        Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Scan completed successfully</p></div>"
                    },
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
                bundle.Entry.Add(new Bundle.EntryComponent 
                { 
                    FullUrl = $"urn:uuid:{Guid.NewGuid()}",
                    Resource = successOutcome 
                });

                return Results.Content(_fhirSerializer.SerializeToString(bundle), "application/fhir+json");
            }
            else
            {
                // Failure - add error OperationOutcome with detailed message
                var failureOutcome = new OperationOutcome
                {
                    Text = new Narrative
                    {
                        Status = Narrative.NarrativeStatus.Generated,
                        Div = $"<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Scan failed: {errorMessage ?? $"Status code {status}"}</p></div>"
                    },
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Code = OperationOutcome.IssueType.Exception,
                            Details = new CodeableConcept { Text = errorMessage ?? $"Scan failed with status code: {status}" }
                        }
                    }
                };
                bundle.Entry.Add(new Bundle.EntryComponent 
                { 
                    FullUrl = $"urn:uuid:{Guid.NewGuid()}",
                    Resource = failureOutcome 
                });

                return Results.Content(_fhirSerializer.SerializeToString(bundle), "application/fhir+json", statusCode: 500);
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
                var errorOutcome = new OperationOutcome
                {
                    Text = new Narrative
                    {
                        Status = Narrative.NarrativeStatus.Generated,
                        Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Scanner not initialized</p></div>"
                    },
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Code = OperationOutcome.IssueType.NotFound,
                            Details = new CodeableConcept { Text = "Scanner not initialized" }
                        }
                    }
                };

                return Results.Content(_fhirSerializer.SerializeToString(errorOutcome), "application/fhir+json", statusCode: 500);
            }

            var (status, errorMessage) = await scanner.CalibrateAsync();

            OperationOutcome outcome;
            int statusCode = 200;

            if (status == 0)
            {
                outcome = new OperationOutcome
                {
                    Text = new Narrative
                    {
                        Status = Narrative.NarrativeStatus.Generated,
                        Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Calibration completed successfully</p></div>"
                    },
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Information,
                            Code = OperationOutcome.IssueType.Informational,
                            Details = new CodeableConcept { Text = "Calibration completed successfully" }
                        }
                    }
                };
            }
            else
            {
                outcome = new OperationOutcome
                {
                    Text = new Narrative
                    {
                        Status = Narrative.NarrativeStatus.Generated,
                        Div = $"<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Calibration failed: {errorMessage ?? $"Status code {status}"}</p></div>"
                    },
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Code = OperationOutcome.IssueType.Exception,
                            Details = new CodeableConcept { Text = errorMessage ?? $"Calibration failed with status code: {status}" }
                        }
                    }
                };
                statusCode = 500;
            }

            return Results.Content(_fhirSerializer.SerializeToString(outcome), "application/fhir+json", statusCode: statusCode);
        })
        .WithName("CalibrateDevice")
        .WithOpenApi();

        // Optional: Eject endpoint
        app.MapPost("/Device/{id}/$eject", async (string id, [FromServices] ScannerService scanner) =>
        {
            app.Logger.LogInformation("Device/{Id}/$eject endpoint called", id);

            if (!scanner.IsInitialized)
            {
                var errorOutcome = new OperationOutcome
                {
                    Text = new Narrative
                    {
                        Status = Narrative.NarrativeStatus.Generated,
                        Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Scanner not initialized</p></div>"
                    },
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Code = OperationOutcome.IssueType.NotFound,
                            Details = new CodeableConcept { Text = "Scanner not initialized" }
                        }
                    }
                };

                return Results.Content(_fhirSerializer.SerializeToString(errorOutcome), "application/fhir+json", statusCode: 500);
            }

            var (status, errorMessage) = await scanner.EjectAsync();

            OperationOutcome outcome;
            int statusCode = 200;

            if (status == 0)
            {
                outcome = new OperationOutcome
                {
                    Text = new Narrative
                    {
                        Status = Narrative.NarrativeStatus.Generated,
                        Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Film ejected successfully</p></div>"
                    },
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Information,
                            Code = OperationOutcome.IssueType.Informational,
                            Details = new CodeableConcept { Text = "Film ejected successfully" }
                        }
                    }
                };
            }
            else
            {
                outcome = new OperationOutcome
                {
                    Text = new Narrative
                    {
                        Status = Narrative.NarrativeStatus.Generated,
                        Div = $"<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Eject failed: {errorMessage ?? $"Status code {status}"}</p></div>"
                    },
                    Issue = new List<OperationOutcome.IssueComponent>
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Code = OperationOutcome.IssueType.Exception,
                            Details = new CodeableConcept { Text = errorMessage ?? $"Eject failed with status code: {status}" }
                        }
                    }
                };
                statusCode = 500;
            }

            return Results.Content(_fhirSerializer.SerializeToString(outcome), "application/fhir+json", statusCode: statusCode);
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
