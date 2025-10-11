# Multi-Scanner Architecture Guide

## Current Implementation

The current BFD9010 system is designed to work with a single scanner type (Vidar). The architecture consists of:

- **BFD9010.Scanner** - Shared library containing all Vidar scanner operations
- **BFD9010.Cli** - Command-line interface
- **BFD9010.FhirApi** - FHIR REST API server
- **BFD9010.Gui** - Windows Forms GUI application

## Future Multi-Scanner Support

When support for additional scanner types is needed, the recommended architecture is:

### Project Structure

```
BFD9010/
├── BFD9010.Scanner.Core/           # Common interfaces and base classes
│   ├── IScannerDriver.cs            # Scanner driver interface
│   ├── IScannerCapabilities.cs      # Scanner capabilities interface
│   ├── ScanResult.cs                # Common scan result types
│   └── ScanParameters.cs            # Common scan parameter types
│
├── BFD9010.Scanner.Vidar/          # Vidar-specific implementation
│   # This folder structure is flexible - scanner vendors can organize their code as needed
│   # as long as they implement IScannerDriver. Current Vidar implementation includes:
│   ├── VidarDriver.cs               # Implements IScannerDriver
│   ├── VscsiTypes.cs                # VSCSI-specific types
│   ├── VscsiMethods.cs              # VSCSI method bindings
│   ├── Calibrate.cs                 # Calibration logic
│   ├── Scan.cs                      # Scan logic
│   ├── Eject.cs                     # Film ejection
│   ├── Startup.cs                   # Scanner initialization
│   ├── ScanConfig.cs                # Configuration handling
│   ├── ScannerData.cs               # Scanner data structures
│   ├── DigitizeEngine.cs            # Digitization engine
│   ├── Hardware.cs                  # Hardware detection
│   └── Vscsi32.dll                  # Native driver library
│
├── BFD9010.Scanner.Xyz/            # Future scanner implementation
│   ├── XyzDriver.cs                 # Implements IScannerDriver
│   └── [Scanner-specific files]
│
├── BFD9010.Cli/                    # CLI application (scanner-agnostic)
│   └── Program.cs                   # Uses IScannerDriver
│
├── BFD9010.FhirApi/                # FHIR API (consumes IScannerDriver)
│   ├── Program.cs                   # API endpoints
│   └── Services/
│       └── ScannerService.cs        # Wraps IScannerDriver
│
└── BFD9010.Gui/                    # GUI application (entry point for FHIR API)
    └── MainForm.cs                  # Launches and hosts BFD9010.FhirApi
```

**Note:** The GUI is the entry point that launches the FHIR API server. The FHIR API consumes scanner drivers through IScannerDriver. The CLI also consumes scanner drivers directly.

### Scanner Driver Interface

```csharp
namespace BFD9010.Scanner.Core;

public interface IScannerDriver
{
    /// <summary>
    /// Initialize the scanner hardware
    /// </summary>
    Task<(int status, IScannerInformation? information)> InitializeAsync();
    
    /// <summary>
    /// Perform a scan operation
    /// </summary>
    Task<(int status, byte[]? imageData)> ScanAsync(ScanParameters parameters);
    
    /// <summary>
    /// Calibrate the scanner
    /// </summary>
    Task<int> CalibrateAsync();
    
    /// <summary>
    /// Eject media from scanner
    /// </summary>
    Task<int> EjectAsync();
    
    /// <summary>
    /// Get scanner information
    /// </summary>
    IScannerInformation? Information { get; }
}

public interface IScannerInformation
{
    string Manufacturer { get; }
    string Model { get; }
    string SerialNumber { get; }
    string FirmwareVersion { get; }
    int MaxResolutionDpi { get; }
    int MaxBitDepth { get; }
    float MaxWidthInches { get; }
}
```

### Scanner Selection

Scanner selection can be handled through:

1. **Configuration File** - `scanner.json`:
```json
{
  "ScannerType": "Vidar",  // or "Xyz"
  "DeviceId": "scanner-001"
}
```

2. **Dependency Injection** - In `Program.cs`:
```csharp
// CLI or API startup
var scannerType = config.GetValue<string>("ScannerType");
services.AddSingleton<IScannerDriver>(sp =>
{
    return scannerType switch
    {
        "Vidar" => new VidarDriver(),
        "Xyz" => new XyzDriver(),
        _ => throw new NotSupportedException($"Scanner type '{scannerType}' not supported")
    };
});
```

3. **Auto-Detection** - Scan USB devices and automatically detect scanner type:
```csharp
public class ScannerDetector
{
    public IScannerDriver? DetectScanner()
    {
        // Try Vidar
        var vidar = new VidarDriver();
        if (vidar.TryConnect()) return vidar;
        
        // Try Xyz
        var xyz = new XyzDriver();
        if (xyz.TryConnect()) return xyz;
        
        return null;
    }
}
```

## Migration Path

To migrate the current codebase to support multiple scanners:

### Phase 1: Extract Interface
1. Create `BFD9010.Scanner.Core` project
2. Define `IScannerDriver` and `IScannerCapabilities` interfaces
3. Document common scan parameters and results

### Phase 2: Refactor Vidar Implementation
1. Rename `BFD9010.Scanner` → `BFD9010.Scanner.Vidar`
2. Create `VidarDriver` class implementing `IScannerDriver`
3. Move all Vidar-specific code into the driver class
4. Update namespaces and references

### Phase 3: Update Consumers
1. Update `BFD9010.Cli` to use `IScannerDriver` interface
2. Update `BFD9010.FhirApi` Services to use `IScannerDriver` interface
3. Note: `BFD9010.Gui` launches the FHIR API server - it doesn't directly consume scanners
4. Add scanner selection logic (config or auto-detect)

### Phase 4: Add New Scanner Support
1. Create `BFD9010.Scanner.Xyz` project
2. Implement `IScannerDriver` interface for new scanner
3. Add scanner-specific native libraries/drivers
4. Update configuration to support new scanner type

## Benefits of This Architecture

1. **Separation of Concerns** - Scanner-specific code is isolated
2. **Easy Testing** - Mock scanner drivers for unit tests
3. **Extensibility** - Add new scanners without modifying existing code
4. **Maintainability** - Clear boundaries between components
5. **Reusability** - CLI, API, and GUI can work with any scanner

## Current Code Organization

For now, the current flat structure works well for a single scanner. The migration should only be done when:
- A second scanner type needs to be supported
- The team has capacity for the refactoring effort
- Requirements are clear for what the common interface should look like

## Testing Strategy

### Unit Tests
- Test scanner drivers in isolation using mock USB/SCSI interfaces
- Test FHIR API endpoints with mock scanner drivers
- Test CLI commands with mock scanner drivers

### Integration Tests
- Test actual scanner hardware with real drivers
- Test FHIR compliance using Firely SDK validation
- Test end-to-end workflows

### Current Test Coverage
- FHIR resource structure validation
- Bundle format compliance
- Base64 encoding/decoding
- Error handling scenarios

## References

- [FHIR Device Resource](https://www.hl7.org/fhir/device.html)
- [FHIR Operations Framework](https://www.hl7.org/fhir/operations.html)
- [Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Strategy Pattern](https://refactoring.guru/design-patterns/strategy)
