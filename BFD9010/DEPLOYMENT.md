# BFD9010 Deployment and Testing Guide

## Overview

This guide covers how to deploy and test the BFD9010 scanner software on a Windows machine with the Vidar scanner connected.

## System Requirements

### Hardware
- Vidar Dosimetry Pro scanner (or compatible model)
- USB connection to scanner
- Windows 10 or later

### Software
- .NET 8.0 Runtime (or SDK for development)
- Vidar scanner drivers (if needed for initial USB recognition)

## Quick Start

### For End Users

1. **Download** the latest release from the GitHub releases page
2. **Extract** the ZIP file to a folder (e.g., `C:\BFD9010\`)
3. **Connect** the scanner via USB and power it on
4. **Run** `start-gui.bat`
5. **Wait** for the scanner to initialize (window will show status)
6. **Navigate** to `https://wingate.case.edu/bfd9000/` in your browser
7. **Scan** using the web interface

### For Developers

1. **Clone** the repository
2. **Install** .NET 8.0 SDK
3. **Build** using `build.bat` or `dotnet build BFD9010.sln --configuration Release`
4. **Run** one of the applications using the start scripts

## Deployment Options

### Option 1: GUI Application (Recommended for End Users)

**File:** `bfd9010_fhir32.exe`

**Features:**
- Always-on-top status window
- Automatic scanner initialization
- Integrated FHIR API server
- Visual feedback for scan operations

**How to Run:**
```batch
cd C:\BFD9010\BFD9010
start-gui.bat
```

Or directly:
```batch
BFD9010.Gui\bin\Release\net8.0-windows\bfd9010_fhir32.exe
```

### Option 2: API Server Only

**File:** `BFD9010.FhirApi.exe`

**Features:**
- Standalone FHIR API server
- No GUI (runs in console)
- Suitable for server deployments

**How to Run:**
```batch
cd C:\BFD9010\BFD9010
start-fhir-api.bat
```

Or directly:
```batch
BFD9010.FhirApi\bin\Release\net8.0\BFD9010.FhirApi.exe
```

### Option 3: CLI Application

**File:** `bfd9010.exe`

**Features:**
- Interactive command-line interface
- Direct scanner control
- No network API

**How to Run:**
```batch
cd C:\BFD9010\BFD9010
start-cli.bat
```

Or directly:
```batch
BFD9010.Cli\bin\Release\net8.0\bfd9010.exe
```

## Testing the API

### Prerequisites
- Scanner connected and powered on
- API server running (GUI or standalone)
- API accessible at `http://localhost:5000`

### Test 1: Get Scanner Information

```bash
curl http://localhost:5000/Device/test-001
```

**Expected Response:**
```json
{
  "resourceType": "Device",
  "id": "test-001",
  "manufacturer": "Vidar Systems Corporation",
  "modelNumber": "...",
  "serialNumber": "...",
  "version": [...]
}
```

### Test 2: Perform a Scan

```bash
curl -X POST http://localhost:5000/Device/test-001/$scan
```

**Expected Response:**
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "resource": {
        "resourceType": "Binary",
        "contentType": "image/png",
        "data": "iVBORw0KG..."
      }
    },
    {
      "resource": {
        "resourceType": "OperationOutcome",
        "issue": [{
          "severity": "information",
          "code": "informational",
          "details": { "text": "Scan completed successfully" }
        }]
      }
    }
  ]
}
```

### Test 3: Calibrate Scanner

```bash
curl -X POST http://localhost:5000/Device/test-001/$calibrate
```

### Test 4: Eject Film

```bash
curl -X POST http://localhost:5000/Device/test-001/$eject
```

### Test 5: From Web Browser

Open `http://localhost:5000/swagger` to access the interactive API documentation.

### Test 6: Integration with wingate.case.edu

1. Run the GUI or API server
2. Open `https://wingate.case.edu/bfd9000/` in your browser
3. Use the web interface to scan
4. The web application will make CORS-enabled requests to your local API

## Troubleshooting

### Scanner Not Found

**Symptoms:**
- "Scanner not initialized" error
- GUI shows "Error" status
- Device endpoint returns 500 error

