# BFD9010 FHIR API

This directory contains the FHIR-compliant REST API library for the BFD9010 scanner system.

## Architecture

The project is organized into these components:

1. **BFD9010.Scanner** - Shared library containing all scanner operations
2. **bfd9010** (BFD9010.Cli) - Command-line interface application with FHIR API server option
3. **BFD9010.FhirApi** - Shared FHIR REST API library (no standalone executable)
4. **BFD9010.Gui** - Windows Forms GUI application (Windows only)

### Architecture Changes

The BFD9010.FhirApi project is now a **class library** that provides shared FHIR API functionality. It is used by:
- **CLI application** (BFD9010.Cli) - via the `[F]HIR API` command
- **GUI application** (BFD9010.Gui) - automatically started on launch

This ensures consistent FHIR API behavior across all entry points without code duplication.

## Building

### Prerequisites
- .NET 8.0 SDK
- Windows OS (for GUI and scanner hardware access)

### Build Commands

```bash
# Build all projects
dotnet build BFD9010.sln --configuration Release

# Build individual projects
dotnet build BFD9010.Scanner/BFD9010.Scanner.csproj --configuration Release
dotnet build BFD9010.FhirApi/BFD9010.FhirApi.csproj --configuration Release
dotnet build BFD9010.Cli/BFD9010.Cli.csproj --configuration Release

# Build GUI project (requires publish for executable)
dotnet publish BFD9010.Gui/BFD9010.Gui.csproj --configuration Release
```

### Output Locations
- CLI: `BFD9010.Cli/bin/Release/net8.0/bfd9010.exe`
- GUI: `BFD9010.Gui/bin/Release/net8.0-windows/publish/bfd9010_fhir32.exe`
- FHIR API Library: `BFD9010.FhirApi/bin/Release/net8.0/BFD9010.FhirApi.dll`

**Note:** The GUI project requires `dotnet publish` to generate the executable. The FHIR API is now a library (DLL) only.

## Running

### CLI Application
```bash
cd BFD9010.Cli/bin/Release/net8.0
./bfd9010.exe [--config path/to/config.ini]
```

The CLI provides these commands:
- **[C]alibrate** - Calibrate the digitizer
- **[E]ject** - Eject the film from the digitizer
- **[S]can** - Initiate a scan using parameters from scan_config.ini
- **[F]HIR API** - Start FHIR REST API server on http://localhost:5000
- **[R]estart** - Re-detect scanner and reload configuration
- **[Q]uit** - Exit the application

**Command-line options:**
- `--config <path>` - Specify a custom configuration file path (default: `scan_config.ini` in executable directory)

### GUI Application (Windows only)
```bash
cd BFD9010.Gui/bin/Release/net8.0-windows/publish
./bfd9010_fhir32.exe [--config path/to/config.ini]
```

The GUI will:
- Initialize the scanner on startup
- Start the FHIR API server on port 5000
- Display scanner status in a small always-on-top window
- Show a clickable link to the web application (configured via `WebAppUrl` in INI file)
- Display API endpoint information

**Command-line options:**
- `--config <path>` - Specify a custom configuration file path (default: `scan_config.ini` in executable directory)

## Scanner Configuration

Scanner settings are loaded from `scan_config.ini` in the working directory. If the file doesn't exist, default values will be used and a new file will be created.

You can specify a custom configuration file using the `--config` command-line argument:
```bash
./bfd9010.exe --config /path/to/custom_config.ini
./bfd9010_fhir32.exe --config C:\Configs\scanner_config.ini
```

### Configuration File Structure

Example `scan_config.ini`:
```ini
[ScanParameters]

# Bit depth (8 or 16)
BitDepth = 16

# DPI resolution (tested DPIs: 75, 150, 300)
DPI = 300

# Output options
# OutputPath: directory where image files will be written
OutputPath = ~\Desktop\VidarScans

# OutputPrefix: filename prefix template. Tokens: ${DPI}, ${BIT}
OutputPrefix = ${DPI}DPI_${BIT}BIT

# Web API settings
# WebAppUrl: URL of the web application users should navigate to for scanning
WebAppUrl = https://wingate.case.edu/bfd9000/

# CorsOrigin: CORS origin(s) to allow API access from (comma-separated for multiple)
CorsOrigin = https://wingate.case.edu
```

### Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `BitDepth` | integer | 16 | Scan bit depth (8 or 16) |
| `DPI` | integer | 300 | Scan resolution in dots per inch (tested: 75, 150, 300) |
| `OutputPath` | string | `~\Desktop\VidarScans` | Directory for saving scanned images (supports ~ for home directory) |
| `OutputPrefix` | string | `${DPI}DPI_${BIT}BIT` | Filename prefix template (tokens: ${DPI}, ${BIT}) |
| `WebAppUrl` | string | `https://wingate.case.edu/bfd9000/` | URL displayed in GUI for users to access web interface |
| `CorsOrigin` | string | `https://wingate.case.edu` | Allowed CORS origin(s), comma-separated for multiple origins |

### CORS Configuration

The `CorsOrigin` setting controls which web origins can make API requests to the scanner. 

**Single origin:**
```ini
CorsOrigin = https://wingate.case.edu
```

**Multiple origins:**
```ini
CorsOrigin = https://wingate.case.edu, https://localhost:3000, https://test.example.com
```

This is essential for web-based scanner control. The web application at the specified origin(s) can make API calls to `http://localhost:5000`.
