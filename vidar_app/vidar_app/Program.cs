using vidar_app;
using static vidar_app.Scanner;

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
        Console.WriteLine("\nWelcome to Vidar Scanner Console!");
        Console.WriteLine("-------------------------------------------------------------");
        Console.WriteLine("Available commands (press the key):");
        Console.WriteLine("  [C]alibrate  - Calibrate the digitizer");
        Console.WriteLine("  [E]ject      - Eject the film from the digitizer");
        Console.WriteLine("  [S]can       - Initiate a scan using the digitizer");
        Console.WriteLine("  [R]estart    - Re-detect scanner and re-initialize");
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
                int scanStatus = Scan.scan(digitizerInfo, scanner_data);
                if (scanStatus != 0)
                {
                    Console.WriteLine($"ERROR: Scan failed with status code: {scanStatus}");
                }
                break;
            case ConsoleKey.R:
                Console.WriteLine("Restarting and re-detecting scanner...");
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