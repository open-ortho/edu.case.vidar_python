# BFD9010 FHIR API

This directory contains the FHIR-compliant REST API for the BFD9010 scanner system.

## Architecture

The project is organized into three main components:

1. **BFD9010.Scanner** - Shared library containing all scanner operations
2. **bfd9010** - Command-line interface application
3. **BFD9010.FhirApi** - FHIR REST API server
4. **BFD9010.Gui** - Windows Forms GUI application (Windows only)

## Building

### Prerequisites
- .NET 8.0 SDK
- Windows OS (for GUI and scanner hardware access)

### Build Commands

```bash
# Build all projects
dotnet build vidar_app.sln --configuration Release

# Build individual projects
dotnet build BFD9010.Scanner/BFD9010.Scanner.csproj --configuration Release
dotnet build BFD9010.FhirApi/BFD9010.FhirApi.csproj --configuration Release
dotnet build vidar_app/vidar_app.csproj --configuration Release
```

### Output Locations
- CLI: `vidar_app/bin/Release/net8.0/bfd9010.exe`
- FHIR API: `BFD9010.FhirApi/bin/Release/net8.0/BFD9010.FhirApi.exe`
- GUI: `BFD9010.Gui/bin/Release/net8.0-windows/bfd9010_fhir32.exe`

## Running

### CLI Application
```bash
cd vidar_app/bin/Release/net8.0
./bfd9010.exe
```

### FHIR API Server
```bash
cd BFD9010.FhirApi/bin/Release/net8.0
./BFD9010.FhirApi.exe
```

The API will start on `http://localhost:5000`

### GUI Application (Windows only)
```bash
cd BFD9010.Gui/bin/Release/net8.0-windows
./bfd9010_fhir32.exe
```

The GUI will:
- Initialize the scanner on startup
- Start the FHIR API server on port 5000
- Display scanner status in a small always-on-top window
- Show the message: "Go to https://wingate.case.edu/bfd9000/ to scan"

## FHIR API Endpoints

### Base URL
```
http://localhost:5000
```

### 1. Get Device Information
**Endpoint:** `GET /Device/{id}`

Returns scanner information as a FHIR Device resource.

**Parameters:**
- `id` - Device identifier (any string, logged for future multi-scanner support)

**Response:** FHIR Device resource

**Example Request:**
```bash
curl http://localhost:5000/Device/scanner-001
```

**Example Response:**
```json
{
  "resourceType": "Device",
  "id": "scanner-001",
  "manufacturer": "Vidar Systems Corporation",
  "modelNumber": "VXR-16 DosimetryPRO",
  "serialNumber": "ABC123",
  "version": [
    {
      "type": { "text": "Firmware" },
      "value": "2.01"
    },
    {
      "type": { "text": "Hardware" },
      "value": "3"
    }
  ],
  "property": [
    {
      "type": { "text": "Current Resolution" },
      "valueQuantity": [{ "value": 75, "unit": "dpi" }]
    },
    {
      "type": { "text": "Optical Resolution" },
      "valueQuantity": [{ "value": 300, "unit": "dpi" }]
    }
  ]
}
```

### 2. Scan Operation
**Endpoint:** `POST /Device/{id}/$scan`

Performs a scan and returns the image as a FHIR Bundle containing a Binary resource (base64-encoded PNG) and an OperationOutcome.

**Parameters:**
- `id` - Device identifier (any string)

**Response:** FHIR Bundle with Binary and OperationOutcome resources

**Example Request:**
```bash
curl -X POST http://localhost:5000/Device/scanner-001/\$scan
```

