<img src="./documentation/images/BFD9000_logo_white.png" alt="BFD9010" width="300">

# BFD9010: An HL7 FHIR API for Scanners

- [BFD9010: An HL7 FHIR API for Scanners](#bfd9010-an-hl7-fhir-api-for-scanners)
  - [Quick Start](#quick-start)
    - [Option 1: GUI Application (Recommended for Web Integration)](#option-1-gui-application-recommended-for-web-integration)
    - [Option 2: CLI Application](#option-2-cli-application)
  - [CLI Usage](#cli-usage)
  - [Documentation](#documentation)
    - [Configuration File Structure](#configuration-file-structure)
    - [Configuration Options](#configuration-options)

This software controls the Vidar professional scanners leveraging the proprietary 32-bit Windows DLL and exposes a local FHIR API for web-based scanning.

**Development and deployment require Windows** due to the 32-bit DLL dependency for scanner hardware access. Non-Windows platforms can build and run unit tests only.

## Quick Start

Requires windows. Refer to the [Deployment Checklist](documentation/DEPLOYMENT.md) for installing on a fresh workstation.

1. Download the latest release from GitHub.
2. Run `bfd9010.exe` (GUI application).
   - The scanner will initialize automatically.
   - A small window will appear showing status.
   - The FHIR API server starts on `http://localhost:5000`.
3. Navigate to `https://wingate.case.edu/bfd9000/` to scan.

### Option 1: GUI Application (Recommended for Web Integration)

``` powershell
start-gui.bat
```

- Launches the Windows Forms GUI
- Automatically starts FHIR API server on <http://localhost:5000>
- Displays scanner status in a small always-on-top window
- Shows a clickable link to the web application (configured via `WebAppUrl` in INI file)
- Best for production use with web-based scanner control

### Option 2: CLI Application

``` powershell
start-cli.bat
```

- Launches the interactive command-line interface
- Provides manual control options via keyboard menu:
  - `[C]` Calibrate - Calibrate the digitizer
  - `[E]` Eject - Eject the film from the digitizer
  - `[S]` Scan - Initiate a scan using parameters from scan_config.ini
  - `[F]` FHIR API - Start FHIR REST API server on <http://localhost:5000>
  - `[R]` Restart - Re-detect scanner and reload configuration
  - `[Q]` Quit - Exit the application
- Best for testing and manual scanner control

**When to use CLI:**

- Debugging scanner connectivity or status
- Manual calibration/eject operations
- Verifying scan output without the web UI

**Command-line options (both CLI and GUI):**

``` powershell
bfd9010_cli.exe --config path\to\config.ini
bfd9010.exe --config path\to\config.ini
```

## CLI Usage

Run `bfd9010_cli.exe` for the interactive command-line interface.

## Documentation

- Build, run, packaging, configuration: `documentation/BUILD_AND_RUN.md`
- FHIR API endpoints: `documentation/API.md`
- Deployment checklist: `documentation/DEPLOYMENT.md`
- Multi-scanner architecture: `documentation/MULTI_SCANNER_ARCHITECTURE.md`
- Reverse engineering notes: `documentation/vidar/RE_Writeup.md`

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
