using vidar_app;
using static vidar_app.Scanner;

Console.WriteLine("Looking for Scanner...");

ScannerData scanner_data = new ScannerData();
VscsiTypes._DIGITIZERINFO digitizerInfo = new VscsiTypes._DIGITIZERINFO();

//parses readable data out of returned DIGITIZERINFO, prints it and returns a populated scanner_data struct.
int status = Startup.InitializeDigitizer(ref digitizerInfo, ref scanner_data);

if (status != 0)
{
    return;
}

bool running = true;
while (running)
{
    Console.WriteLine("\nWelcome to Vidar Scanner Console!");
    Console.WriteLine("-------------------------------------------------------------");
    Console.WriteLine("Available commands (press the key):");
    Console.WriteLine("  [C]alibrate  - Calibrate the digitizer");
    Console.WriteLine("  [E]ject      - Eject the film from the digitizer");
    Console.WriteLine("  [S]can       - Initiate a scan using the digitizer");
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
            Scan.scan(digitizerInfo, scanner_data);
            break;
        case ConsoleKey.Q:
            running = false;
            break;
        default:
            Console.WriteLine("Unknown Command");
            break;
    }
}

Console.WriteLine("");
Console.WriteLine("Shutting Down");