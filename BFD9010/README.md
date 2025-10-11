# BFD9010 Scanner Software

This repository contains the C# implementation of the BFD9010 scanner control software, providing both CLI and GUI interfaces with integrated FHIR API support.

## Project Overview

The BFD9010 scanner software is built on .NET 8.0 and provides:
- **Command-Line Interface (CLI)** - Interactive menu-driven scanner control with optional FHIR API server
- **Graphical User Interface (GUI)** - Windows Forms application with automatic FHIR API server startup
- **FHIR REST API** - Shared library providing FHIR-compliant REST API for web-based scanner control
- **Scanner Library** - Core scanner communication and control functionality

## Quick Start

### Building the Project

1. Ensure you have the [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed
2. Open a Command Prompt in the project directory (where `BFD9010.sln` is located)
3. Run:
   ```
   build.bat
   ```

This will build all projects in Release configuration and prepare executables for both CLI and GUI.

**Output locations:**
- CLI: `BFD9010.Cli\bin\Release\net8.0\bfd9010.exe`
- GUI: `BFD9010.Gui\bin\Release\net8.0-windows\publish\bfd9010_fhir32.exe`
- Libraries: `BFD9010.Scanner\bin\Release\net8.0\*.dll` and `BFD9010.FhirApi\bin\Release\net8.0\*.dll`

### Running the Application

**Option 1: GUI Application (Recommended for Web Integration)**
```
start-gui.bat
```
- Launches the Windows Forms GUI
- Automatically starts FHIR API server on http://localhost:5000
- Displays scanner status in a small always-on-top window
- Shows a clickable link to the web application (configured via `WebAppUrl` in INI file)
- Best for production use with web-based scanner control

**Option 2: CLI Application**
```
start-cli.bat
```
- Launches the interactive command-line interface
- Provides manual control options via keyboard menu:
  - `[C]` Calibrate - Calibrate the digitizer
  - `[E]` Eject - Eject the film from the digitizer
  - `[S]` Scan - Initiate a scan using parameters from scan_config.ini
  - `[F]` FHIR API - Start FHIR REST API server on http://localhost:5000
  - `[R]` Restart - Re-detect scanner and reload configuration
  - `[Q]` Quit - Exit the application
- Best for testing and manual scanner control

**Command-line options (both CLI and GUI):**
```
bfd9010.exe --config path\to\config.ini
bfd9010_fhir32.exe --config path\to\config.ini
```

### Creating Distribution Packages

To create self-contained ZIP packages for distribution:
```
package.bat
```

This will:
- Build both CLI and GUI as self-contained executables (no .NET runtime required on target machine)
- Include all dependencies including Vscsi32.dll scanner driver
- Create ZIP files in the `artifacts\` directory:
  - `bfd9010_cli_v<version>.zip` - CLI package
  - `bfd9010_gui_v<version>.zip` - GUI package

Each package includes:
- Self-contained executable (no .NET installation required)
- All dependencies
- Vscsi32.dll (scanner driver)
- scan_config.ini (sample configuration)

**Note:** Add `artifacts/` to your `.gitignore` to exclude generated packages from version control.

## Batch File Reference

| File | Purpose | When to Use |
|------|---------|-------------|
| `build.bat` | Builds all projects in Release configuration | After code changes, before running or packaging |
| `start-cli.bat` | Launches CLI application | For manual scanner control and testing |
| `start-gui.bat` | Launches GUI application with auto-started API | For production use with web-based scanner control |
| `package.bat` | Creates distribution ZIP packages | When preparing software for deployment |

## Scanner Configuration

Scanner settings are loaded from `scan_config.ini` in the working directory. If the file doesn't exist, default values will be used and a new file will be created.

You can specify a custom configuration file using the `--config` command-line argument:
```bash
bfd9010.exe --config /path/to/custom_config.ini
bfd9010_fhir32.exe --config C:\Configs\scanner_config.ini
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

**To test different scan settings:**