**Solutions:**
1. Check USB connection
2. Verify scanner is powered on
3. Try running the CLI application first to test connectivity
4. Check Windows Device Manager for USB device
5. Verify Vscsi32.dll is present in the application directory

### API Not Starting

**Symptoms:**
- Application exits immediately
- Port 5000 already in use error

**Solutions:**
1. Check if another application is using port 5000
2. Close other instances of the API server
3. Try running as Administrator
4. Check Windows Firewall settings

### CORS Errors

**Symptoms:**
- Browser console shows CORS error
- Requests from wingate.case.edu are blocked

**Solutions:**
1. Verify API is running on `http://localhost:5000` (not HTTPS)
2. Check that requests are coming from `https://wingate.case.edu`
3. Ensure API server is running before accessing web interface
4. Check browser console for specific CORS error messages

### Scan Fails

**Symptoms:**
- API returns error status code
- OperationOutcome shows error severity
- No image data returned

**Solutions:**
1. Ensure film is properly loaded in scanner
2. Check scanner status in GUI window
3. Try calibrating the scanner first
4. Run CLI application to test scan manually
5. Check scan configuration in `scan_config.ini`

## Configuration

### Scan Parameters

Create a `scan_config.ini` file in the application directory to customize scan parameters:

```ini
[ScanParameters]
Offset0_BitDepth=12
Offset2_DPI_X=75
OutputPath=./scans
OutputPrefix=${DPI}DPI_${BIT}BIT
```

### API Port

To change the API port, edit `appsettings.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

### CORS Origins

To allow additional origins, modify `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("WingatePolicy", policy =>
    {
        policy.WithOrigins(
            "https://wingate.case.edu",
            "https://other-domain.com"  // Add more origins here
        )
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});
```

## Logging

### API Server Logs

The API server logs to the console. To enable detailed logging, edit `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Debug"
    }
  }
}
```

### CLI Application

The CLI application outputs directly to the console. Redirect to a file if needed:

```batch
bfd9010.exe > scanner.log 2>&1
```

## Deployment Checklist

- [ ] .NET 8.0 Runtime installed
- [ ] Scanner connected via USB
- [ ] Scanner powered on
- [ ] Vscsi32.dll present in application directory
- [ ] Port 5000 available
- [ ] Windows Firewall allows application
- [ ] Application has permissions to access USB device
- [ ] scan_config.ini configured (optional)
- [ ] Application tested with CLI
- [ ] API endpoints tested with curl or browser
- [ ] Integration tested with wingate.case.edu

## Building for Distribution

### Create Release Package

1. Build in Release configuration:
   ```batch
   build.bat
   ```

2. Gather files:
   - `BFD9010.Cli/bin/Release/net8.0/*` (bfd9010.exe and dependencies)
   - `BFD9010.FhirApi/bin/Release/net8.0/*` (BFD9010.FhirApi.exe and dependencies)
   - `BFD9010.Gui/bin/Release/net8.0-windows/*` (bfd9010_fhir32.exe and dependencies)
   - Helper scripts (build.bat, start-*.bat)
   - Documentation (README.md, API_README.md)

3. Create ZIP file with organized structure:
   ```
   BFD9010-v0.2.0/
   ├── bfd9010.exe (CLI)
   ├── BFD9010.FhirApi.exe (API Server)
   ├── bfd9010_fhir32.exe (GUI)
   ├── [all DLL dependencies]
   ├── start-cli.bat
   ├── start-fhir-api.bat
   ├── start-gui.bat
   ├── README.md
   └── API_README.md
   ```

### GitHub Release

1. Create a new release on GitHub
2. Tag with version (e.g., `v0.2.0`)
3. Upload the ZIP file
4. Include release notes with:
   - Features
   - Known issues
   - Installation instructions
   - Link to documentation

## Support

For issues or questions:
- Open an issue on GitHub
- Check existing issues for solutions
- Review API_README.md for detailed API documentation
- Check RE_Writeup.md for technical background

## Next Steps

After successful deployment and testing:
1. Gather user feedback
2. Monitor for errors and edge cases
3. Plan additional features (see API_README.md "Future Enhancements")
4. Consider packaging as Windows Installer (.msi)
5. Create automated tests for continuous integration
