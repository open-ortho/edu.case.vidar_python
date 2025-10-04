# Vidar App

Vidar App is a .NET 8 console application for interacting with Vidar digitizer/scanner hardware. It provides commands for calibration, ejecting film, and scanning.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows OS (required for native DLLs)
- `Vscsi32.dll` must be available in the application directory or in your system PATH

### Getting `Vscsi32.dll`

1. **Install Vidar Driver/Software:**
   - Download and install the official Vidar scanner driver/software package from Vidar or your hardware provider.
   - The required DLL (`Vscsi32.dll`) is typically installed with the Vidar TWAIN or SCSI driver package.

2. **Locate the DLL:**
   - After installation, you can usually find `Vscsi32.dll` in the Vidar installation directory, `C:\Program Files (x86)\VIDAR\Driver\Vscsi32.dll`

3. **Copy the DLL:**
   - Copy `Vscsi32.dll` to this directory.

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

### Using Command Line

```
dotnet run --project vidar_app/vidar_app.csproj
```

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
   - The script requires the .NET SDK and PowerShell's `Compress-Archive` cmdlet (PowerShell 5+), and it expects `Vscsi32.dll` to be present either in the current directory or in `C:\Program Files (x86)\VIDAR\Driver\Vscsi32.dll`.

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
- `[E]` Eject       – Ejects the film from the digitizer. The `Eject` command receives the detected digitizer info.
- `[S]` Scan        – Initiates a scan using default settings (300 DPI, 16-bit depth).
- `[P]` Parameters  – Scan with custom DPI and bit depth parameters (for testing different settings).
- `[R]` Restart     – Re-detects and re-initializes the scanner (returns to detection step).
- `[Q]` Quit        – Exit the application.

Example: press the `S` key to start a scan with default settings. If a command fails, the program prints an error code to the console.

### Custom Scan Parameters

The `[P]` Parameters option allows you to test different scan settings to find the optimal configuration for your scanner. When you select this option:

1. You'll be prompted to enter a DPI value (default: 300)
2. You'll be prompted to enter a bit depth (8 or 16, default: 16)
3. The scan will proceed with your custom values

This is useful for:
- Testing different resolutions to find the best quality/speed trade-off
- Troubleshooting image distortion or sizing issues
- Experimenting with scanner capabilities

**Common DPI values to try:** 75, 150, 200, 300, 600
**Bit depth options:** 8-bit (grayscale) or 16-bit (higher quality grayscale)

### Default Scan Settings

The default scan settings (used with `[S]` option) are:
- **Resolution:** 300 DPI
- **Bit Depth:** 16-bit
- These provide a good balance of quality and file size for most radiographic film scanning applications.

## Reverse engineering write-up

A write-up describing how the Vidar driver and protocols were reverse engineered is available at `./RE_Writeup.md`. That document outlines the steps, tools, and observations used to understand the `Vscsi32.dll` behavior and the scanner communication.

## Notes

- Ensure `Vscsi32.dll` is present and accessible.
- Administrative privileges may be required for hardware access.
- For troubleshooting, check console output for error codes.

## Troubleshooting

- If you see errors about missing DLLs, ensure `Vscsi32.dll` is in the same directory as the executable or in your system PATH.
- If you encounter permission errors, try running the application as administrator.
- For build issues, verify that .NET 8 SDK is installed and your environment variables are set correctly.

## Support

For further assistance, please refer to the project documentation or contact the repository maintainer.
