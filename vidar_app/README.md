# Vidar App

Vidar App is a .NET 8 console application for interacting with Vidar digitizer/scanner hardware. It provides commands for calibration, ejecting film, and scanning.

## Prerequisites

- Windows OS with x86 32 bit support (required for native DLLs)
- The Vidar scanner driver / WinUSB binding must be installed on target machines. Typically this is provided by the Vidar driver package (INF/SYS/CAT). The driver must be installed/added (for example with `pnputil /add-driver <path-to-inf> /install`) and administrative privileges are required. On 64-bit Windows a properly signed driver is required for normal operation.
- `Vscsi32.dll` must be available in the application directory or in your system PATH (the packager attempts to copy this DLL into the release).

### Getting `Vscsi32.dll`

1. **Install Vidar Driver/Software:**
   - Download and install the official Vidar scanner driver/software package from Vidar or your hardware provider. The required DLL (`Vscsi32.dll`) is typically installed with the Vidar TWAIN or SCSI driver package.

2. **Locate the DLL:**
   - After installation, you can usually find `Vscsi32.dll` in the Vidar installation directory, `C:\Program Files (x86)\VIDAR\Driver\Vscsi32.dll`

3. **Copy the DLL:**
   - Copy `Vscsi32.dll` to this directory if you are building or running from source.

## Building

### Using Command Line

1. Open a terminal in the `vidar_app` directory.
2. Run the following command to build the application:

   ```
   dotnet build
   ```

### Using Visual Studio

1. Open the solution in Visual Studio 2022 or later.
2. Select **Build > Build Solution** from the menu.

## Running

After building, you can run the application using one of the following methods:

### Command-line usage

- The application accepts an optional `--config <path>` argument to specify the path to the `scan_config.ini` file.
  - Example (published EXE): `vidar_app.exe --config "C:\path\to\scan_config.ini"`
  - Example (dotnet run): `dotnet run -- --config "C:\path\to\scan_config.ini"`

- If `--config` is omitted the application defaults to the original behaviour: it looks for (and if missing creates) `scan_config.ini` next to the executable (same behavior as before).

- Note: the path given to `--config` is used as-is (relative paths are resolved by the process working directory). If you want paths relative to the executable, change the path to an absolute path or update the code to resolve relative to the exe directory.

### Using Visual Studio

- Press **F5** or select **Debug > Start Debugging** to run the app.

### Running the Executable

- Navigate to `bin/Debug/net8.0` or `bin/Release/net8.0` and run `vidar_app.exe`.

## Packaging (create a ZIP release)

A helper script `build-package.bat` is provided to publish the app and create a release ZIP.

How to run the packager:

1. Open Command Prompt (cmd.exe).
2. Change directory to this `vidar_app` folder (the folder that contains `build-package.bat`).
3. Run:

   ```
   build-package.bat
   ```

   Notes:
   - Run the script from `cmd.exe` (not PowerShell) to see full command echoing reliably. If you run it from PowerShell, PowerShell will invoke cmd to execute the batch file, but debugging output is clearer in a dedicated Command Prompt.
   - The script requires the .NET SDK to be present if you intend to run the `dotnet` build/publish steps locally. The packager will publish a self-contained release for the configured RID (see the script), and it attempts to include `Vscsi32.dll` from either the repo root or the Vidar driver install location.

Where to find the ZIP:

- The produced ZIP file will be placed in the `artifacts` directory under this project folder. The filename format is `vidar_app_release_v<version>.zip` (for example: `artifacts\vidar_app_release_v0.1.0.zip`).

Excluding artifacts from Git:

- You should add `artifacts/` to your `.gitignore` so generated packages are not checked in. If you prefer, add the following line to the repository `.gitignore`:

   ```
   artifacts/
   ```

## Usage

When started, the app shows a simple key-driven menu. Press the single letter key shown in brackets to run a command (the program reads a single key press, no Enter required):

- `[C]` Calibrate   – Calibrates the digitizer.
- `[E]` Eject       – Ejects the film from the digitizer.
- `[S]` Scan        – Initiates a scan using parameters from `scan_config.ini`.
- `[R]` Restart     – Re-detects and re-initializes the scanner, and reloads configuration from `scan_config.ini`.
- `[Q]` Quit        – Exit the application.

Example: press the `S` key to start a scan with the settings loaded from the config file. If a command fails, the program prints an error code to the console.

### Configuration File

The application uses a configuration file (`scan_config.ini`) to control scan parameters. On first run, if the file doesn't exist, it will be automatically created with default values (original settings from before the 300 DPI changes: 75 DPI, 8-bit depth).

The configuration file uses a simple INI format:

```ini
# Vidar Scanner Configuration
[ScanParameters]

# Bit depth (8 or 16)
BitDepth = 8

# DPI resolution (common values: 75, 150, 300)
DPI = 75

```

**To test different scan settings:**

1. Open `scan_config.ini` in a text editor
2. Modify the values you want to test (e.g., change `DPI` from 75 to 300)
3. Save the file
4. Press `[R]` in the application to restart and reload the configuration
5. Press `[S]` to scan with the new settings

This streamlined workflow allows you to quickly test different parameter combinations without manually entering values each time.

**Config File Location:** The config file is created in the same directory as the executable (typically `bin/Debug/net8.0/` or `bin/Release/net8.0/`). If you provide `--config <path>` the application will use that file instead.

## Reverse engineering write-up

A write-up describing how the Vidar driver and protocols were reverse engineered is available at `./RE_Writeup.md`. That document outlines the steps, tools, and observations used to understand the `Vscsi32.dll` behavior and the scanner communication.

## Notes

- Ensure `Vscsi32.dll` is present and accessible.
- Administrative privileges may be required for hardware access and driver installation.
- For troubleshooting, check console output for error codes.

## Troubleshooting

- If you see errors about missing DLLs, ensure `Vscsi32.dll` is in the same directory as the executable or in your system PATH.
- If you encounter permission errors, try running the application as administrator.
- For build issues, verify that .NET 8 SDK is installed and your environment variables are set correctly.

## Support

For further assistance, please refer to the project documentation or contact the repository maintainer.
