using System.Reflection;
using System.Diagnostics;
using vidar_app;
using static vidar_app.Scanner;

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
    bool restartRequested = RunApp();
    if (!restartRequested)
    {
        break; // Quit the application
    }
}

// Runs the scanner initialization and command menu.
// Returns true if restart is requested, false if quit.
bool RunApp()
{
    Console.WriteLine("Looking for Scanner...");

    // Load scan configuration from file (creates default if not exists)
    ScanConfig scanConfig = ScanConfig.Load(configPath);

    // Create scanner data and digitizer info objects
    ScannerData scanner_data = new ScannerData();
    VscsiTypes._DIGITIZERINFO digitizerInfo = new VscsiTypes._DIGITIZERINFO();

    //parses readable data out of returned DIGITIZERINFO, prints it and returns a populated scanner_data struct.
    int status = Startup.InitializeDigitizer(ref digitizerInfo, ref scanner_data);
    if (status != 0)
    {
        return false;
    }

    while (true)
    {
        Console.WriteLine();
        Console.WriteLine($"Welcome to Vidar Scanner Console!  (version: {appVersion})");
        Console.WriteLine("-------------------------------------------------------------");
        Console.WriteLine("Available commands (press the key):");
        Console.WriteLine("  [C]alibrate  - Calibrate the digitizer");
        Console.WriteLine("  [E]ject      - Eject the film from the digitizer");
        Console.WriteLine("  [S]can       - Initiate a scan using parameters from scan_config.ini");
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

static string? GetAssemblyVersion()
{
    var entry = Assembly.GetEntryAssembly();
    if (entry == null) return null;

    // Prefer AssemblyInformationalVersion
    var infoAttr = entry.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    if (!string.IsNullOrWhiteSpace(infoAttr)) return infoAttr;

    // Next prefer product/file version
    try
    {
        var fileVer = FileVersionInfo.GetVersionInfo(entry.Location).ProductVersion;
        if (!string.IsNullOrWhiteSpace(fileVer)) return fileVer;
    }
    catch { }

    // Fallback to assembly name version
    return entry.GetName().Version?.ToString();
}