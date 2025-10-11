using BFD9010.FhirApi;
using BFD9010.Scanner;
using System.Diagnostics;
using System.Reflection;
using static BFD9010.Scanner.Scanner;

// Parse command-line args for optional --config <path>
string? configPath = null;
var cmdArgs = Environment.GetCommandLineArgs();
for (int i = 1; i < cmdArgs.Length; i++)
{
    if (cmdArgs[i].Equals("--config", StringComparison.OrdinalIgnoreCase) && i + 1 < cmdArgs.Length)
    {
        configPath = cmdArgs[i + 1];
        i++; // skip next
    }
}

// Resolve version once at startup using assembly metadata
string appVersion = GetAssemblyVersion() ?? "unknown";

// Main application loop. Handles restart and quit logic.
while (true)
{
    // Run the scanner initialization and command menu.
    bool restartRequested = await RunAppAsync();
    if (!restartRequested)
    {
        break; // Quit the application
    }
}

// Runs the scanner initialization and command menu.
// Returns true if restart is requested, false if quit.
async Task<bool> RunAppAsync()
{
    Console.WriteLine("Looking for Scanner...");

    // Load scan configuration from file (creates default if not exists)
    ScanConfig scanConfig = ScanConfig.Load(configPath);

    // Create scanner data and digitizer info objects
    ScannerData scanner_data = new ScannerData();
    BFD9010.Scanner.VscsiTypes._DIGITIZERINFO digitizerInfo = new BFD9010.Scanner.VscsiTypes._DIGITIZERINFO();

    //parses readable data out of returned DIGITIZERINFO, prints it and returns a populated scanner_data struct.
    int status = Startup.InitializeDigitizer(ref digitizerInfo, ref scanner_data);
    if (status != 0)
    {
        return false;
    }

    while (true)
    {
        Console.WriteLine();
        Console.WriteLine($"Welcome to BFD9010 Scanner Console!  (version: {appVersion})");
        Console.WriteLine("-------------------------------------------------------------");
        Console.WriteLine("Available commands (press the key):");
        Console.WriteLine("  [C]alibrate  - Calibrate the digitizer");
        Console.WriteLine("  [E]ject      - Eject the film from the digitizer");
        Console.WriteLine("  [S]can       - Initiate a scan using parameters from scan_config.ini");
        Console.WriteLine("  [F]HIR API   - Start FHIR REST API server on http://localhost:5000");
        Console.WriteLine("  [R]estart    - Re-detect scanner and reload configuration");
        Console.WriteLine("  [Q]uit       - Exit the application");
        Console.WriteLine("-------------------------------------------------------------");

        Console.Write("Enter a command: ");
        var key = Console.ReadKey(true).Key;
        Console.WriteLine();
        switch (key)
        {
            case ConsoleKey.C:
                Calibrate.calibrate();
                break;
            case ConsoleKey.E:
                Eject.eject(digitizerInfo);
                break;
            case ConsoleKey.S:
                int scanStatus = Scan.scan(digitizerInfo, scanner_data, scanConfig);
                if (scanStatus != 0)
                {
                    Console.WriteLine($"ERROR: Scan failed with status code: {scanStatus}");
                }
                break;
            case ConsoleKey.F:
                await StartFhirApiServerAsync(configPath);
                break;
            case ConsoleKey.R:
                Console.WriteLine("Restarting, re-detecting scanner, and reloading configuration...");
                return true; // Signal restart
            case ConsoleKey.Q:
                Console.WriteLine("");
                Console.WriteLine("Shutting Down");
                return false; // Signal quit
            default:
                Console.WriteLine("Unknown Command");
                break;
        }
    }
}

async Task StartFhirApiServerAsync(string? configPath)
{
    Console.WriteLine("\n=================================================================");
    Console.WriteLine("Starting FHIR API Server...");
    Console.WriteLine("=================================================================");

    try
    {
        // Load configuration to display info
        var config = ScanConfig.Load(configPath);

        // Create and start the server host (use await using for async disposal)
        await using var serverHost = new FhirServerHost(configPath);
        
        Console.WriteLine("\nInitializing scanner for FHIR API...");
        bool initialized = await serverHost.StartAsync();

        if (!initialized)
        {
            Console.WriteLine("ERROR: Failed to initialize scanner for FHIR API");
            Console.WriteLine("Press any key to return to menu...");
            Console.ReadKey(true);
            return;
        }

        Console.WriteLine("\n✓ Scanner initialized successfully!");
        Console.WriteLine($"\nCORS configured for: {config.CorsOrigin}");
        Console.WriteLine("\n=================================================================");
        Console.WriteLine("FHIR API Server is running on http://localhost:5000");
        Console.WriteLine("=================================================================");
        Console.WriteLine("\nAvailable endpoints:");
        Console.WriteLine("  GET  /Device/{id}         - Get scanner information");
        Console.WriteLine("  POST /Device/{id}/$scan   - Perform scan");
        Console.WriteLine("  POST /Device/{id}/$calibrate - Calibrate scanner");
        Console.WriteLine("  POST /Device/{id}/$eject  - Eject film");
        Console.WriteLine($"\nWeb application: {config.WebAppUrl}");
        Console.WriteLine("\nPress Ctrl+C to stop the server and return to menu...");
        Console.WriteLine("=================================================================\n");

        // Wait for Ctrl+C
        var cancellationSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cancellationSource.Cancel();
        };

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationSource.Token);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("\nShutting down server...");
        }

        // Server will be automatically stopped and disposed asynchronously when exiting the await using block
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nERROR: Failed to start FHIR API server: {ex.Message}");
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey(true);
    }
}

static string? GetAssemblyVersion()
{
    var entry = Assembly.GetEntryAssembly();
    if (entry == null) return null;

    // Prefer AssemblyInformationalVersion
    var infoAttr = entry.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    if (!string.IsNullOrWhiteSpace(infoAttr))
    {
        // Remove build metadata (everything after '+')
        var plusIndex = infoAttr.IndexOf('+');
        if (plusIndex >= 0)
            infoAttr = infoAttr.Substring(0, plusIndex);
        
        return infoAttr;
    }

    // Next prefer product/file version
    try
    {
        var fileVer = FileVersionInfo.GetVersionInfo(entry.Location).ProductVersion;
        if (!string.IsNullOrWhiteSpace(fileVer))
        {
            // Remove build metadata from file version too
            var plusIndex = fileVer.IndexOf('+');
            if (plusIndex >= 0)
                fileVer = fileVer.Substring(0, plusIndex);
            
            return fileVer;
        }
    }
    catch { }

    // Fallback to assembly name version
    return entry.GetName().Version?.ToString();
}