**Example Response (Success):**
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "resource": {
        "resourceType": "Binary",
        "contentType": "image/png",
        "data": "iVBORw0KGgoAAAANSUhEUgAA..."
      }
    },
    {
      "resource": {
        "resourceType": "OperationOutcome",
        "issue": [
          {
            "severity": "information",
            "code": "informational",
            "details": {
              "text": "Scan completed successfully"
            }
          }
        ]
      }
    }
  ]
}
```

**Example Response (Error):**
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "resource": {
        "resourceType": "OperationOutcome",
        "issue": [
          {
            "severity": "error",
            "code": "exception",
            "details": {
              "text": "Scan failed with status code: 8"
            }
          }
        ]
      }
    }
  ]
}
```

### 3. Calibrate Operation
**Endpoint:** `POST /Device/{id}/$calibrate`

Calibrates the scanner.

**Parameters:**
- `id` - Device identifier (any string)

**Response:** FHIR OperationOutcome

**Example Request:**
```bash
curl -X POST http://localhost:5000/Device/scanner-001/\$calibrate
```

**Example Response:**
```json
{
  "resourceType": "OperationOutcome",
  "issue": [
    {
      "severity": "information",
      "code": "informational",
      "details": {
        "text": "Calibration completed successfully"
      }
    }
  ]
}
```

### 4. Eject Film Operation
**Endpoint:** `POST /Device/{id}/$eject`

Ejects the film from the scanner.

**Parameters:**
- `id` - Device identifier (any string)

**Response:** FHIR OperationOutcome

**Example Request:**
```bash
curl -X POST http://localhost:5000/Device/scanner-001/\$eject
```

**Example Response:**
```json
{
  "resourceType": "OperationOutcome",
  "issue": [
    {
      "severity": "information",
      "code": "informational",
      "details": {
        "text": "Film ejected successfully"
      }
    }
  ]
}
```

## CORS Configuration

The API is configured to allow requests from:
- `https://wingate.case.edu`

This allows the web application at wingate.case.edu to call the local API.

## Scanner Configuration

Scanner settings are loaded from `scan_config.ini` in the working directory. If the file doesn't exist, default values will be used.

Example `scan_config.ini`:
```ini
[ScanParameters]
Offset0_BitDepth=12
Offset2_DPI_X=75
# ... additional parameters
```

## Image Output

Images are returned as:
- **Format:** PNG
- **Encoding:** Base64 (in FHIR Binary resource)
- **Bit Depth:** Configurable (8-bit or 16-bit grayscale)

## Error Handling

All errors are returned as FHIR OperationOutcome resources with appropriate severity levels:
- `information` - Successful operation
- `error` - Operation failed
- `fatal` - Critical error

HTTP status codes:
- `200 OK` - Successful operation
- `500 Internal Server Error` - Operation failed

## Swagger/OpenAPI

When running in development mode, Swagger UI is available at:
```
http://localhost:5000/swagger
```

This provides interactive API documentation.

## Integration with wingate.case.edu

The expected workflow is:

1. User runs the BFD9010 GUI application (or FHIR API server) on their local machine
2. The application initializes the scanner and starts the API on `http://localhost:5000`
3. User navigates to `https://wingate.case.edu/bfd9000/` in their browser
4. The web application makes requests to the local API to control the scanner
5. Scanned images are returned as base64-encoded PNG data in FHIR Bundle responses

## Future Enhancements

- Support for multiple scanners
- WebSocket notifications for scan progress
- Additional image formats (TIFF, DICOM)
- Authentication/authorization
- Database storage for scan history
- Network deployment (currently localhost only)

## Troubleshooting

### Scanner Not Detected
- Ensure the scanner is powered on and connected via USB
- Check that the Vscsi32.dll is in the application directory
- Try running the CLI application first to test scanner connectivity

### CORS Errors
- Verify you're accessing from `https://wingate.case.edu`
- Check browser console for CORS error details
- Ensure the API is running on `http://localhost:5000`

### Build Errors on Linux
- The GUI project (BFD9010.Gui) requires Windows
- Remove it from the solution if building on Linux:
  ```bash
  dotnet sln remove BFD9010.Gui/BFD9010.Gui.csproj
  ```

## License

See the main repository LICENSE file.
