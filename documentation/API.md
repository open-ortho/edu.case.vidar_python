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

## Build, Run, and Configuration
Build, run, packaging, and configuration instructions live in `documentation/BUILD_AND_RUN.md`. To avoid duplication, use that document for:
- Build commands and prerequisites
- Packaging (`package.bat`) and release ZIP contents
- CLI/GUI run commands and `--config` usage
- `scan_config.ini` structure and CORS settings

## FHIR API Endpoints

- `GET /Device/{id}` - Get scanner information
- `POST /Device/{id}/$scan` - Perform scan, returns PNG image as base64 in FHIR Bundle
- `POST /Device/{id}/$calibrate` - Calibrate scanner
- `POST /Device/{id}/$eject` - Eject film

See [FHIR Device Resource](https://hl7.org/fhir/device.html) for detailed documentation of the FHIR standard for the Device.
