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
   - The required DLL (`Vscsi32.dll`) is typically installed with the Vidar TWAIN or SCSI driver package.

1. Ensure you have the [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed
2. Open a Command Prompt in the project directory (where `BFD9010.sln` is located)
3. Run:
   ```
   build.bat
   ```
This will build all projects in Release configuration and prepare executables for both CLI and GUI.
   - Copy `Vscsi32.dll` to this directory.

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

**Note:** Add `artifacts/` to your `.gitignore` to exclude generated packages from version control.

## Development vs Production Workflows

Understanding the difference between development and production builds is essential for effective development and deployment.

### Development Mode (For Developers)

**Use development mode when:**
- Writing and testing code
- Debugging issues
- Testing API changes with local HTML files
- Iterating quickly without full builds

**How to run in development mode:**

1. **Using dotnet run** (recommended):
   ```bash
   cd BFD9010.Cli
   dotnet run
   ```
   or
   ```bash
   cd BFD9010.Gui
   dotnet run
   ```

2. **Using Visual Studio**:
   - Open `BFD9010.sln`
   - Set `BFD9010.Cli` or `BFD9010.Gui` as the startup project
   - Press **F5** to run with debugging, or **Ctrl+F5** to run without debugging

**Development mode features:**
- Environment is set to `Development` (via `launchSettings.json`)
- CORS allows **any origin** (including `file://` protocol for local HTML testing)
- Swagger UI is enabled at `http://localhost:5000/swagger`
- Detailed error messages and logging
- Hot reload capabilities (when using dotnet watch)
- No need for self-contained publishing

**Benefits:**
- ? Fast iteration - changes compile quickly
- ? Easy debugging with breakpoints
- ? Test HTML files directly from disk (`file://`)
- ? Detailed logs and error information
- ? Swagger documentation available

### Production Mode (For Deployment)

**Use production mode when:**
- Creating packages for end users
- Deploying to production environments
- Building final releases
- Testing deployment scenarios

**How to build for production:**

1. **Build only** (faster, requires .NET runtime on target):
   ```bash
   build.bat
   ```
   - Builds in Release configuration
   - Outputs to `bin\Release\net8.0\` folders
   - Requires .NET 8.0 Runtime on target machine

2. **Package for distribution** (recommended):
   ```bash
   package.bat
   ```
   - Builds self-contained executables
   - Includes .NET runtime (no installation needed)
   - Bundles all dependencies including `Vscsi32.dll`
   - Creates versioned ZIP files in `artifacts\` directory
   - Ready for distribution to end users

**How to run production builds:**

After building with `build.bat`:
```bash
start-cli.bat
```
or
```bash
start-gui.bat
```

After packaging with `package.bat`:
- Extract the ZIP file from `artifacts\` directory
- Run the executable directly (no installation needed)

**Production mode features:**
- Environment is set to `Production`
- CORS restricted to configured origins only (see `scan_config.ini`)
- Swagger UI disabled for security
- Optimized binaries with better performance
- Self-contained deployment (when using `package.bat`)

**Benefits:**
- ? Enhanced security (CORS restrictions, no Swagger)
- ? Optimized performance
- ? Self-contained packages (no .NET installation required)
- ? Versioned releases
- ? Ready for end-user deployment

### Key Differences Summary

| Aspect | Development | Production |
|--------|-------------|------------|
| **Environment** | `Development` | `Production` |
| **CORS Policy** | Allow any origin (incl. `file://`) | Restricted to configured origins |
| **Swagger UI** | ? Enabled | ? Disabled |
| **Error Details** | Verbose | Minimal |
| **Build Command** | `dotnet run` | `build.bat` or `package.bat` |
| **Run Command** | `dotnet run` or F5 | `start-cli.bat` / `start-gui.bat` |
| **.NET Runtime** | Uses installed SDK | Included (with `package.bat`) |
| **CORS Testing** | Can use local HTML files | Requires proper web server |

### Workflow Examples

**Typical Development Workflow:**
```bash
# 1. Make code changes in your editor/IDE
# 2. Run in development mode
cd BFD9010.Cli
dotnet run

# 3. Test with browser (file:// or http://localhost)
# 4. Iterate - make changes and re-run
```

**Typical Production Workflow:**
```bash
# 1. Finalize and test all code changes
# 2. Update version in Directory.Build.props
# 3. Build and package
package.bat

# 4. Test the production package
cd artifacts
# Extract bfd9010_cli_v1.0.0.zip or bfd9010_gui_v1.0.0.zip
# Run the extracted executable

# 5. Distribute ZIP file to end users
```

### Testing Web Integration

**In Development:**
```bash
# 1. Start the API server in development mode
cd BFD9010.Cli
dotnet run

# 2. Open test_scanner_api.html directly in browser
# File can be opened via file:// protocol - CORS will allow it
```

**In Production:**
```bash
# 1. Package the application
package.bat

# 2. Configure CORS in scan_config.ini
CorsOrigin = https://yourdomain.com

# 3. Start the production build
start-gui.bat

# 4. Access from configured web origin only
# file:// protocol will NOT work in production
```

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

**Note:** In Development mode, CORS restrictions are relaxed to allow any origin for easier testing. In Production mode, only configured origins are allowed.

This is essential for web-based scanner control. The web application at the specified origin(s) can make API calls to `http://localhost:5000`.
