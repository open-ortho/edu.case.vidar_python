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

## Usage

When started, the app will prompt:

```
Enter a command:
```

Available commands:

- `calibrate` — Calibrates the digitizer.
- `eject` — Ejects the film from the digitizer.
- `scan` — Initiates a scan using the digitizer.

Type your command and press Enter.

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
